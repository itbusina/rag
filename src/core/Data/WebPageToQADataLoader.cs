using core.AI;
using core.Data.Models;
using core.Helpers;
using core.Models;

namespace core.Data
{
    //TODO: works with OpenAI ChatClient only for now to be able to call web search tool via openai API. Could be extended to other llm have web search capabilities
    public class WebPageToQADataLoader(OpenAIHelper client, string url) : IDataLoader
    {
        private readonly OpenAIHelper _client = client;
        private readonly string _url = url;
        private readonly string _prompt = $"Load the web page from {url} and create a list of question-answer pairs based on the page content. Return in JSON array format with 'question' and 'answer' fields.";
        private readonly List<FAQModel> _qaPairs = [];

        public async Task LoadAsync()
        {
            try
            {
                Console.WriteLine($"Loading web page to QA pairs from: {_url}");

                var response = await _client.GetResponseWithWebSearchAsync<ContentToQAResponse>(_prompt);
                var pairs = response?.Responses ?? [];

                _qaPairs.AddRange(pairs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading web page to QA pairs: {ex.Message}");
            }
        }

        public async Task<List<Chunk>> GetContentChunks(IAIClient aIClient)
        {
            if (_qaPairs.Count == 0)
            {
                throw new InvalidOperationException("No content loaded. Call LoadAsync() before GetContentChunks().");
            }

            var chunks = new List<Chunk>();
            foreach (var pair in _qaPairs)
            {
                var chunk = new Chunk
                {
                    Content = pair.Answer,
                    Type = DataSourceType.Url,
                    Value = _url,
                    Embedding = await aIClient.GetEmbeddingAsync(pair.Question),
                    Metadata = new Dictionary<string, string>
                    {
                        { "source_url", _url }
                    }
                };
                chunks.Add(chunk);
            }

            return chunks;
        }

        public List<FAQModel> GetContentBlocks()
        {
            return _qaPairs;
        }
    }
}
