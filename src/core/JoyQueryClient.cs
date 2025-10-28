using System.Text.Json;
using core.AI;
using core.AI.Models;
using core.Data;
using core.Helpers;
using core.Models;
using core.VectorStorage;

namespace core
{
    public class JoyQueryClient(IVectorStorage vectorStorage, IAIClient aIClient)
    {
        private const string DEFAULT_SYSTEM_PROMPT = "You are an expert assistant. Answer the question based only on the given context.";
        private readonly IVectorStorage _vectorStorage = vectorStorage;
        private readonly IAIClient _aIClient = aIClient;

        public async Task<string> LoadDataAsync(IDataLoader dataLoader)
        {
            // Step 1. Load file content
            await Monitoring.Log(() => dataLoader.LoadAsync(), "dataLoader.LoadAsync()");

            // Step 2: Load chunks for data source
            var chunks = await Monitoring.Log(() => dataLoader.GetContentChunks(_aIClient), "dataLoader.GetContentChunks(_aIClient)");

            if (chunks.Count == 0)
                throw new InvalidOperationException("No content chunks were loaded from the data source.");

            // Step 3: Store chunks in vector storage
            var collectionName = Guid.NewGuid().ToString();
            var vectorSize = (ulong)chunks.First().Embedding.Length;
            await _vectorStorage.CreateCollectionAsync(collectionName, vectorSize);
            await _vectorStorage.InsertAsync(collectionName, chunks);

            return collectionName;
        }

        public async Task<string> QueryAsync(List<string> collections, string question, int limit = 3, string? instructions = "")
        {
            // Step 4. Convert query to embedding
            var query = await GetEmbedding(question);

            // Step 5. Retrieve top-k chunks from vector storage for each collection
            var topChunks = await Monitoring.Log(() => _vectorStorage.SearchAsync(collections, query, limit), "_vectorStorage.SearchAsync(collections, query, limit)");

            // Step 6: Summarize the answer
            var summary = await Monitoring.Log(() => SummarizeAsync(question, [.. topChunks], instructions), "SummarizeAsync(question, [.. topChunks],  instructions");

            return summary;
        }

        private async Task<float[]> GetEmbedding(string text)
        {
            return await _aIClient.GetEmbeddingAsync(text);
        }

        private async Task<string> SummarizeAsync(string query, List<Chunk> chunks, string? instructions = null)
        {
            // create LLM context from chunk's content and metadata
            var context = JsonSerializer.Serialize(chunks.Select(c => new
            {
                c.Content,
                Metadata = c.Metadata.ToDictionary(m => m.Key, m => m.Value)
            }), DefaultJsonSerializerOptions.Options);
            
            // prepare messages for LLM
            var messages = new List<AIChatMessage>
            {
                new(){ Role = "system", Content = instructions ?? DEFAULT_SYSTEM_PROMPT },
                new(){ Role = "user", Content = $"Context: {context}\n\nQuestion: {query}" }
            };

            var completion = await _aIClient.GetChatCompletionAsync(messages);

            return completion;
        }

        public async Task DeleteCollection(string collectionName)
        {
            await _vectorStorage.DeleteCollectionAsync(collectionName);
        }
    }
}