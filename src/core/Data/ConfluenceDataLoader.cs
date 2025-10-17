using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using core.Embeddings;
using core.Models;

namespace core.Data
{
    public enum ConfluenceType { Cloud, Server }
    public enum AuthType { Basic, Bearer }

    public class ConfluenceDataLoader : IDataLoader, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ConfluenceType _type;
        private readonly string _parentPageId;
        private Dictionary<string, string> _pages = [];

        public ConfluenceDataLoader(string baseUrl, ConfluenceType type, string token, string parentPageId)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _type = type;
            _parentPageId = parentPageId;   
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Dictionary<string, string>> GetPageWithChildrenAsync(string parentPageId)
        {
            var results = new Dictionary<string, string>();

            var parent = await GetPageByIdAsync(parentPageId);
            if (parent != null)
                AddPageToDictionary(parent.Value, results);

            await FetchChildrenRecursive(parentPageId, results);
            return results;
        }

        private async Task FetchChildrenRecursive(string parentId, Dictionary<string, string> result)
        {
            string expand = _type == ConfluenceType.Cloud ? "body.storage" : "body.view";
            string url = $"{_baseUrl}/rest/api/content/{parentId}/child/page?expand={expand}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("results", out var resultsArray))
                return;

            foreach (var child in resultsArray.EnumerateArray())
            {
                AddPageToDictionary(child, result);
                string childId = child.GetProperty("id").GetString() ?? "";
                await FetchChildrenRecursive(childId, result);
            }
        }

        private async Task<JsonElement?> GetPageByIdAsync(string pageId)
        {
            string expand = _type == ConfluenceType.Cloud ? "body.storage" : "body.view";
            string url = $"{_baseUrl}/rest/api/content/{pageId}?expand={expand}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement;
        }

        private void AddPageToDictionary(JsonElement page, Dictionary<string, string> results)
        {
            string id = page.GetProperty("id").GetString() ?? "";
            string title = page.GetProperty("title").GetString() ?? "";

            string bodyValue = "";
            if (page.TryGetProperty("body", out var body))
            {
                string key = _type == ConfluenceType.Cloud ? "storage" : "view";
                if (body.TryGetProperty(key, out var view) &&
                    view.TryGetProperty("value", out var valueProp))
                {
                    bodyValue = valueProp.GetString() ?? "";
                }
            }

            // Clean HTML
            string plainText = StripHtml(bodyValue);

            string pageUrl = _type switch
            {
                ConfluenceType.Cloud => $"{_baseUrl}/spaces/{GetSpaceKeyFromPage(page)}/pages/{id}/{Uri.EscapeDataString(title.Replace(' ', '-'))}",
                _ => $"{_baseUrl}/pages/viewpage.action?pageId={id}"
            };

            results[pageUrl] = plainText;
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Replace specific tags with newlines or spaces before stripping the rest
            string text = html;

            // Block-level elements → newlines
            text = Regex.Replace(text, @"<(br|br\s*/)>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</(p|div|section|article|header|footer|tr|table)>", "\n", RegexOptions.IgnoreCase);

            // List items → newlines (and optional dash for readability)
            text = Regex.Replace(text, @"<li[^>]*>", "\n• ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</li>", "\n", RegexOptions.IgnoreCase);

            // Table cells → spaces or newlines
            text = Regex.Replace(text, @"</td>", " ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</th>", " ", RegexOptions.IgnoreCase);

            // Headings → ensure line breaks
            text = Regex.Replace(text, @"</h\d>", "\n", RegexOptions.IgnoreCase);

            // Remove all other tags
            text = Regex.Replace(text, "<.*?>", string.Empty);

            // Decode HTML entities (&nbsp;, &amp;, etc.)
            text = System.Net.WebUtility.HtmlDecode(text);

            // Normalize line endings and trim whitespace
            text = Regex.Replace(text, @"\r\n|\r|\n", "\n");   // unify line endings
            text = Regex.Replace(text, @"[ \t]+", " ");        // collapse spaces
            text = Regex.Replace(text, @"\n{2,}", "\n");       // collapse multiple newlines

            return text.Trim();
        }

        private static string GetSpaceKeyFromPage(JsonElement page)
        {
            if (page.TryGetProperty("space", out var space) && space.TryGetProperty("key", out var key))
                return key.GetString() ?? "UNKNOWN";
            return "UNKNOWN";
        }

        public void Dispose() => _httpClient?.Dispose();

        public async Task LoadAsync()
        {
            _pages = await GetPageWithChildrenAsync(_parentPageId);
        }

        public async Task<List<Chunk>> GetContentChunks(IEmbedder embedder)
        {
            if (_pages == null)
                throw new InvalidOperationException("Pages have not been loaded. Call LoadAsync() before GetContentChunks().");

            var chunks = new List<Chunk>();

            foreach (var page in _pages)
            {
                var chunk = new Chunk
                {
                    Content = page.Value,
                    Type = DataSourceType.Confluence,
                    Value = page.Key,
                    Embedding = await embedder.GetEmbedding(page.Value),
                    Metadata = new Dictionary<string, string>
                    {
                        { "url", page.Key }
                    }
                };

                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}
