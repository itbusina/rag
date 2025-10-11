using core.Data;
using core.Embeddings;
using core.Summarization;
using core.VectorStorage;

namespace core
{
    public class JoyQueryClient(string llmEndpoint, string embeddingModel, string summarizingModel, string qdrantEndpoint, int qdrantPort)
    {
        private readonly IVectorStorage _vectorStorage = new QdrantVectorStorage(qdrantEndpoint, qdrantPort);
        private readonly IEmbedder _embedder = new OllamaEmbedder(embeddingModel, llmEndpoint);
        private readonly ISummarizer _summarizer = new OllamaSummarizer(summarizingModel, llmEndpoint);

        public async Task<string> LoadDataAsync(string sourceType, string sourceValue)
        {
            IDataLoader dataLoader = sourceType switch
            {
                "file" => new LocalFileDataLoader(sourceValue),
                "qa" => new QADataLoader(sourceValue),
                "github" => new GitHubDataLoader(sourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
                "http" => new HttpDataLoader(sourceValue),
                "sitemap" => new SitemapDataLoader(sourceValue),
                _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'qa', 'github', 'http', or 'sitemap'."),
            };

            return await LoadDataAsync(dataLoader);
        }

        public async Task<string> LoadDataAsync(IDataLoader dataLoader)
        {
            // Step 1. Load file content
            await dataLoader.LoadAsync();

            // Step 2: Load chunks for data source
            var chunks = await dataLoader.GetContentChunks(_embedder);

            // Step 3: Store chunks in vector storage
            var collectionName = Guid.NewGuid().ToString();
            await _vectorStorage.CreateCollectionAsync(collectionName, 768); // 768 is the dimension of the "nomic-embed-text" model
            await _vectorStorage.InsertAsync(collectionName, chunks);

            return collectionName;
        }

        public async Task<string> QueryAsync(List<string> collections, string question)
        {
            // Step 4. Convert query to embedding
            var query = await _embedder.GetEmbedding(question);

            // Step 5. Retrieve top-k chunks from vector storage for each collection
            var topChunks = await _vectorStorage.SearchAsync(collections, query);

            // Step 6: Summarize the answer
            var summary = await _summarizer.SummarizeAsync(question, [.. topChunks]);

            return summary;
        }

        public async Task DeleteCollection(string collectionName)
        {
            await _vectorStorage.DeleteCollectionAsync(collectionName);
        }
    }
}