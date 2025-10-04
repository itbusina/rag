namespace console
{
    public interface IAugmenter
    {
        Task<string> AugmentAsync(string query, List<Chunk> contextChunks);
    }
}