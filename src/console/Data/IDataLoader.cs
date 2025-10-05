using console.Models;

namespace console.Data
{
    public interface IDataLoader
    {
        public Task LoadAsync();
        
        public Task<List<Chunk>> GetContentChunks();
    }
}