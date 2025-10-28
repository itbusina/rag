using core.AI.Models;

namespace core.AI
{
    public interface IAIClient
    {
        Task<string> GetResponseAsync(string prompt);
        Task<T> GetResponseAsync<T>(string prompt) where T : class;
        Task<string> GetChatCompletionAsync(List<AIChatMessage> messages);
        Task<float[]> GetEmbeddingAsync(string text);
    }
}