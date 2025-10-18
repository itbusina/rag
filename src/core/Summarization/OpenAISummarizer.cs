using System.Text.Json;
using core.Models;
using OpenAI.Chat;

namespace core.Summarization
{
    public class OpenAISummarizer(string model, string apiKey) : ISummarizer
    {
        private const string DEFAULT_SYSTEM_PROMPT = "You are an expert assistant. Answer based only on the given context.";
        public readonly ChatClient chatClient = new(model, apiKey);
        private JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true // Enables pretty formatting
        };

        public async Task<string> SummarizeAsync(string query, List<Chunk> contextChunks, string? instructions = null)
        {
            // create LLM context from chunk's content and metadata
            var context = JsonSerializer.Serialize(contextChunks.Select(c => new
            {
                c.Content,
                Metadata = c.Metadata.ToDictionary(m => m.Key, m => m.Value)
            }), _jsonOptions);
            
            // prepare messages for LLM
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(instructions ?? DEFAULT_SYSTEM_PROMPT),
                new UserChatMessage($"Context: {context}\n\nQuestion: {query}")
            };

            var completion = await chatClient.CompleteChatAsync(messages);

            return completion.Value.Content.First().Text;
        }
    }
}