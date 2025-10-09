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

        var embedder = new OllamaEmbedder("nomic-embed-text"); // new OpenAIEmbedder("text-embedding-3-small", apiKey);
        var summarizer = new OllamaSummarizer("llama3.1:8b"); //new OpenAISummarizer("gpt-4.1-mini", apiKey);
        var vectorStorage = new QdrantVectorStorage();

        string collectionName;
        if (!Guid.TryParse(source, out _))
        {
            // Load data to vectore storage
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

            // Step 3: Store chunks in vector storage
            collectionName = Guid.NewGuid().ToString();
            await vectorStorage.CreateCollectionAsync(collectionName, 768); // 768 is the dimension of the "nomic-embed-text" model
            await vectorStorage.InsertAsync(collectionName, chunks);
        }
        else
        {
            collectionName = source;
        }

        // Step 4. Query vector storage
        while (true)
        {
            Console.Write("\n\nEnter your question (or press Enter to exit): ");
            var input = Console.ReadLine() ?? string.Empty;
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            // Step 5. Convert query to embedding
            var query = await embedder.GetEmbedding(input);

            // Step 6. Retrieve top-k chunks from vector storage
            var topChunks = await vectorStorage.SearchAsync(collectionName, query);

            // Step 7: Summarize the answer
            var summary = await summarizer.SummarizeAsync(input, [.. topChunks]);

            Console.WriteLine(summary);
        }
    }
}