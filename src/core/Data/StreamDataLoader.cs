using core.Embeddings;
using core.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace core.Data
{
    public class StreamDataLoader(string filename, Stream fileStream) : IDataLoader
    {
        private readonly string _filename = filename;
        private readonly Stream _fileStream = fileStream;
        private string _content = string.Empty;

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
                    case ".pdf":
                        await LoadPdfFromStreamAsync();
                        break;
                    case ".txt":
                    case ".md":
                    case ".csv":
                    case ".json":
                    case ".xml":
                    case ".html":
                    case ".log":
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
                _content = await reader.ReadToEndAsync();
            }

            Console.WriteLine($"Successfully loaded text file from stream.");
        }

        private async Task LoadPdfFromStreamAsync()
        {
            Console.WriteLine($"Loading PDF from stream: {_filename}");

            // Reset stream position if possible
            if (_fileStream.CanSeek)
            {
                _fileStream.Position = 0;
            }

            // Copy stream to memory stream if it's not seekable
            Stream streamToUse = _fileStream;
            if (!_fileStream.CanSeek)
            {
                var memoryStream = new MemoryStream();
                await _fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                streamToUse = memoryStream;
            }

            try
            {
                using var document = PdfDocument.Open(streamToUse);
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

                Console.WriteLine($"Successfully loaded PDF with {document.NumberOfPages} pages from stream.");
            }
            finally
            {
                // Clean up memory stream if we created one
                if (streamToUse != _fileStream)
                {
                    await streamToUse.DisposeAsync();
                }
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
            
            var chunkTasks = textChunks.Select(async content => new Chunk
            {
                Content = content,
                SourceType = SourceType.Stream,
                SourceValue = _filename,
                Embedding = await embedder.GetEmbedding(content),
                Metadata = new Dictionary<string, string>
                {
                    { "file_name", _filename }
                }
            }).ToList();

            var chunks = await Task.WhenAll(chunkTasks);

            Console.WriteLine($"Generated embeddings for all {chunks.Length} chunks.");

            return [..chunks];
        }
    }
}

