using api.Endpoints;
using api.Services;
using core;
using core.ChatClients;
using core.Chunking;
using core.Embeddings;
using core.Storage;
using core.Summarization;
using core.VectorStorage;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OpenAI.Embeddings;
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
        model: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "gpt-5-mini",
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
    var provider = Environment.GetEnvironmentVariable("SERVICE_PROVIDER")?.ToLower() ?? OLLAMA_SERVICE_PROVIDER;
    var embedder = sp.GetRequiredKeyedService<IEmbedder>(provider);
    var summarizer = sp.GetRequiredKeyedService<ISummarizer>(provider);
    var vectorStorage = sp.GetRequiredService<IVectorStorage>();

    return new JoyQueryClient(embedder, summarizer, vectorStorage);
});
builder.Services.AddScoped<DataSourceService>();
builder.Services.AddSingleton<IAIClient>(sp =>
{
    var llmModel = Environment.GetEnvironmentVariable("LLM_MODEL");
    var embeddingModel = Environment.GetEnvironmentVariable("EMBEDDING_MODEL");
    
    var serviceProvider = Environment.GetEnvironmentVariable("SERVICE_PROVIDER")?.ToLower() ?? OPENAI_SERVICE_PROVIDER;
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
        var apiKey = Environment.GetEnvironmentVariable("LLM_API_KEY") ?? throw new InvalidOperationException("LLM_API_KEY environment variable is not set.");
        return new OpenAIClient(
            new ChatClient(
                model: llmModel ?? "gpt-5-mini",
                apiKey: apiKey
            ),
            new EmbeddingClient(
                model: embeddingModel ?? "text-embedding-3-small",
                apiKey: apiKey
        ));
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