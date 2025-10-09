using core.Data;
using core.Embeddings;
using core.Summarization;
using core.VectorStorage;
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


var embedder = new OllamaEmbedder("nomic-embed-text"); // new OpenAIEmbedder("text-embedding-3-small", apiKey);
var summarizer = new OllamaSummarizer("llama3.1:8b"); //new OpenAISummarizer("gpt-4.1-mini", apiKey);
var vectorStorage = new QdrantVectorStorage();

app.MapPost("/assistant", async (AssistantRequest request) =>
{
    var source = request.SourceType;
    var sourceValue = request.SourceValue;

    // Load data to vectore storage
    IDataLoader dataLoader = source switch
    {
        "file" => new FileDataLoader(embedder, sourceValue),
        "qa" => new QADataLoader(embedder, sourceValue),
        "github" => new GitHubDataLoader(embedder, sourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
        "http" => new HttpDataLoader(embedder, sourceValue),
        "sitemap" => new SitemapDataLoader(embedder, sourceValue),
        _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'qa', 'github', 'http', or 'sitemap'."),
    };

    // Step 1. Load file content
    await dataLoader.LoadAsync();

    // Step 2: Load chunks for data source
    var chunks = await dataLoader.GetContentChunks();

    // Step 3: Store chunks in vector storage
    var collectionName = Guid.NewGuid().ToString();
    await vectorStorage.CreateCollectionAsync(collectionName, 768); // 768 is the dimension of the "nomic-embed-text" model
    await vectorStorage.InsertAsync(collectionName, chunks);

    return Results.Ok(new
    {
        Id = collectionName,
    });
})
.WithName("CreateAssistant");

app.MapPost("/assistant/{id:guid}", async (Guid id, [FromBody] string input) =>
{
    var collectionName = id.ToString();

    // Step 5. Convert query to embedding
    var query = await embedder.GetEmbedding(input.Trim());

    // Step 6. Retrieve top-k chunks from vector storage
    var topChunks = await vectorStorage.SearchAsync(collectionName, query);

    // Step 7: Summarize the answer
    var summary = await summarizer.SummarizeAsync(input, [.. topChunks]);

    return Results.Ok(new
    {
        Id = id,
        Response = summary
    });
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