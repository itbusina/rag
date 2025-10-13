using core.Embeddings;
using core.Models;

namespace core.Data
{
    public class QADataLoader(string filePath) : IDataLoader
    {
        private readonly string _filePath = filePath;
        private readonly List<string> _qaPairs = [];

        public async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"File {_filePath} not found.");
                return;
            }

            try
            {
                Console.WriteLine($"Loading Q&A pairs from: {_filePath}");
                
                var content = await File.ReadAllTextAsync(_filePath);
                
                // Split by the separator '\n--\n'
                var pairs = content.Split("\n--\n", StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var pair in pairs)
                {
                    var trimmedPair = pair.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedPair))
                    {
                        _qaPairs.Add(trimmedPair);
                    }
                }

                Console.WriteLine($"Successfully loaded {_qaPairs.Count} Q&A pairs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Q&A file {_filePath}: {ex.Message}");
            }
        }

        public async Task<List<Chunk>> GetContentChunks(IEmbedder embedder)
        {
            if (_qaPairs.Count == 0)
            {
                throw new InvalidOperationException("No Q&A pairs loaded. Call LoadAsync() before GetContentChunks().");
            }

            var chunks = new List<Chunk>();

            foreach (var qaPair in _qaPairs)
            {
                var chunk = new Chunk
                {
                    Content = qaPair,
                    Type = DataSourceType.File,
                    Value = _filePath,
                    Embedding = await embedder.GetEmbedding(qaPair),
                    Metadata = new Dictionary<string, string>
                    {
                        { "file_path", Path.GetFileName(_filePath) },
                    }
                };

                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}
