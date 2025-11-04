using System.Net.Http.Json;
using System.Text.Json;
using core.AI.Models;
using core.AI.Models.Ollama;
using core.Helpers;

namespace core.AI
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

        public async Task<string> GetResponseAsync(string prompt)
        {
            var request = new OllamaRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false,
            };

            return await GetResponseAsync(request);
        }

        public async Task<T> GetResponseAsync<T>(string prompt, object[]? tools) where T : class
        {
            var request = new OllamaRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false,
                Format = JsonSchemaGenerator.GenerateSchema(typeof(T))
            };

            var response = await GetResponseAsync(request);
            var result = JsonSerializer.Deserialize<T>(response, DefaultJsonSerializerOptions.Options) ?? throw new Exception("Failed to deserialize Ollama response");
            return result;
        }

        private async Task<string> GetResponseAsync(OllamaRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>() ?? throw new Exception("Invalid response from Ollama API");
                return result.Response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error. Ollama request: {JsonSerializer.Serialize(request)}");
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
        }

        public async Task<string> GetChatCompletionAsync(List<AIChatMessage> messages)
        {
            // prepare messages for Ollama
            var ollamaMessages = messages.Select(m => new OllamaMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList();

            var request = new OllamaChatRequest
            {
                Model = _model,
                Messages = ollamaMessages,
                Stream = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>() ?? throw new Exception("Invalid response from Ollama API");
                return result.Message!.Content;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error. Ollama request: {JsonSerializer.Serialize(request)}");
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
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

                var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>() ?? throw new Exception("Invalid response from Ollama API");

                // Convert double[] to float[]
                return [.. result.Embedding.Select(d => (float)d)];
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error. Ollama request: {JsonSerializer.Serialize(request)}");
                throw new Exception($"Failed to connect to Ollama at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}", ex);
            }
        }
    }
}