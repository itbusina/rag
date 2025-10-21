using api.Services;
using core;
using core.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints
{
    public static class DataSourcesEndpointsExtension
    {
        public static void InitDataSourcesEndpoints(this WebApplication app)
        {
            app.MapPost("/datasources", async ([FromQuery] string type, HttpRequest request, DataSourceService service, DataStorageContext context) =>
            {
                var form = await request.ReadFormAsync();
                var name = form["name"].ToString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Results.BadRequest("Data source name is required");
                }

                var collectionNames = type switch
                {
                    "file" => await service.AddFileDataSourceAsync(name, form.Files),
                    "faq" => await service.AddFAQDataSourceAsync(name, form.Files),
                    "confluence" => await service.AddConfluenceDataSourceAsync(name, form["url"].ToString(), form["token"].ToString(), form["parentPageId"].ToString()),
                    "github" => await service.AddGitHubDataSourceAsync(name, form["url"].ToString(), form["token"].ToString()),
                    _ => throw new NotSupportedException($"Data source type '{type}' is not supported")
                };

                return Results.Ok(collectionNames);
            })
            .WithMetadata(new RequestSizeLimitAttribute(100*1024*1024)) // 100 MB
            .WithName("AddDataSources");

            app.MapGet("/datasources", (DataStorageContext context) =>
            {
                var dataSources = context.DataSources.ToList();
                return Results.Ok(dataSources);
            })
            .WithName("ListDataSources");

            app.MapDelete("/datasources/{id:guid}", async (Guid id, JoyQueryClient client, DataStorageContext context) =>
            {
                var dataSource = await context.DataSources
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (dataSource == null)
                    return Results.NotFound();

                // First delete the collection from vector storage
                await client.DeleteCollection(dataSource.CollectionName);

                // Remove the data source from app storage
                context.DataSources.Remove(dataSource);
                await context.SaveChangesAsync();

                return Results.Ok();
            })
            .WithName("DeleteDataSource");
        }
    }
}