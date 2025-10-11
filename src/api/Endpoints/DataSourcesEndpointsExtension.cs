using core;
using core.Data;
using core.Storage;
using core.Storage.Models;
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

                foreach (var file in form.Files)
                {
                    if (file.Length == 0)
                        continue;

                    var dataLoader = new StreamDataLoader(file.FileName, file.OpenReadStream());
                    var collectionName = await client.LoadDataAsync(dataLoader);
                    collectionNames.Add(collectionName);

                    context.DataSources.Add(new DataSource
                    {
                        CollectionName = collectionName,
                        DataSourceType = core.Models.DataSourceType.Stream,
                        DataSourceValue = file.FileName,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                await context.SaveChangesAsync();

                return Results.Ok(collectionNames);
            })
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