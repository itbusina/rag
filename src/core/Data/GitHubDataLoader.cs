using core.AI;
using core.Helpers;
using core.Models;
using Octokit;

namespace core.Data
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
        private readonly string _repositoryUrl;
        private readonly string? _accessToken;
        private readonly string _owner = string.Empty;
        private readonly string _repoName = string.Empty;
        private readonly string _server = string.Empty;
        private readonly List<GitHubChunkMetadata> _allComments = [];

        public GitHubDataLoader(string repositoryUrl, string? accessToken = null)
        {
            var (server, org, repo) = GitHubUrlParser.Parse(repositoryUrl);

            _repositoryUrl = repositoryUrl;
            _accessToken = accessToken;
            _server = server;
            _owner = org;
            _repoName = repo;
        }

        public async Task LoadAsync()
        {
            try
            {
                Console.WriteLine($"Loading commit messages from {_owner}/{_repoName}...");

                var client = new GitHubClient(new ProductHeaderValue("RAG-GitHub-App"), new Uri(_server));

                // Prioritize constructor token, then check environment variable
                var githubToken = _accessToken ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                if (!string.IsNullOrEmpty(githubToken))
                {
                    client.Credentials = new Credentials(githubToken);
                    Console.WriteLine("Using authenticated GitHub API access (supports private repositories and higher rate limits)");
                }
                else
                {
                    Console.WriteLine("Warning: No GitHub token provided. Using unauthenticated access (public repositories only, lower rate limits)");
                }

                // Get all commits from the repository
                var commits = await client.Repository.Commit.GetAll(_owner, _repoName);
                Console.WriteLine($"Found {commits.Count} commits");

                var allCommits = commits
                    .Where(x => !string.IsNullOrWhiteSpace(x.Commit.Message))
                    .Select(commit => new GitHubChunkMetadata
                    {
                        Repository = _repoName,
                        Organization = _owner,
                        RepositoryUrl = _repositoryUrl,
                        CommitMessage = commit.Commit.Message,
                        CommitSha = commit.Sha,
                        Author = commit.Commit.Author.Name,
                        Date = commit.Commit.Author.Date.DateTime
                    })
                    .ToList();

                _allComments.AddRange(allCommits);

                Console.WriteLine($"Successfully loaded {_allComments.Count} commit messages from {commits.Count} commits");
            }
            catch (RateLimitExceededException ex)
            {
                Console.WriteLine($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
                Console.WriteLine("Consider adding a GITHUB_TOKEN environment variable for higher rate limits.");
                throw;
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine($"Error loading data from GitHub: {ex.Message}");
                Console.WriteLine($"Repository {_owner}/{_repoName} not found or access denied.");
                Console.WriteLine("If this is a private repository, ensure you've provided a valid GitHub access token.");
                throw;
            }
            catch (AuthorizationException ex)
            {
                Console.WriteLine($"Error loading data from GitHub: {ex.Message}");
                Console.WriteLine($"Authorization failed for repository {_owner}/{_repoName}.");
                Console.WriteLine("The provided token may be invalid or may not have the required permissions.");
                Console.WriteLine("For private repositories, ensure your token has 'repo' scope.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from GitHub: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Chunk>> GetContentChunks(IAIClient aIClient)
        {
            if (_allComments.Count == 0)
            {
                throw new InvalidOperationException("No content loaded. Call Load() before GetContentChunks().");
            }

            var splitter = new LangChain.Splitters.Text.RecursiveCharacterTextSplitter(
                                separators: ["\n\n", "\n", " ", ""],
                                chunkSize: 1000,
                                chunkOverlap: 200
                            );

            var chunks = new List<Chunk>();
            foreach (var comment in _allComments)
            {
                var commentChunks = splitter.SplitText(comment.CommitMessage);
                foreach (var commentChunk in commentChunks)
                {
                    var chunk = new Chunk
                    {
                        Content = commentChunk,
                        Type = DataSourceType.GitHub,
                        Value = _repositoryUrl,
                        Embedding = await aIClient.GetEmbeddingAsync(commentChunk),
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
                    };

                    chunks.Add(chunk);
                }

                Console.WriteLine($"Processing comment {_allComments.IndexOf(comment) + 1}/{_allComments.Count}, chunks in comment: {commentChunks.Count}");
            }

            return chunks;
        }
    }
}

