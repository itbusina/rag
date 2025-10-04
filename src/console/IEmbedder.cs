namespace console
{
    public interface IEmbedder
    {
        Task<float[]> GetEmbedding(string text);
    }
}