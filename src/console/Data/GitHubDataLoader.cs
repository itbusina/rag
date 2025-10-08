using console.Embeddings;
using console.Models;
using Octokit;

namespace console.Data
{
    public class GitHubChunkMetadata
    {
        public required string Repository { get; set; }
        public required string Organization { get; set; }
        public required string RepositoryUrl { get; set; }
        public required string CommitMessage { get; set; }
        public required string CommitSha { get; set; }
        public required string Author { get; set; }
        public required DateTime Date { get; set; }
    }
    
    public class GitHubDataLoader : IDataLoader
    {
        private readonly IEmbedder _embedder;
        private readonly string _repositoryUrl;
        private string _owner = string.Empty;
        private string _repoName = string.Empty;
        private readonly List<GitHubChunkMetadata> _allComments = [];

        public GitHubDataLoader(IEmbedder embedder, string repositoryUrl)
        {
            _embedder = embedder;
            _repositoryUrl = repositoryUrl;
            ParseRepositoryUrl();
        }

        private void ParseRepositoryUrl()
        {
            // Parse GitHub URL like "https://github.com/owner/repo" or "github.com/owner/repo"
            var uri = _repositoryUrl.Replace("https://", "").Replace("http://", "").Replace("github.com/", "");
            var parts = uri.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException($"Invalid GitHub repository URL: {_repositoryUrl}. Expected format: https://github.com/owner/repo");
            }

            _owner = parts[0];
            _repoName = parts[1].Replace(".git", ""); // Remove .git suffix if present
        }

        public async Task LoadAsync()
        {
            try
            {
                Console.WriteLine($"Loading commit messages from {_owner}/{_repoName}...");

                var client = new GitHubClient(new ProductHeaderValue("RAG-GitHub-App"));

                // Check if GitHub token is available in environment
                var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                if (!string.IsNullOrEmpty(githubToken))
                {
                    client.Credentials = new Credentials(githubToken);
                    Console.WriteLine("Using authenticated GitHub API access (higher rate limits)");
                }
                else
                {
                    Console.WriteLine("Warning: No GITHUB_TOKEN found. Using unauthenticated access (lower rate limits)");
                }

                // Get all commits from the repository
                var commits = await client.Repository.Commit.GetAll(_owner, _repoName);
                Console.WriteLine($"Found {commits.Count} commits");

                int totalMessages = 0;

                // Iterate through each commit
                foreach (var commit in commits)
                {
                    var commitSha = commit.Sha;
                    var commitMessage = commit.Commit.Message;
                    var commitAuthor = commit.Commit.Author.Name;
                    var commitDate = commit.Commit.Author.Date;

                    Console.WriteLine($"Processing commit {commitSha.Substring(0, 7)}: {commitMessage}");

                    // Add commit message
                    if (!string.IsNullOrWhiteSpace(commitMessage))
                    {
                        _allComments.Add(new GitHubChunkMetadata
                        {
                            Repository = _repoName,
                            Organization = _owner,
                            RepositoryUrl = _repositoryUrl,
                            CommitMessage = commitMessage,
                            CommitSha = commitSha,
                            Author = commitAuthor,
                            Date = commitDate.DateTime
                        });

                        totalMessages++;
                    }
                }

                Console.WriteLine($"Successfully loaded {totalMessages} commit messages from {commits.Count} commits");
            }
            catch (RateLimitExceededException ex)
            {
                Console.WriteLine($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
                Console.WriteLine("Consider adding a GITHUB_TOKEN environment variable for higher rate limits.");
                throw;
            }
            catch (NotFoundException)
            {
                Console.WriteLine($"Repository {_owner}/{_repoName} not found or is not public.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from GitHub: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Chunk>> GetContentChunks()
        {
            if (_allComments.Count == 0)
            {
                throw new InvalidOperationException("No content loaded. Call Load() before GetContentChunks().");
            }

            var chunks = _allComments.Select(async comment => new Chunk
            {
                Content = comment.CommitMessage,
                Embedding = await _embedder.GetEmbedding(comment.CommitMessage),
                Metadata = new Dictionary<string, string>
                {
                    { "repository", comment.Repository },
                    { "organization", comment.Organization },
                    { "repository_url", comment.RepositoryUrl },
                    { "commit_url", $"{comment.RepositoryUrl}/commit/{comment.CommitSha}" },
                    { "commit_sha", comment.CommitSha },
                    { "author", comment.Author },
                    { "date", comment.Date.ToString("o") } // ISO 8601 format
                }
            }).ToList();

            await Task.WhenAll(chunks);

            return [.. chunks.Select(t => t.Result)];
        }
    }
}

