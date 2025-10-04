
using OpenAI.Chat;

namespace console
{
    public class OpenAIAugmenter(string model, string apiKey) : IAugmenter
    {
        public readonly ChatClient chatClient = new ChatClient(model, apiKey);

        public async Task<string> AugmentAsync(string query, List<Chunk> contextChunks)
        {
            var systemPrompt = "You are an expert assistant. Answer based only on the given context.";
            var userPrompt = $"Context: {string.Join("\n", contextChunks.Select(c => c.Content))}\n\nQuestion: {query}";

            var completion = await chatClient.CompleteChatAsync([
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ]);

            return completion.Value.Content.First().Text;
        }
    }
}