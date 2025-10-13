using core;
using core.Data;
using core.Storage;
using core.Storage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints
{
    public static class DataSourcesEndpointsExtension
    {
        public static void InitDataSourcesEndpoints(this WebApplication app, JoyQueryClient client)
        {
            app.MapPost("/datasources", async (HttpRequest request, DataStorageContext context) =>
            {
                var form = await request.ReadFormAsync();
                var collectionNames = new List<string>();
                var name = form["name"].ToString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Results.BadRequest("Name is required");
                }

                foreach (var file in form.Files)
                {
                    if (file.Length == 0)
                        continue;

                    var dataLoader = new StreamDataLoader(file.FileName, file.OpenReadStream(), 2, 1);
                    var collectionName = await client.LoadDataAsync(dataLoader);
                    collectionNames.Add(collectionName);

                    context.DataSources.Add(new DataSource
                    {
                        Name = name,
                        CollectionName = collectionName,
                        DataSourceType = core.Models.DataSourceType.Stream,
                        DataSourceValue = file.FileName,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                await context.SaveChangesAsync();

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

            app.MapDelete("/datasources/{id:guid}", async (Guid id, DataStorageContext context) =>
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