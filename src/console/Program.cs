using core.Data;
using core.Embeddings;
using core.Retrieving;
using core.Summarization;

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
            Console.WriteLine("    qa <file-path>           - Load Q&A pairs from a text file (separated by \\n--\\n)");
            Console.WriteLine("    github <repository-url>  - Load from GitHub repository");
            Console.WriteLine("    http <url>               - Load from HTML page");
            Console.WriteLine("    sitemap <sitemap-url>    - Load from sitemap (all URLs in parallel)");
            Console.WriteLine("Make sure to set the OPENAI_API_KEY environment variable.");
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
        var source = args[0];
        var sourceValue = args[1];

        var embedder = new OllamaEmbedder("nomic-embed-text"); // new OpenAIEmbedder("text-embedding-3-small", apiKey);
        var summarizer = new OllamaSummarizer("llama3.1:8b"); //new OpenAISummarizer("gpt-4.1-mini", apiKey);
        var retriever = new Retriever(embedder);

        IDataLoader dataLoader = source switch
        {
            "file" => new FileDataLoader(embedder, sourceValue),
            "qa" => new QADataLoader(embedder, sourceValue),
            "github" => new GitHubDataLoader(embedder, sourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
            "http" => new HttpDataLoader(embedder, sourceValue),
            "sitemap" => new SitemapDataLoader(embedder, sourceValue),
            _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'qa', 'github', 'http', or 'sitemap'."),
        };

        // Step 1. Load file content
        await dataLoader.LoadAsync();

        // Step 2: Load chunks for data source
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
            var topChunks = await retriever.GetTopKChunks(chunks, query, k: 3);

            // Step 6: Augment with context
            var summary = await summarizer.SummarizeAsync(query, topChunks);

            Console.WriteLine(summary);
        }
    }
}