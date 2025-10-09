using core.Data;
using core.Embeddings;
using core.Models;
using core.Retrieving;
using core.Summarization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

var storage = new Dictionary<string, List<Chunk>>();

var embedder = new OllamaEmbedder("nomic-embed-text"); // new OpenAIEmbedder("text-embedding-3-small", apiKey);
var summarizer = new OllamaSummarizer("llama3.1:8b"); //new OpenAISummarizer("gpt-4.1-mini", apiKey);
var retriever = new Retriever(embedder);

app.MapPost("/assistant", async (AssistantRequest request) =>
{
    IDataLoader dataLoader = request.SourceType switch
    {
        "file" => new FileDataLoader(embedder, request.SourceValue),
        "qa" => new QADataLoader(embedder, request.SourceValue),
        "github" => new GitHubDataLoader(embedder, request.SourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
        "http" => new HttpDataLoader(embedder, request.SourceValue),
        "sitemap" => new SitemapDataLoader(embedder, request.SourceValue),
        _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'qa', 'github', 'http', or 'sitemap'."),
    };

    // Step 1. Load file content
    await dataLoader.LoadAsync();

    // Step 2: Load chunks for data source
    var chunks = await dataLoader.GetContentChunks();

    // Store chunks in memory (in a real app, consider using a persistent storage)
    var id = Guid.NewGuid().ToString();
    storage[id] = chunks;

    return Results.Ok(new
    {
        Id = id,
        Chunks = chunks.Select(x => new { x.Content, x.Metadata }),
    });
})
.WithName("CreateAssistant");

app.MapPost("/assistant/{id:guid}", async (Guid id, [FromBody] string query) =>
{
    if (storage.TryGetValue(id.ToString(), out var chunks))
    {
        query = query.Trim();

        if (string.IsNullOrEmpty(query))
        {
            return Results.BadRequest("Query cannot be empty.");
        }

        // Step 5. Retrieve top k findings
        var topChunks = await retriever.GetTopKChunks(chunks, query, k: 3);

        // Step 6: Augment with context
        var summary = await summarizer.SummarizeAsync(query, topChunks);

        return Results.Ok(new
        {
            Id = id,
            Response = summary
        });
    }

    return Results.NotFound();
});

app.MapGet("/assistant/{id:guid}/chat.js", async (Guid id, HttpContext context) =>
{
    var scheme = context.Request.Scheme;
    var host = context.Request.Host.Value;
    var baseUrl = $"{scheme}://{host}";
    
    // Read the widget script template from file
    var scriptPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "chat-widget.js");
    var scriptTemplate = await File.ReadAllTextAsync(scriptPath);
    
    // Replace placeholders with actual values
    var script = scriptTemplate
        .Replace("{{ASSISTANT_ID}}", id.ToString())
        .Replace("{{API_BASE_URL}}", baseUrl);
    
    return Results.Content(script, "application/javascript");
})
.WithName("GetChatWidget");

app.Run();

record AssistantRequest(string SourceType, string SourceValue);