using core.Models;

namespace core.VectorStorage
{
    public interface IVectorStorage
    {
        Task CreateCollectionAsync(string collectionName, ulong vectorSize);
        Task DeleteCollectionAsync(string collectionName);
        Task InsertAsync(string collectionName, List<Chunk> chunks);
        Task<IEnumerable<Chunk>> SearchAsync(List<string> collections, float[] query, int limit = 3);
    }
}