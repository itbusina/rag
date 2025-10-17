using System.Net.Http.Headers;
using System.Text;
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
        private readonly string _parentPageUrl;
        private Dictionary<string, string> _pages = [];

        public ConfluenceDataLoader(string baseUrl, ConfluenceType type, string token, string parentPageUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _type = type;
            _parentPageUrl = parentPageUrl;
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string?> ResolvePageIdAsync(string urlOrTitle, string? spaceKey = null)
        {
            // Try parse ID directly from URL
            var parsedId = ExtractPageId(urlOrTitle);
            if (!string.IsNullOrEmpty(parsedId))
                return parsedId;

            // If not a URL, try lookup by title
            if (string.IsNullOrEmpty(spaceKey))
                throw new ArgumentException("spaceKey is required when resolving by title.");

            return await GetPageIdByTitleAsync(spaceKey, urlOrTitle);
        }

        public static string? ExtractPageId(string url)
        {
            var cloudMatch = Regex.Match(url, @"/pages/(\d+)");
            if (cloudMatch.Success)
                return cloudMatch.Groups[1].Value;

            var serverMatch = Regex.Match(url, @"[?&]pageId=(\d+)");
            if (serverMatch.Success)
                return serverMatch.Groups[1].Value;

            return null;
        }

        private async Task<string?> GetPageIdByTitleAsync(string spaceKey, string title)
        {
            string url = $"{_baseUrl}/rest/api/content?title={Uri.EscapeDataString(title)}&spaceKey={spaceKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("results", out var resultsArray) &&
                resultsArray.GetArrayLength() > 0)
            {
                return resultsArray[0].GetProperty("id").GetString();
            }

            return null;
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
            var plainText = StripHtml(bodyValue);

            var pageUrl = _type switch
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

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            string text = doc.DocumentNode.InnerText; // then replace <li> with bullets manually

            // 2. Normalize unicode and whitespace
            text = text.Normalize(NormalizationForm.FormC);
            text = Regex.Replace(text, @"\s{2,}", " ").Trim();

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
            var parentPageId = await ResolvePageIdAsync(_parentPageUrl) ?? throw new InvalidOperationException("Parent page not found.");
            _pages = await GetPageWithChildrenAsync(parentPageId);
        }

        public async Task<List<Chunk>> GetContentChunks(IEmbedder embedder)
        {
            if (_pages == null)
                throw new InvalidOperationException("Pages have not been loaded. Call LoadAsync() before GetContentChunks().");

            var resultChunks = new List<Chunk>();
            var splitter = new LangChain.Splitters.Text.RecursiveCharacterTextSplitter(
                                separators: ["\n\n", "\n", " ", ""],
                                chunkSize: 1000,
                                chunkOverlap: 200
                            );

            foreach (var page in _pages)
            {
                var text = page.Value;
                var chunks = splitter.SplitText(text);

                foreach (var chunkText in chunks)
                {
                    var chunk = new Chunk
                    {
                        Content = chunkText,
                        Type = DataSourceType.Confluence,
                        Value = page.Key,
                        Embedding = await embedder.GetEmbedding(chunkText),
                        Metadata = new Dictionary<string, string>
                        {
                            { "url", page.Key }
                        }
                    };
                    
                    resultChunks.Add(chunk);
                }
            }

            return resultChunks;
        }
    }
}
