using System.Net.Http.Json;
using System.Text.Json;
using core.AI.Models;
using core.AI.Models.OpenAI;
using core.Helpers;
using OpenAI.Chat;

namespace core.AI
{
    public class OpenAIClient(string apiKey, string completionModel, string embeddingModel) : IAIClient
    {
        private readonly string _completionModel = completionModel;
        private readonly string _embeddingModel = embeddingModel;
        private readonly OpenAI.OpenAIClient _openAIClient = new(apiKey);
        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://api.openai.com/"),
            DefaultRequestHeaders = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey) }
        };

        public async Task<string> GetResponseAsync(string prompt)
        {
            var request = new OpenAIResponseRequest
            {
                Model = _completionModel,
                Input = prompt,
            };

            return await GetResponseAsync(request);
        }

        public async Task<T> GetResponseAsync<T>(string prompt) where T : class
        {
            var request = new OpenAIResponseRequest
            {
                Model = _completionModel,
                Input = prompt,
                Text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "qa",
                        schema = JsonSchemaGenerator.GenerateSchema(typeof(T))
                    }
                },
            };

            var json = await GetResponseAsync(request);
            var result = JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions.Options) ?? throw new Exception("Failed to deserialize OpenAI response");
            return result;
        }

        private async Task<string> GetResponseAsync(OpenAIResponseRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("v1/responses", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var text = doc.RootElement
                .GetProperty("output")
                .EnumerateArray()
                .Where(el => el.GetProperty("type").GetString() == "message")
                .First()
                .GetProperty("content")
                .EnumerateArray()
                .First()
                .GetProperty("text")
                .GetString();

            return text!;
        }

        public async Task<string> GetChatCompletionAsync(List<AIChatMessage> messages)
        {
            // prepare messages for LLM
            var openAIMessages = messages.Select<AIChatMessage, ChatMessage>(m =>
            {
                return m.Role.ToLower() switch
                {
                    "system" => new SystemChatMessage(m.Content),
                    "user" => new UserChatMessage(m.Content),
                    "assistant" => new AssistantChatMessage(m.Content),
                    _ => throw new ArgumentException($"Unknown role: {m.Role}")
                };
            }).ToList();

            var client = _openAIClient.GetChatClient(_completionModel);
            var completion = await client.CompleteChatAsync(openAIMessages);
            return completion.Value.Content.First().Text;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var embeddingClient = _openAIClient.GetEmbeddingClient(_embeddingModel);
            var embedding = await embeddingClient.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
    }
}