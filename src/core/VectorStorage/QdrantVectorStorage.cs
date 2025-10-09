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
            var points = chunks.Select(c => new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = c.Embedding,
                Payload =
                {
                    ["text"] = c.Content,
                }
            }).ToList();

            var updateResult = await _client.UpsertAsync(collectionName, points);
        }

        public async Task<IEnumerable<Chunk>> SearchAsync(string collectionName, float[] query, ulong limit = 3)
        {
            var points = await _client.SearchAsync(collectionName, query, limit: limit);

            return [.. points.Select(p => new Chunk
            {
                Content = p.Payload["text"].ToString() ?? string.Empty,
            })];
        }
    }
}