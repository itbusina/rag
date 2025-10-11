using core.Embeddings;
using core.Models;
using HtmlAgilityPack;

namespace core.Data
{
    public class HttpDataLoader(string url) : IDataLoader
    {
        private readonly string _url = url;
        private readonly List<string> _paragraphs = [];
        private static readonly HttpClient _httpClient = new();

        public async Task LoadAsync()
        {
            try
            {
                Console.WriteLine($"Loading HTML from: {_url}");
                
                // Download HTML content
                var html = await _httpClient.GetStringAsync(_url);
                
                // Parse HTML
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Remove script, style, and other non-content elements
                RemoveNonContentNodes(htmlDoc);

                // Extract content from main content area if possible, otherwise use body
                var contentNode = FindMainContent(htmlDoc) ?? htmlDoc.DocumentNode.SelectSingleNode("//body") ?? htmlDoc.DocumentNode;

                // Extract text blocks from various semantic elements
                ExtractContentBlocks(contentNode);

                if (_paragraphs.Count == 0)
                {
                    Console.WriteLine("No content found in the HTML document.");
                    return;
                }

                Console.WriteLine($"Loaded {_paragraphs.Count} content blocks from the HTML page.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error loading URL {_url}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing HTML: {ex.Message}");
            }
        }

        private static void RemoveNonContentNodes(HtmlDocument htmlDoc)
        {
            // Remove scripts, styles, and other non-content elements
            var nodesToRemove = new[] { "script", "style", "nav", "footer", "header", "iframe", "noscript" };
            
            foreach (var tagName in nodesToRemove)
            {
                var nodes = htmlDoc.DocumentNode.SelectNodes($"//{tagName}");
                if (nodes != null)
                {
                    foreach (var node in nodes.ToList())
                    {
                        node.Remove();
                    }
                }
            }
        }

        private static HtmlNode? FindMainContent(HtmlDocument htmlDoc)
        {
            // Try to find the main content area using common patterns
            var selectors = new[]
            {
                "//main",
                "//article",
                "//*[@id='content']",
                "//*[@id='main-content']",
                "//*[@class='content']",
                "//*[@class='main-content']",
                "//*[@role='main']"
            };

            foreach (var selector in selectors)
            {
                var node = htmlDoc.DocumentNode.SelectSingleNode(selector);
                if (node != null)
                {
                    Console.WriteLine($"Found main content using selector: {selector}");
                    return node;
                }
            }

            return null;
        }

        private void ExtractContentBlocks(HtmlNode rootNode)
        {
            // Extract content from various semantic elements in document order
            var contentSelectors = new[]
            {
                "//h1", "//h2", "//h3", "//h4", "//h5", "//h6",  // Headings
                "//p",                                              // Paragraphs
                "//li",                                             // List items
                "//blockquote",                                     // Quotes
                "//pre",                                            // Code blocks
                "//td", "//th"                                      // Table cells
            };

            var processedNodes = new HashSet<HtmlNode>();

            foreach (var selector in contentSelectors)
            {
                var nodes = rootNode.SelectNodes(selector);
                if (nodes == null) continue;

                foreach (var node in nodes)
                {
                    // Skip if already processed or if it's inside another content node
                    if (processedNodes.Contains(node)) continue;

                    var text = ExtractTextFromNode(node);
                    
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Add context for headings and special elements
                        var prefix = GetNodePrefix(node);
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            text = $"{prefix}: {text}";
                        }

                        _paragraphs.Add(text);
                        processedNodes.Add(node);
                    }
                }
            }
        }

        private static string ExtractTextFromNode(HtmlNode node)
        {
            var text = node.InnerText.Trim();
            
            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);
            
            // Clean up excessive whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            
            return text;
        }

        private static string GetNodePrefix(HtmlNode node)
        {
            return node.Name.ToLower() switch
            {
                "h1" => "Heading 1",
                "h2" => "Heading 2",
                "h3" => "Heading 3",
                "h4" => "Heading 4",
                "h5" => "Heading 5",
                "h6" => "Heading 6",
                "blockquote" => "Quote",
                "pre" => "Code",
                _ => string.Empty
            };
        }

        public async Task<List<Chunk>> GetContentChunks(IEmbedder embedder)
        {
            if (_paragraphs.Count == 0)
            {
                throw new InvalidOperationException("No content loaded. Call LoadAsync() before GetContentChunks().");
            }

            // Compute embeddings in parallel for all paragraphs
            var chunkTasks = _paragraphs.Select(async paragraph => new Chunk
            {
                Content = paragraph,
                Type = DataSourceType.Url,
                Value = _url,
                Embedding = await embedder.GetEmbedding(paragraph),
                Metadata = new Dictionary<string, string>
                {
                    { "source_url", _url }
                }
            }).ToList();

            var chunks = await Task.WhenAll(chunkTasks);

            Console.WriteLine($"Created {chunks.Length} chunks with embeddings.");
            return [.. chunks];
        }

        public List<string> GetContentBlocks()
        {
            return _paragraphs;
        }
    }
}
