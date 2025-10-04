using console;
using OpenAI.Chat;

class Program
{
    static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load(); // loads .env file

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");

        var embedder = new OpenAIEmbedder("text-embedding-3-small", apiKey);

        // Option 1: Load from a file
        //var dataLoader = new FileDataLoader(embedder, "sample.txt");

        // Option 2: Load from GitHub commit messages
        var dataLoader = new GitHubDataLoader(embedder, "https://github.com/testlemon/testlemon");
        // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits

        var retriver = new Retriver(embedder);
        var augmenter = new OpenAIAugmenter("gpt-4.1-mini", apiKey);

        // Step 1. Load file content
        dataLoader.Load();

        // Step 2: Chunk text
        var chunks = await dataLoader.GetContentChunks();

        // Step 4. Query
        while (true)
        {
            Console.Write("Enter your question (or press Enter to exit): ");
            var input = Console.ReadLine() ?? string.Empty;
            var query = input.Trim();

            // Step 5. Retrieve top k findings
            var topChunks = await retriver.GetTopKChunks(chunks, query, k: 3);

            // Step 6: Augment with context
            var augmentedResponse = await augmenter.AugmentAsync(query, topChunks);

            Console.WriteLine(augmentedResponse);
        }
    }
}