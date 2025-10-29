using System.Text.Json.Serialization;

namespace core.AI.Models.Ollama
{
    internal class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}