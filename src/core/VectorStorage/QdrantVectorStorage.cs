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
                        ["source_type"] = chunk.SourceType.ToString(),
                        ["source_value"] = chunk.SourceValue
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

        public async Task<IEnumerable<Chunk>> SearchAsync(string collectionName, float[] query, ulong limit = 3)
        {
            var points = await _client.SearchAsync(collectionName, query, limit: limit);

            return [.. points.Select(p => new Chunk
            {
                Content = p.Payload["text"].ToString() ?? string.Empty,
                SourceType = Enum.Parse<SourceType>(p.Payload["source_type"].ToString() ?? string.Empty),
                SourceValue = p.Payload["source_value"].ToString() ?? string.Empty,
                Metadata = p.Payload.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())).ToDictionary()
            })];
        }
    }
}