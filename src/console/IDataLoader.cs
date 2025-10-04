namespace console
{
    public interface IDataLoader
    {
        public void Load();
        
        public Task<List<Chunk>> GetContentChunks();
    }
}