namespace core.Embeddings
{
    public interface IEmbedder
    {
        Task<float[]> GetEmbedding(string text);
    }
}