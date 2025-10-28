using System.Text.Json.Serialization;

namespace core.AI.Models.OpenAI
{
    public class OpenAIResponseRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public object? Text { get; set; }

        [JsonPropertyName("tools")]
        public object[]? Tools { get; set; }
    }
}