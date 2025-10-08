using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace console.Embeddings
{
    public class OllamaEmbedder : IEmbedder
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _baseUrl;

        public OllamaEmbedder(string model, string baseUrl = "http://localhost:11434")
        {
            _model = model;
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMinutes(5) // Ollama can be slow on first run
            };
        }

        public async Task<float[]> GetEmbedding(string text)
        {
            var request = new OllamaEmbeddingRequest
            {
                Model = _model,
                Prompt = text
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
                
                if (result?.Embedding == null || result.Embedding.Length == 0)
                {
                    throw new Exception("Invalid response from Ollama API");
                }

                // Convert double[] to float[]
                return result.Embedding.Select(d => (float)d).ToArray();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
        }

        // Ollama API models
        private class OllamaEmbeddingRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
        }

        private class OllamaEmbeddingResponse
        {
            [JsonPropertyName("embedding")]
            public double[] Embedding { get; set; } = Array.Empty<double>();
        }
    }
}
