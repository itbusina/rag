using System.Text.Json.Serialization;

namespace core.ChatClients.Models
{
    internal class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public double[] Embedding { get; set; } = [];
    }
}