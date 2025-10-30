using api.Endpoints;
using api.Services;
using core;
using core.AI;
using core.Chunking;
using core.Storage;
using core.VectorStorage;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// default parameters
const string OLLAMA_SERVICE_PROVIDER = "ollama";
const string OPENAI_SERVICE_PROVIDER = "openai";
const string DEFAULT_OPENAI_API_KEY = "sk-svc..";
const string DEFAULT_OPENAI_MODEL = "gpt-5-mini";
const string DEFAULT_OPENAI_EMBEDDING_MODEL = "text-embedding-3-small";
const string DEFAULT_OLLAMA_MODEL = "llama3.1:8b";
const string DEFAULT_OLLAMA_EMBEDDING_MODEL = "nomic-embed-text";
const string DEFAULT_OLLAMA_ENDPOINT = "http://localhost:11434";
const string DEFAULT_DB_CONNECTION_STRING = "rag.db";
const string DEFAULT_QDRANT_HOST = "localhost";
const string DEFAULT_QDRANT_API_KEY = "";
const float DEFAULT_QDRANT_SCORE_THRESHOLD = 0.5f;
const string DEFAULT_TEXT_CHUNKER_TYPE = "recursive";
const int DEFAULT_TEXT_CHUNK_SIZE = 1000;
const int DEFAULT_TEXT_CHUNK_OVERLAP = 100;
const int DEFAULT_SENTENCE_CHUNK_SIZE = 5;
const int DEFAULT_SENTENCE_CHUNK_OVERLAP = 1;

const string DEFAULT_SERVICE_PROVIDER = OPENAI_SERVICE_PROVIDER;
const string DEFAULT_LLM_MODEL = DEFAULT_OPENAI_MODEL;
const string DEFAULT_LLM_EMBEDDING_MODEL = DEFAULT_OPENAI_EMBEDDING_MODEL;
const string DEFAULT_LLM_API_KEY = DEFAULT_OPENAI_API_KEY;

var serviceProvider = Environment.GetEnvironmentVariable("SERVICE_PROVIDER")?.ToLower() ?? DEFAULT_SERVICE_PROVIDER;
var apiKey = Environment.GetEnvironmentVariable("LLM_API_KEY") ?? DEFAULT_LLM_API_KEY;
var model = Environment.GetEnvironmentVariable("LLM_MODEL") ?? DEFAULT_LLM_MODEL;
var embeddingModel = Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? DEFAULT_LLM_EMBEDDING_MODEL;
var llmEndpoint = Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? DEFAULT_OLLAMA_ENDPOINT;
var sqLiteConnectionString = Environment.GetEnvironmentVariable("DATA_STORAGE_CONNECTION_STRING") ?? DEFAULT_DB_CONNECTION_STRING;

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataStorageContext>(options => options.UseSqlite($"Data Source={sqLiteConnectionString}"));
builder.Services.AddSingleton<IVectorStorage>((sp) =>
    new QdrantVectorStorage(
        host: Environment.GetEnvironmentVariable("QDRANT_HOST") ?? DEFAULT_QDRANT_HOST,
        apiKey: Environment.GetEnvironmentVariable("QDRANT_API_KEY") ?? DEFAULT_QDRANT_API_KEY,
        scoreThreshold: float.TryParse(Environment.GetEnvironmentVariable("QDRANT_SCORE_THRESHOLD"), CultureInfo.InvariantCulture, out var threshold) ? threshold : DEFAULT_QDRANT_SCORE_THRESHOLD
    )
);
// text chunker registrations
builder.Services.AddSingleton<SentenceChunker>(sp =>
{
    int maxSentences = int.TryParse(Environment.GetEnvironmentVariable("SENTENCE_CHUNK_SIZE"), out var max) ? max : DEFAULT_SENTENCE_CHUNK_SIZE;
    int overlap = int.TryParse(Environment.GetEnvironmentVariable("SENTENCE_CHUNK_OVERLAP"), out var ovl) ? ovl : DEFAULT_SENTENCE_CHUNK_OVERLAP;
    return new SentenceChunker(maxSentences, overlap);
});
builder.Services.AddSingleton<RecursiveTextChunker>(sp =>
{
    int chunkSize = int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_SIZE"), out var size) ? size : DEFAULT_TEXT_CHUNK_SIZE;
    int overlap = int.TryParse(Environment.GetEnvironmentVariable("TEXT_CHUNK_OVERLAP"), out var ovl) ? ovl : DEFAULT_TEXT_CHUNK_OVERLAP;
    return new RecursiveTextChunker(chunkSize, overlap);
});
builder.Services.AddSingleton<ITextChunker>((sp) =>
{
    var chunkerType = Environment.GetEnvironmentVariable("TEXT_CHUNKER_TYPE")?.ToLower() ?? DEFAULT_TEXT_CHUNKER_TYPE;
    return chunkerType == "sentence"
        ? sp.GetRequiredService<SentenceChunker>() 
        : sp.GetRequiredService<RecursiveTextChunker>();
});
builder.Services.AddSingleton<JoyQueryClient>();
builder.Services.AddSingleton<OpenAIClient>(sp => new OpenAIClient(apiKey, model, embeddingModel));
builder.Services.AddSingleton<OllamaClient>(sp => new OllamaClient(model, embeddingModel, llmEndpoint));
builder.Services.AddSingleton<IAIClient>(sp =>
{
    return serviceProvider == OLLAMA_SERVICE_PROVIDER
        ? sp.GetRequiredService<OllamaClient>()
        : sp.GetRequiredService<OpenAIClient>();
});
builder.Services.AddScoped<DataSourceService>();

// Application registrations
builder.Services.AddOpenApi(); // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCors(options => // Add CORS services
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

app.UseDefaultFiles(); // Serves index.html automatically
app.UseStaticFiles(); // Enable static file hosting from wwwroot
app.UseCors();
app.UseHttpsRedirection();
app.InitDataSourcesEndpoints();
app.InitAssistantEndpoints();
app.Run();