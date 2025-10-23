using core;
using core.Data;
using core.Embeddings;
using core.Summarization;
using core.VectorStorage;

class Program
{
    static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load(); // loads .env file

        if (args.Length < 2)
        {
            Console.WriteLine("\n\nWelcome to the Retrieval-Augmented Generation (RAG) app.");
            Console.WriteLine("Usage: dotnet run <source> <source-value>");
            Console.WriteLine("  Sources:");
            Console.WriteLine("    file <file-path>         - Load from a file (supports .txt, .pdf, .md, .csv, .json, .xml, .html, .log, etc.)");
            Console.WriteLine("    qa <file-path>           - Load Q&A pairs from a text file (separated by \\n--\\n)");
            Console.WriteLine("    github <repository-url>  - Load from GitHub repository");
            Console.WriteLine("    http <url>               - Load from HTML page");
            Console.WriteLine("    sitemap <sitemap-url>    - Load from sitemap (all URLs in parallel)");
            Console.WriteLine("Make sure to set the OPENAI_API_KEY environment variable.");
            return;
        }

        //var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
        var source = args[0];
        var sourceValue = args[1];

        var joyQueryClient = new JoyQueryClient(
            new OllamaEmbedder(
                model: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text",
                baseUrl: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434"
            ),
            new OllamaSummarizer(
                model: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "llama3.1:8b",
                baseUrl: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434"
            ),
            new QdrantVectorStorage(
                host: Environment.GetEnvironmentVariable("QDRANT_HOST") ?? "localhost",
                apiKey: Environment.GetEnvironmentVariable("QDRANT_API_KEY") ?? "",
                scoreThreshold: 0.7f // TODO: adjust threshold based on your content
            )
        );

        var textChunker = new core.Chunking.RecursiveTextChunker(
            chunkSize: int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_SIZE"), out var size) ? size : 500,
            overlap: int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_OVERLAP"), out var ovl) ? ovl : 50
        );

        IDataLoader dataLoader = source switch
        {
            "file" => new LocalFileDataLoader(textChunker, sourceValue),
            "github" => new GitHubDataLoader(sourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
            "http" => new HttpDataLoader(sourceValue),
            "sitemap" => new SitemapDataLoader(sourceValue),
            _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'faq', 'github', 'http', or 'sitemap'."),
        };

        var collectionName = Guid.TryParse(source, out _)
            ? source
            : await joyQueryClient.LoadDataAsync(dataLoader);

        while (true)
        {
            Console.Write("\n\nEnter your question (or press Enter to exit): ");
            var input = Console.ReadLine() ?? string.Empty;
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            var summary = await joyQueryClient.QueryAsync([collectionName], input);

            Console.WriteLine(summary);
        }
    }
}