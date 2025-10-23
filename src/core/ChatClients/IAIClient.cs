using core.ChatClients.Models;

namespace core.ChatClients
{
    public interface IAIClient
    {
        Task<string> GetChatCompletionAsync(List<AIClientMessage> messages);
        Task<float[]> GetEmbeddingAsync(string text);
    }
}