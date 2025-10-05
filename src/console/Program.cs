using console;
using console.Augmentation;
using console.Data;
using console.Embeddings;
using console.Retriving;
using OpenAI.Chat;

class Program
{
    static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load(); // loads .env file

        if(args.Length < 2)
        {
            Console.WriteLine("\n\nWelcome to the Retrieval-Augmented Generation (RAG) app.");
            Console.WriteLine("Usage: dotnet run <source> <source-value>");
            Console.WriteLine("  Sources:");
            Console.WriteLine("    file <file-path>         - Load from a file (supports .txt, .pdf, .md, .csv, .json, .xml, .html, .log, etc.)");
            Console.WriteLine("    github <repository-url>  - Load from GitHub repository");
            Console.WriteLine("    http <url>               - Load from HTML page");
            Console.WriteLine("    sitemap <sitemap-url>    - Load from sitemap (all URLs in parallel)");
            Console.WriteLine("Make sure to set the OPENAI_API_KEY environment variable.");
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
        var source = args[0];
        var sourceValue = args[1];

        var embedder = new OpenAIEmbedder("text-embedding-3-small", apiKey);

        // Option 1: Load data

        IDataLoader dataLoader = source switch
        {
            "file" => new FileDataLoader(embedder, sourceValue),
            "github" => new GitHubDataLoader(embedder, sourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
            "http" => new HttpDataLoader(embedder, sourceValue),
            "sitemap" => new SitemapDataLoader(embedder, sourceValue),
            _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'github', 'http', or 'sitemap'."),
        };

        var retriver = new Retriver(embedder);
        var augmenter = new OpenAIAugmenter("gpt-4.1-mini", apiKey);

        // Step 1. Load file content
        await dataLoader.LoadAsync();

        // Step 2: Chunk text
        var chunks = await dataLoader.GetContentChunks();

        // Step 4. Query
        while (true)
        {
            Console.Write("\n\nEnter your question (or press Enter to exit): ");
            var input = Console.ReadLine() ?? string.Empty;
            var query = input.Trim();

            if(string.IsNullOrEmpty(query))
            {
                break;
            }

            // Step 5. Retrieve top k findings
            var topChunks = await retriver.GetTopKChunks(chunks, query, k: 3);

            // Step 6: Augment with context
            var augmentedResponse = await augmenter.AugmentAsync(query, topChunks);

            Console.WriteLine(augmentedResponse);
        }
    }
}