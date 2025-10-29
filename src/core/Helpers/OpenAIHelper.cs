namespace core.Helpers
{
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

    public class OpenAIHelper(string model, string apiKey)
    {
        private readonly string _model = model;
        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://api.openai.com/v1/"),
            DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", apiKey) }
        };

        public async Task<T> GetResponseWithWebSearchAsync<T>(string prompt) where T : class
        {
            var payload = new
            {
                model = _model,
                input = prompt,
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "qa",
                        schema = JsonSchemaGenerator.GenerateSchema(typeof(T))
                    }
                },
                tools = new object[]
                {
                    new { type = "web_search", search_context_size = "high" }
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("responses", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            var json = doc.RootElement
                .GetProperty("output")
                .EnumerateArray()
                .Where(el => el.GetProperty("type").GetString() == "message")
                .First()
                .GetProperty("content")
                .EnumerateArray()
                .First()
                .GetProperty("text")
                .GetString();

            var result = JsonSerializer.Deserialize<T>(json!, DefaultJsonSerializerOptions.Options);
            return result!;
        }
    }
}