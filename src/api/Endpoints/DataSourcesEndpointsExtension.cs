using core;
using core.Data;
using core.Storage;
using core.Storage.Models;

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
            }).WithName("AddDataSources");

            app.MapGet("/datasources", (HttpRequest request, DataStorageContext context) =>
            {
                var sources = context.DataSources.ToList();
                return Results.Ok(sources);
            }).WithName("ListDataSources");
        }
    }
}