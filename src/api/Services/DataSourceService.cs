using core;
using core.Data;
using core.Storage;
using core.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class DataSourceService(JoyQueryClient client, DataStorageContext context)
    {
        private readonly JoyQueryClient _client = client;
        private readonly DataStorageContext _context = context;

        public async Task<List<DataSource>> GetAllDataSourcesAsync()
        {
            return await _context.DataSources.ToListAsync();
        }

        public async Task<DataSource?> GetDataSourceByIdAsync(Guid id)
        {
            return await _context.DataSources.FindAsync(id);
        }

        public async Task AddDataSourceAsync(DataSource dataSource)
        {
            _context.DataSources.Add(dataSource);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteDataSourceAsync(Guid id)
        {
            var dataSource = await _context.DataSources.FindAsync(id);
            if (dataSource == null)
            {
                return false;
            }

            _context.DataSources.Remove(dataSource);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> AddFileDataSourceAsync(string name, IFormFileCollection files)
        {
            var collectionNames = new List<string>();
            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var dataLoader = new StreamDataLoader(file.FileName, file.OpenReadStream(), 2, 1);
                var collectionName = await _client.LoadDataAsync(dataLoader);
                collectionNames.Add(collectionName);

                _context.DataSources.Add(new DataSource
                {
                    Name = name,
                    CollectionName = collectionName,
                    DataSourceType = core.Models.DataSourceType.Stream,
                    DataSourceValue = file.FileName,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return collectionNames;
        }

        internal async Task<List<string>> AddConfluenceDataSourceAsync(string name, string baseUrl, string token, string parentPageId)
        {
            var collectionNames = new List<string>();
            var dataLoader = new ConfluenceDataLoader(baseUrl, ConfluenceType.Server, token, parentPageId);
            var collectionName = await _client.LoadDataAsync(dataLoader);
            collectionNames.Add(collectionName);

            _context.DataSources.Add(new DataSource
            {
                Name = name,
                CollectionName = collectionName,
                DataSourceType = core.Models.DataSourceType.Confluence,
                DataSourceValue = $"{baseUrl} - Parent Page ID: {parentPageId}",
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return collectionNames;
        }
    }
}