using core.Models;
using OpenAI.Chat;

namespace core.Summarization
{
    public class OpenAISummarizer(string model, string apiKey) : ISummarizer
    {
        private const string SYSTEM_PROMPT = "You are an expert assistant. Answer based only on the given context.";
        public readonly ChatClient chatClient = new(model, apiKey);

        public async Task<string> SummarizeAsync(string query, List<Chunk> contextChunks)
        {
            // create LLM context from chunk's content and metadata
            var context = string.Join("\n", contextChunks.Select(c => "Content: " + c.Content + "\n" + string.Join("\n", c.Metadata.Select(m => $"{m.Key}: {m.Value}"))));

            // prepare messages for LLM
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SYSTEM_PROMPT),
                new UserChatMessage($"Context: {context}\n\nQuestion: {query}")
            };

            var completion = await chatClient.CompleteChatAsync(messages);

            return completion.Value.Content.First().Text;
        }
    }
}