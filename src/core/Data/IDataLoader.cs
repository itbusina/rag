using core.Models;

namespace core.Data
{
    public interface IDataLoader
    {
        public Task LoadAsync();
        
        public Task<List<Chunk>> GetContentChunks();
    }
}