using core.Models;

namespace core.Summarization
{
    public interface ISummarizer
    {
        Task<string> SummarizeAsync(string query, List<Chunk> contextChunks);
    }
}