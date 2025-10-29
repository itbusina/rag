using System.Text.Json.Serialization;

namespace core.AI.Models.Ollama
{
    internal class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}