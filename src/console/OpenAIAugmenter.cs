
using OpenAI.Chat;

namespace console
{
    public class OpenAIAugmenter(string model, string apiKey) : IAugmenter
    {
        private const string systemPrompt = "You are an expert assistant. Answer based only on the given context.";
        public readonly ChatClient chatClient = new(model, apiKey);

        public async Task<string> AugmentAsync(string query, List<Chunk> contextChunks)
        {
            var userPrompt = $"Context: {string.Join("\n", contextChunks.Select(c => c.Content))}\n\nQuestion: {query}";
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var completion = await chatClient.CompleteChatAsync(messages);

            return completion.Value.Content.First().Text;
        }
    }
}