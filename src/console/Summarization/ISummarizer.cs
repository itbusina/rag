using console.Models;

namespace console.Summarization
{
    public interface ISummarizer
    {
        Task<string> SummarizeAsync(string query, List<Chunk> contextChunks);
    }
}