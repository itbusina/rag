using System.Text.Json.Serialization;

namespace core.AI.Models.Ollama
{
    internal class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("format")]
        public object? Format { get; set; }
    }
}