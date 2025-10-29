using System.Text.Json.Serialization;

namespace core.AI.Models.Ollama
{
    internal class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}