using OpenAI.Embeddings;

namespace console.Embeddings
{
    public class OpenAIEmbedder(string model, string apiKey) : IEmbedder
    {
        private readonly EmbeddingClient _client = new(model, apiKey);

        public async Task<float[]> GetEmbedding(string text)
        {
            var embedding = await _client.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
    }
}