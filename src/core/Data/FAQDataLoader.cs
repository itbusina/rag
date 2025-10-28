using System.Text.Json;
using core.AI;
using core.Data.Models;
using core.Models;

namespace core.Data
{
    public class FAQDataLoader(string filename, Stream fileStream) : IDataLoader
    {
        private readonly string _filename = filename;
        private readonly Stream _fileStream = fileStream;
        private List<FAQModel> _content = [];

        public async Task LoadAsync()
        {
            if (_fileStream == null || !_fileStream.CanRead)
            {
                Console.WriteLine($"Invalid or unreadable stream for file: {_filename}");
                return;
            }

            var extension = Path.GetExtension(_filename).ToLowerInvariant();

            try
            {
                switch (extension)
                {
                    case ".txt":
                    case ".json":
                    case "": // Files without extension
                        await LoadTextFromStreamAsync();
                        break;
                    default:
                        // Try to load as text for any other extension
                        Console.WriteLine($"Unknown file extension '{extension}', attempting to load as text file.");
                        await LoadTextFromStreamAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file {_filename} from stream: {ex.Message}");
            }
        }

        private async Task LoadTextFromStreamAsync()
        {
            Console.WriteLine($"Loading text file from stream: {_filename}");

            // Reset stream position if possible
            if (_fileStream.CanSeek)
            {
                _fileStream.Position = 0;
            }

            using (var reader = new StreamReader(_fileStream, leaveOpen: true))
            {
                var json = await reader.ReadToEndAsync();
                _content = JsonSerializer.Deserialize<List<FAQModel>>(json) ?? [];
            }

            Console.WriteLine($"Successfully loaded text file from stream.");
        }

        public async Task<List<Chunk>> GetContentChunks(IAIClient aIClient)
        {
            var chunks = new List<Chunk>();

            foreach (var content in _content)
            {
                var chunk = new Chunk
                {
                    Content = content.Answer!,
                    Type = DataSourceType.Stream,
                    Value = _filename,
                    Embedding = await aIClient.GetEmbeddingAsync(content.Question!),
                    Metadata = []
                };

                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}

