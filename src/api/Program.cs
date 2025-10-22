using api.Endpoints;
using api.Services;
using core;
using core.Embeddings;
using core.Storage;
using core.Summarization;
using core.VectorStorage;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register EF Core + SQLite
var connectionString = Environment.GetEnvironmentVariable("DATA_STORAGE_CONNECTION_STRING") ?? "rag.db";
builder.Services.AddDbContext<DataStorageContext>(options => options.UseSqlite($"Data Source={connectionString}"));

// Core services registrations
const string OLLAMA_SERVICE_PROVIDER = "ollama";
const string OPENAI_SERVICE_PROVIDER = "openai";

builder.Services.AddKeyedSingleton<IEmbedder>(OLLAMA_SERVICE_PROVIDER, (sp, key) =>
    new OllamaEmbedder(
        model: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text",
        baseUrl: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434"
    )
);
builder.Services.AddKeyedSingleton<IEmbedder>(OPENAI_SERVICE_PROVIDER, (sp, key) =>
    new OpenAIEmbedder(
        model: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "text-embedding-3-small",
        apiKey: Environment.GetEnvironmentVariable("LLM_API_KEY") ?? ""
    )
);
builder.Services.AddKeyedSingleton<ISummarizer>(OLLAMA_SERVICE_PROVIDER, (sp, key) =>
    new OllamaSummarizer(
        model: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "llama3.1:8b",
        baseUrl: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434"
    )
);
builder.Services.AddKeyedSingleton<ISummarizer>(OPENAI_SERVICE_PROVIDER, (sp, key) =>
    new OpenAISummarizer(
        model: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "gpt-4o-mini",
        apiKey: Environment.GetEnvironmentVariable("LLM_API_KEY") ?? ""
    )
);
builder.Services.AddSingleton<IVectorStorage>((sp) =>
    new QdrantVectorStorage(
        host: Environment.GetEnvironmentVariable("QDRANT_HOST") ?? "localhost",
        apiKey: Environment.GetEnvironmentVariable("QDRANT_API_KEY") ?? "",
        scoreThreshold: float.TryParse(Environment.GetEnvironmentVariable("QDRANT_SCORE_THRESHOLD"), CultureInfo.InvariantCulture, out var threshold) ? threshold : 0.7f //TODO: adjust threshold based on your content
    )
);

// Register JoyQueryClient
builder.Services.AddSingleton<JoyQueryClient>((sp) =>
{
    var provider = Environment.GetEnvironmentVariable("SERVICE_PROVIDER")?.ToLower() ?? OLLAMA_SERVICE_PROVIDER;
    var embedder = sp.GetRequiredKeyedService<IEmbedder>(provider);
    var summarizer = sp.GetRequiredKeyedService<ISummarizer>(provider);
    var vectorStorage = sp.GetRequiredService<IVectorStorage>();

    return new JoyQueryClient(embedder, summarizer, vectorStorage);
});
builder.Services.AddScoped<DataSourceService>();

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

// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataStorageContext>();
    db.Database.EnsureCreated();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable static file hosting from wwwroot
app.UseDefaultFiles(); // serves index.html automatically
app.UseStaticFiles();

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

app.InitDataSourcesEndpoints();
app.InitAssistantEndpoints();

app.Run();