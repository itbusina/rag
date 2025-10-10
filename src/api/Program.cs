using core;
using core.Data;
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

// Enable static file hosting from wwwroot
app.UseDefaultFiles(); // serves index.html automatically
app.UseStaticFiles();

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

var joyQueryClient = new JoyQueryClient(

    llmEndpoint: Environment.GetEnvironmentVariable("LLM_ENDPOINT") ?? "http://localhost:11434",
    embeddingModel: Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text",
    summarizingModel: Environment.GetEnvironmentVariable("SUMMARIZING_MODEL") ?? "llama3.1:8b",
    qdrantEndpoint: Environment.GetEnvironmentVariable("QDRANT_ENDPOINT") ?? "localhost",
    qdrantPort: int.TryParse(Environment.GetEnvironmentVariable("QDRANT_PORT"), out var port) ? port : 6334
);

app.MapPost("/files", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var collectionNames = new List<string>();

    foreach (var file in form.Files)
    {
        if (file.Length == 0)
            continue;

        var dataLoader = new StreamDataLoader(file.FileName, file.OpenReadStream());
        var collectionName = await joyQueryClient.LoadDataAsync(dataLoader);
        collectionNames.Add(collectionName);
    }

    return Results.Ok(new
    {
        Ids = collectionNames
    });
}).WithName("AddDataSources");

app.MapGet("/files", async (HttpRequest request) =>
{
    var list = await joyQueryClient.ListDataSources();
    return Results.Ok(list);
}).WithName("ListDataSources");

app.MapPost("/assistant", async (AssistantRequest request) =>
{
    var collectionName = await joyQueryClient.LoadDataAsync(request.SourceType, request.SourceValue);

    return Results.Ok(new
    {
        Id = collectionName,
    });
})
.WithName("CreateAssistant");

app.MapPost("/assistant/{id:guid}", async (Guid id, [FromBody] string input) =>
{
    var summary = await joyQueryClient.QueryAsync(id.ToString(), input);

    return Results.Ok(new
    {
        Id = id,
        Response = summary
    });
})
.WithName("Query");

app.MapGet("/assistant/{id:guid}/chat.js", async (Guid id, HttpContext context) =>
{
    var scheme = context.Request.Scheme;
    var host = context.Request.Host.Value;
    var baseUrl = $"{scheme}://{host}";

    // Read the widget script template from file
    var scriptPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "js/chat-widget.js");
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