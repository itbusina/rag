using console.Models;

namespace console.Augmentation
{
    public interface IAugmenter
    {
        Task<string> AugmentAsync(string query, List<Chunk> contextChunks);
    }
}