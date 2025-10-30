using core.AI;
using core.Data.Models;
using core.Models;
using System.Xml.Linq;

namespace core.Data
{
    public class SitemapDataLoader(OpenAIClient openAIClient, string sitemapUrl) : IDataLoader
    {
        private readonly OpenAIClient _openAIClient = openAIClient;
        private readonly string _sitemapUrl = sitemapUrl;
        private readonly Dictionary<string, List<FAQModel>> _sitemapPages = [];
        private static readonly HttpClient _httpClient = new();

        public async Task LoadAsync()
        {
            try
            {
                Console.WriteLine($"Loading sitemap from: {_sitemapUrl}");

                // Download sitemap XML
                var sitemapXml = await _httpClient.GetStringAsync(_sitemapUrl);

                // Parse sitemap and extract URLs
                var urls = ParseSitemapUrls(sitemapXml);

                if (urls.Count == 0)
                {
                    Console.WriteLine("No URLs found in the sitemap.");
                    return;
                }

                Console.WriteLine($"Found {urls.Count} URLs in the sitemap. Loading content in parallel...");

                // Load all URLs in parallel using HttpDataLoader
                var loadTasks = urls.Select(async url =>
                {
                    try
                    {
                        var webQALoader = new WebPageToQADataLoader(_openAIClient, url);
                        await webQALoader.LoadAsync();
                        return new KeyValuePair<string, List<FAQModel>>(url, webQALoader.GetContentBlocks());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading URL {url}: {ex.Message}");
                        return new KeyValuePair<string, List<FAQModel>>(url, []);
                    }
                }).ToList();

                // Wait for all tasks to complete
                var results = await Task.WhenAll(loadTasks);

                // Aggregate all content blocks
                foreach (var result in results)
                {
                    _sitemapPages.Add(result.Key, result.Value);
                }

                Console.WriteLine($"Successfully loaded {_sitemapPages.Count} total content blocks from {urls.Count} URLs.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error loading sitemap from {_sitemapUrl}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing sitemap: {ex.Message}");
            }
        }

        private static List<string> ParseSitemapUrls(string sitemapXml)
        {
            var urls = new List<string>();

            try
            {
                var xdoc = XDocument.Parse(sitemapXml);

                // Handle sitemap namespace
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

                // Try with namespace first
                var urlElements = xdoc.Descendants(ns + "url").ToList();

                // If no elements found with namespace, try without namespace
                if (urlElements.Count == 0)
                {
                    urlElements = xdoc.Descendants("url").ToList();
                }

                foreach (var urlElement in urlElements)
                {
                    // Try with namespace
                    var locElement = urlElement.Element(ns + "loc") ?? urlElement.Element("loc");

                    if (locElement != null && !string.IsNullOrWhiteSpace(locElement.Value))
                    {
                        urls.Add(locElement.Value.Trim());
                    }
                }

                // Check if this is a sitemap index (contains links to other sitemaps)
                var sitemapElements = xdoc.Descendants(ns + "sitemap").ToList();
                if (sitemapElements.Count == 0)
                {
                    sitemapElements = xdoc.Descendants("sitemap").ToList();
                }

                if (sitemapElements.Count > 0)
                {
                    Console.WriteLine($"Detected sitemap index with {sitemapElements.Count} sub-sitemaps.");

                    // This is a sitemap index, recursively load all sub-sitemaps
                    var subSitemapUrls = new List<string>();
                    foreach (var sitemapElement in sitemapElements)
                    {
                        var locElement = sitemapElement.Element(ns + "loc") ?? sitemapElement.Element("loc");
                        if (locElement != null && !string.IsNullOrWhiteSpace(locElement.Value))
                        {
                            subSitemapUrls.Add(locElement.Value.Trim());
                        }
                    }

                    // Load all sub-sitemaps in parallel
                    var subSitemapTasks = subSitemapUrls.Select(async subSitemapUrl =>
                    {
                        try
                        {
                            Console.WriteLine($"Loading sub-sitemap: {subSitemapUrl}");
                            var subSitemapXml = await _httpClient.GetStringAsync(subSitemapUrl);
                            return ParseSitemapUrls(subSitemapXml);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading sub-sitemap {subSitemapUrl}: {ex.Message}");
                            return [];
                        }
                    }).ToList();

                    var subResults = Task.WhenAll(subSitemapTasks).GetAwaiter().GetResult();

                    // Aggregate all URLs from sub-sitemaps
                    foreach (var subUrls in subResults)
                    {
                        urls.AddRange(subUrls);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing sitemap XML: {ex.Message}");
            }

            return urls;
        }

        public async Task<List<Chunk>> GetContentChunks(IAIClient aIClient)
        {
            if (_sitemapPages.Count == 0)
            {
                throw new InvalidOperationException("No content loaded. Call LoadAsync() before GetContentChunks().");
            }

            var chunks = new List<Chunk>();
            
            foreach (var page in _sitemapPages)
            {
                foreach (var pair in page.Value)
                {
                    var chunk = new Chunk
                    {
                        Content = pair.Answer,
                        Type = DataSourceType.Sitemap,
                        Value = _sitemapUrl,
                        Embedding = await aIClient.GetEmbeddingAsync(pair.Question),
                        Metadata = new Dictionary<string, string>
                        {
                            { "source_url", page.Key }
                        }
                    };

                    chunks.Add(chunk);
                }
            }

            return chunks;
        }
    }
}
