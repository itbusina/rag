using System.Text.Json.Serialization;

namespace core.AI.Models.Ollama
{
    internal class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public double[] Embedding { get; set; } = [];
    }
}