using core;
using core.Storage;
using core.Storage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints
{
    record AssistantRequest(string Name, List<string> DataSources, string? Instructions = null, int? QueryResultsLimit = null);

    public static class AssistantEndpointsExtension
    {
        public static void InitAssistantEndpoints(this WebApplication app)
        {
            app.MapGet("/assistants", (DataStorageContext context) =>
            {
                var assistants = context.Assistants
                    .Include(x => x.DataSources)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        DataSources = x.DataSources.Select(x => x.Id).ToList()
                    })
                    .ToList();

                return Results.Ok(assistants);
            })
            .WithName("ListAssistants");

            app.MapPost("/assistants", async (AssistantRequest request, DataStorageContext context) =>
            {
                var searchList = request.DataSources.Select(x => Guid.Parse(x)).ToList();

                var dataSources = context.DataSources
                    .Where(x => searchList.Any(s => s == x.Id))
                    .ToList();

                var assistant = new Assistant
                {
                    Name = request.Name,
                    DataSources = dataSources,
                    Instructions = request.Instructions,
                    QueryResultsLimit = request.QueryResultsLimit
                };

                context.Assistants.Add(assistant);
                await context.SaveChangesAsync();

                return Results.Created($"/assistants/{assistant.Id}", new
                {
                    Id = assistant.Id,
                    Name = assistant.Name,
                });
            })
            .WithName("CreateAssistant");

            app.MapDelete("/assistants/{id:guid}", async (Guid id, DataStorageContext context) =>
            {
                var assistant = context.Assistants.FirstOrDefault(x => x.Id == id);
                if (assistant == null)
                    return Results.NotFound();

                context.Assistants.Remove(assistant);
                await context.SaveChangesAsync();

                return Results.Ok();
            })
            .WithName("DeleteAssistant");

            app.MapGet("/assistants/{id:guid}", async (Guid id, DataStorageContext context) =>
            {
                var assistant = context.Assistants
                    .Include(x => x.DataSources)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Instructions,
                        x.QueryResultsLimit,
                        DataSources = x.DataSources.Select(ds => ds.Id).ToList()
                    })
                    .FirstOrDefault(x => x.Id == id);

                if (assistant == null)
                    return Results.NotFound();

                return Results.Ok(assistant);
            })
            .WithName("GetAssistant");

            app.MapPut("/assistants/{id:guid}", async (Guid id, [FromBody] AssistantRequest request, DataStorageContext context) =>
            {
                var assistant = context.Assistants
                    .Include(x => x.DataSources)
                    .FirstOrDefault(x => x.Id == id);

                if (assistant == null)
                    return Results.NotFound();

                // Update data sources
                var searchList = request.DataSources.Select(x => Guid.Parse(x)).ToList();
                var dataSources = context.DataSources
                    .Where(x => searchList.Any(s => s == x.Id))
                    .ToList();

                // Update assistant properties
                assistant.Name = request.Name;
                assistant.DataSources = dataSources;
                assistant.Instructions = request.Instructions;
                assistant.QueryResultsLimit = request.QueryResultsLimit;
                
                await context.SaveChangesAsync();

                return Results.Ok();
            })
            .WithName("UpdateAssistant");

            app.MapPost("/assistants/all", async ([FromBody] string input, JoyQueryClient client, DataStorageContext context) =>
            {
                var collections = context.DataSources
                    .Select(x => x.CollectionName)
                    .ToList();

                var summary = await client.QueryAsync(collections, input, 3);

                return Results.Ok(new
                {
                    Response = summary
                });
            })
            .WithName("QueryAllAssistant");

            app.MapPost("/assistants/{id:guid}", async (Guid id, [FromBody] string input, JoyQueryClient client, DataStorageContext context) =>
            {
                var assistant = context.Assistants
                    .Include(x => x.DataSources)
                    .FirstOrDefault(x => x.Id == id);

                if (assistant == null)
                    return Results.NotFound();

                var collections = assistant.DataSources
                    .Select(x => x.CollectionName)
                    .ToList();

                var summary = await client.QueryAsync(
                                            collections,
                                            input,
                                            assistant.QueryResultsLimit ?? 3,
                                            assistant.Instructions);

                return Results.Ok(new
                {
                    Response = summary
                });
            })
            .WithName("QuerySpecificAssistant");

            app.MapGet("/assistants/{id:guid}/chat.js", async (Guid id, HttpContext context) =>
            {
                var scheme = context.Request.Scheme;
                var host = context.Request.Host.Value;
                var baseUrl = $"{scheme}://{host}";

                // Read the widget script template from file
                var scriptPath = Path.Combine(app.Environment.ContentRootPath, "widget", "js/widget.js");
                var scriptTemplate = await File.ReadAllTextAsync(scriptPath);

                // Replace placeholders with actual values
                var script = scriptTemplate
                    .Replace("{{ASSISTANT_ID}}", id.ToString())
                    .Replace("{{API_BASE_URL}}", baseUrl);

                return Results.Content(script, "application/javascript");
            })
            .WithName("GetAssistantWidget");
        }
    }
}