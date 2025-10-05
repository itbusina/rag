using console.Models;

namespace console.Data
{
    public interface IDataLoader
    {
        public void Load();
        
        public Task<List<Chunk>> GetContentChunks();
    }
}