using api.Endpoints;
using core;
using core.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register EF Core + SQLite
builder.Services.AddDbContext<DataStorageContext>(options =>
    options.UseSqlite("Data Source=rag.db"));

// Add CORS services
// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

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
//app.UseCors();

app.UseHttpsRedirection();

var joyQueryClient = new JoyQueryClient(

    llmEndpoint: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434",
    embeddingModel: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text",
    summarizingModel: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "llama3.1:8b",
    qdrantEndpoint: Environment.GetEnvironmentVariable("QDRANT_ENDPOINT") ?? "localhost",
    qdrantPort: int.TryParse(Environment.GetEnvironmentVariable("QDRANT_PORT"), out var port) ? port : 6334
);

app.InitDataSourcesEndpoints(joyQueryClient);
app.InitAssistantEndpoints(joyQueryClient);

app.Run();