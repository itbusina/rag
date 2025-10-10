using core.Embeddings;
using core.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace core.Data
{
    public class LocalFileDataLoader(string filePath) : IDataLoader
    {
        private readonly string _filePath = filePath;
        private string _content = string.Empty;

        public async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"File {_filePath} not found.");
                return;
            }

            var extension = Path.GetExtension(_filePath).ToLowerInvariant();

            try
            {
                switch (extension)
                {
                    case ".pdf":
                        await LoadPdfAsync();
                        break;
                    case ".txt":
                    case ".md":
                    case ".csv":
                    case ".json":
                    case ".xml":
                    case ".html":
                    case ".log":
                    case "": // Files without extension
                        await LoadTextAsync();
                        break;
                    default:
                        // Try to load as text for any other extension
                        Console.WriteLine($"Unknown file extension '{extension}', attempting to load as text file.");
                        await LoadTextAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file {_filePath}: {ex.Message}");
            }
        }

        private async Task LoadTextAsync()
        {
            Console.WriteLine($"Loading text file from: {_filePath}");
            _content = await File.ReadAllTextAsync(_filePath);
            Console.WriteLine($"Successfully loaded text file.");
        }

        private async Task LoadPdfAsync()
        {
            Console.WriteLine($"Loading PDF from: {_filePath}");

            using (var document = PdfDocument.Open(_filePath))
            {
                var textBuilder = new System.Text.StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    var pageText = page.Text;

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        // Add page separator for context
                        textBuilder.AppendLine($"\n--- Page {page.Number} ---\n");
                        textBuilder.AppendLine(pageText);
                    }
                }

                _content = textBuilder.ToString();

                if (string.IsNullOrWhiteSpace(_content))
                {
                    Console.WriteLine("No text content found in the PDF document.");
                    return;
                }

                Console.WriteLine($"Successfully loaded PDF with {document.NumberOfPages} pages.");
            }

            await Task.CompletedTask;
        }

        public async Task<List<Chunk>> GetContentChunks(IEmbedder embedder)
        {
            if (string.IsNullOrEmpty(_content))
            {
                throw new InvalidOperationException("Content not loaded. Call LoadAsync() before GetContentChunks().");
            }

            var textChunks = TextChunker.ChunkText(_content, maxTokens: 200, overlap: 50);

            Console.WriteLine($"Created {textChunks.Count} chunks from file content.");

            var chunks = new List<Chunk>();

            foreach (var text in textChunks)
            {
                var chunk = new Chunk
                {
                    Content = text,
                    SourceType = SourceType.File,
                    SourceValue = _filePath,
                    Embedding = await embedder.GetEmbedding(text),
                    Metadata = new Dictionary<string, string>
                    {
                        { "file_path", Path.GetFileName(_filePath) }
                    },
                };

                chunks.Add(chunk);
            }

            Console.WriteLine($"Generated embeddings for all {chunks.Count} chunks.");

            return chunks;
        }
    }
}