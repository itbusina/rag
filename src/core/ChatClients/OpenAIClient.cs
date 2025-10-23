using core.ChatClients.Models;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace core.ChatClients
{
    public class OpenAIClient(ChatClient chatClient, EmbeddingClient embeddingClient) : IAIClient
    {
        private readonly ChatClient _chatClient = chatClient;
        private readonly EmbeddingClient _embeddingClient = embeddingClient;

        public async Task<string> GetChatCompletionAsync(List<AIClientMessage> messages)
        {
            // prepare messages for LLM
            var openAIMessages = messages.Select<AIClientMessage, ChatMessage>(m =>
            {
                return m.Role.ToLower() switch
                {
                    "system" => new SystemChatMessage(m.Content),
                    "user" => new UserChatMessage(m.Content),
                    "assistant" => new AssistantChatMessage(m.Content),
                    _ => throw new ArgumentException($"Unknown role: {m.Role}")
                };
            }).ToList();

            var completion = await _chatClient.CompleteChatAsync(openAIMessages);

            return completion.Value.Content.First().Text;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
    }
}