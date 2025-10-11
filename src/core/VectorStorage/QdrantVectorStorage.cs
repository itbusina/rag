using core.Models;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace core.VectorStorage
{
    public class QdrantVectorStorage(string host = "localhost", int grpcPort = 6334) : IVectorStorage
    {
        private readonly QdrantClient _client = new(host, grpcPort);

        public async Task CreateCollectionAsync(string collectionName, ulong vectorSize)
        {
            await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams
                    {
                        Size = vectorSize,
                        Distance = Distance.Cosine
                    });
        }

        public async Task InsertAsync(string collectionName, List<Chunk> chunks)
        {
            var points = chunks.Select(chunk =>
            {
                var point = new PointStruct
                {
                    Id = Guid.NewGuid(),
                    Vectors = chunk.Embedding,
                    Payload =
                    {
                        ["text"] = chunk.Content,
                        ["source_type"] = chunk.Type.ToString(),
                        ["source_value"] = chunk.Value
                    }
                };

                // extend the payload with chunk metadata
                foreach (var metadata in chunk.Metadata)
                {
                    point.Payload.Add(metadata.Key, metadata.Value);
                }

                return point;
            }).ToList();

            var updateResult = await _client.UpsertAsync(collectionName, points);
        }

        public async Task<IEnumerable<Chunk>> SearchAsync(List<string> collections, float[] query, int limit = 3)
        {
            var results = new List<ScoredPoint>();
            foreach (var collection in collections)
            {
                var result = await _client.SearchAsync(collection, query, limit: (ulong)limit);
                results.AddRange(result);
            }

            var topResults = results
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .ToList();

            return [.. topResults.Select(p => new Chunk
            {
                Content = p.Payload["text"].StringValue ?? string.Empty,
                Type = Enum.Parse<DataSourceType>(p.Payload["source_type"].StringValue ?? string.Empty),
                Value = p.Payload["source_value"].StringValue ?? string.Empty,
                Metadata = p.Payload.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.StringValue)).ToDictionary()
            })];
        }
    }
}