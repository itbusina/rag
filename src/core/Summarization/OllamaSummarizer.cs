using core.Helpers;
using core.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace core.Summarization
{
    public class OllamaSummarizer : ISummarizer
    {
        private const string DEFAULT_SYSTEM_PROMPT = "You are an expert assistant. Answer the question based only on the given context.";
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _baseUrl;

        public OllamaSummarizer(string model, string baseUrl = "http://localhost:11434")
        {
            _model = model;
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMinutes(5) // Ollama can be slow on first run
            };
        }

        public async Task<string> SummarizeAsync(string query, List<Chunk> contextChunks, string? instructions = null)
        {
            // create LLM context from chunk's content and metadata
            var context = JsonSerializer.Serialize(contextChunks.Select(c => new
            {
                c.Content,
                Metadata = c.Metadata.ToDictionary(m => m.Key, m => m.Value)
            }), DefaultJsonSerializerOptions.Options);

            // prepare messages for Ollama
            var messages = new List<OllamaMessage>
            {
                new() { Role = "system", Content = instructions ?? DEFAULT_SYSTEM_PROMPT },
                new() { Role = "user", Content = $"Context: {context}\n\nQuestion: {query}" }
            };

            var request = new OllamaChatRequest
            {
                Model = _model,
                Messages = messages,
                Stream = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

                if (result?.Message?.Content == null)
                {
                    throw new Exception("Invalid response from Ollama API");
                }

                return result.Message.Content;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
        }

        // Ollama API models
        private class OllamaMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private class OllamaChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<OllamaMessage> Messages { get; set; } = new();

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        private class OllamaChatResponse
        {
            [JsonPropertyName("message")]
            public OllamaMessage? Message { get; set; }

            [JsonPropertyName("done")]
            public bool Done { get; set; }
        }
    }
}
