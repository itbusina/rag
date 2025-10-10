using core;

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
            llmEndpoint: Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434",
            embeddingModel: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text",
            summarizingModel: Environment.GetEnvironmentVariable("LLM_MODEL") ?? "llama3.1:8b",
            qdrantEndpoint: Environment.GetEnvironmentVariable("QDRANT_ENDPOINT") ?? "localhost",
            qdrantPort: int.TryParse(Environment.GetEnvironmentVariable("QDRANT_PORT"), out var port) ? port : 6334
        );

        var collectionName = Guid.TryParse(source, out _)
            ? source 
            : await joyQueryClient.LoadDataAsync(source, sourceValue);
        
        while (true)
        {
            Console.Write("\n\nEnter your question (or press Enter to exit): ");
            var input = Console.ReadLine() ?? string.Empty;
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            var summary = await joyQueryClient.QueryAsync(collectionName, input);

            Console.WriteLine(summary);
        }
    }
}