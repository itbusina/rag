using System.Net.Http.Json;
using core.ChatClients.Models;

namespace core.ChatClients
{
    public class OllamaClient : IAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _embeddingModel;
        private readonly string _baseUrl;

        public OllamaClient(string model, string embeddingModel, string baseUrl = "http://localhost:11434")
        {
            _model = model;
            _embeddingModel = embeddingModel;
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMinutes(5) // Ollama can be slow on first run
            };
        }

        public async Task<string> GetChatCompletionAsync(List<AIClientMessage> messages)
        {
            throw new NotImplementedException();
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var request = new OllamaEmbeddingRequest
            {
                Model = _embeddingModel,
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
                return [.. result.Embedding.Select(d => (float)d)];
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
        }
    }
}