using api.Endpoints;
using api.Services;
using core;
using core.AI;
using core.Chunking;
using core.Storage;
using core.VectorStorage;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Core services registrations
const string OLLAMA_SERVICE_PROVIDER = "ollama";
const string OPENAI_SERVICE_PROVIDER = "openai";
const string OPENAI_API_KEY = null;

// Determine service provider
var serviceProvider = Environment.GetEnvironmentVariable("SERVICE_PROVIDER")?.ToLower() ?? OLLAMA_SERVICE_PROVIDER;

// Register EF Core + SQLite
var connectionString = Environment.GetEnvironmentVariable("DATA_STORAGE_CONNECTION_STRING") ?? "rag.db";
builder.Services.AddDbContext<DataStorageContext>(options => options.UseSqlite($"Data Source={connectionString}"));

builder.Services.AddSingleton<IVectorStorage>((sp) =>
    new QdrantVectorStorage(
        host: Environment.GetEnvironmentVariable("QDRANT_HOST") ?? "localhost",
        apiKey: Environment.GetEnvironmentVariable("QDRANT_API_KEY") ?? "",
        scoreThreshold: float.TryParse(Environment.GetEnvironmentVariable("QDRANT_SCORE_THRESHOLD"), CultureInfo.InvariantCulture, out var threshold) ? threshold : 0.5f //TODO: adjust threshold based on your content
    )
);

// text chunker registrations
builder.Services.AddSingleton<ITextChunker>((sp) =>
{
    var chunkerType = Environment.GetEnvironmentVariable("TEXT_CHUNKER_TYPE")?.ToLower() ?? "recursive";
    if(chunkerType == "sentence")
    {
        int maxSentences = int.TryParse(Environment.GetEnvironmentVariable("SENTENCE_CHUNK_SIZE"), out var max) ? max : 5;
        int overlap = int.TryParse(Environment.GetEnvironmentVariable("SENTENCE_CHUNK_OVERLAP"), out var ovl) ? ovl : 1;
        return new SentenceChunker(maxSentences, overlap);
    }
    else // default to recursive
    {
        int chunkSize = int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_SIZE"), out var size) ? size : 1000;
        int overlap = int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_OVERLAP"), out var ovl) ? ovl : 100;
        return new RecursiveTextChunker(chunkSize, overlap);
    }
});

// Register JoyQueryClient
builder.Services.AddSingleton<JoyQueryClient>((sp) =>
{
    var vectorStorage = sp.GetRequiredService<IVectorStorage>();
    var aiClient = sp.GetRequiredService<IAIClient>();
    return new JoyQueryClient(vectorStorage, aiClient);
});
builder.Services.AddScoped<DataSourceService>();
builder.Services.AddSingleton<IAIClient>(sp =>
{
    var llmModel = Environment.GetEnvironmentVariable("LLM_MODEL");
    var embeddingModel = Environment.GetEnvironmentVariable("EMBEDDING_MODEL");
    
    if (serviceProvider == OLLAMA_SERVICE_PROVIDER)
    {
        var llmEndpoint = Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434";
        return new OllamaClient(
            model: llmModel ?? "llama3.1:8b",
            embeddingModel: embeddingModel ?? "nomic-embed-text",
            baseUrl: llmEndpoint 
        );
    }
    else // default to OpenAI
    {
        var apiKey = OPENAI_API_KEY ?? Environment.GetEnvironmentVariable("LLM_API_KEY") ?? throw new InvalidOperationException("LLM_API_KEY environment variable is not set.");
        return new OpenAIClient(
            apiKey,
            llmModel ?? "gpt-5-mini",
            embeddingModel ?? "text-embedding-3-small"
        );
    }
});

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