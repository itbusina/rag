using core;
using core.Chunking;
using core.Data;
using core.Models;
using core.Storage;
using core.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class DataSourceService(JoyQueryClient client, DataStorageContext context, ITextChunker textChunker)
    {
        private readonly JoyQueryClient _client = client;
        private readonly DataStorageContext _context = context;
        private readonly ITextChunker _textChunker = textChunker;

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

                var dataLoader = new StreamDataLoader(_textChunker, file.FileName, file.OpenReadStream());
                var collectionName = await _client.LoadDataAsync(dataLoader);
                collectionNames.Add(collectionName);

                _context.DataSources.Add(new DataSource
                {
                    Name = name,
                    CollectionName = collectionName,
                    DataSourceType = DataSourceType.Stream,
                    DataSourceValue = file.FileName,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return collectionNames;
        }

        public async Task<List<string>> AddFAQDataSourceAsync(string name, IFormFileCollection files)
        {
            var collectionNames = new List<string>();
            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var dataLoader = new FAQDataLoader(file.FileName, file.OpenReadStream());
                var collectionName = await _client.LoadDataAsync(dataLoader);
                collectionNames.Add(collectionName);

                _context.DataSources.Add(new DataSource
                {
                    Name = name,
                    CollectionName = collectionName,
                    DataSourceType = DataSourceType.Stream,
                    DataSourceValue = file.FileName,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return collectionNames;
        }

        internal async Task<List<string>> AddConfluenceDataSourceAsync(string name, string baseUrl, string token, string parentPageUrl)
        {
            var dataLoader = new ConfluenceDataLoader(baseUrl, ConfluenceType.Server, token, parentPageUrl);
            var collectionName = await _client.LoadDataAsync(dataLoader);

            _context.DataSources.Add(new DataSource
            {
                Name = name,
                CollectionName = collectionName,
                DataSourceType = DataSourceType.Confluence,
                DataSourceValue = parentPageUrl,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return [collectionName];
        }

        internal async Task<List<string>> AddGitHubDataSourceAsync(string name, string repoUrl, string token)
        {
            var dataLoader = new GitHubDataLoader(repoUrl, token);
            var collectionName = await _client.LoadDataAsync(dataLoader);

            _context.DataSources.Add(new DataSource
            {
                Name = name,
                CollectionName = collectionName,
                DataSourceType = DataSourceType.GitHub,
                DataSourceValue = repoUrl,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return [collectionName];
        }
    }
}