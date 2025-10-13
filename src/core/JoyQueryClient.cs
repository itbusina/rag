using core.Data;
using core.Embeddings;
using core.Helpers;
using core.Summarization;
using core.VectorStorage;

namespace core
{
    public class JoyQueryClient(string llmEndpoint, string embeddingModel, string summarizingModel, string qdrantEndpoint)
    {
        private readonly IVectorStorage _vectorStorage = new QdrantVectorStorage(qdrantEndpoint);
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
            var chunks = await Monitoring.Log(() => dataLoader.GetContentChunks(_embedder), "dataLoader.GetContentChunks(_embedder)");

            if (chunks.Count == 0)
                throw new InvalidOperationException("No content chunks were loaded from the data source.");

            // Step 3: Store chunks in vector storage
            var collectionName = Guid.NewGuid().ToString();
            var vectorSize = (ulong)chunks.First().Embedding.Length;
            await _vectorStorage.CreateCollectionAsync(collectionName, vectorSize);
            await _vectorStorage.InsertAsync(collectionName, chunks);

            return collectionName;
        }

        public async Task<string> QueryAsync(List<string> collections, string question, int limit = 3)
        {
            // Step 4. Convert query to embedding
            var query = await _embedder.GetEmbedding(question);

            // Step 5. Retrieve top-k chunks from vector storage for each collection
            var topChunks = await Monitoring.Log(() => _vectorStorage.SearchAsync(collections, query, limit), "_vectorStorage.SearchAsync(collections, query, limit)");

            // Step 6: Summarize the answer
            var summary = await Monitoring.Log(() => _summarizer.SummarizeAsync(question, [.. topChunks]), "_summarizer.SummarizeAsync(question, [.. topChunks]");

            return summary;
        }

        public async Task DeleteCollection(string collectionName)
        {
            await _vectorStorage.DeleteCollectionAsync(collectionName);
        }
    }
}