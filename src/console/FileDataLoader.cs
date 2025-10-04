namespace console
{
    public class FileDataLoader(IEmbedder embedder, string filePath) : IDataLoader
    {
        private readonly IEmbedder _embedder = embedder;
        private readonly string _filePath = filePath;
        private string _content = string.Empty;

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"File {_filePath} not found.");
                return;
            }

            _content = File.ReadAllText(_filePath);
        }

        public async Task<List<Chunk>> GetContentChunks()
        {
            if (string.IsNullOrEmpty(_content))
            {
                throw new InvalidOperationException("Content not loaded. Call Load() before GetContentChunks().");
            }

            var chunks = TextChunker.ChunkText(_content, maxTokens: 200, overlap: 50);

            foreach (var chunk in chunks)
            {
                chunk.Embedding = await _embedder.GetEmbedding(chunk.Content);
            }

            return chunks;
        }
    }
}