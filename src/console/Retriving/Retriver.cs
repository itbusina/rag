using console.Embeddings;
using console.Models;

namespace console.Retriving
{
    public class Retriver(IEmbedder embedder)
    {
        private readonly IEmbedder _embedder = embedder;

        public async Task<List<Chunk>> GetTopKChunks(List<Chunk> chunks, string query, int k = 3)
        {
            var queryEmbedding = await _embedder.GetEmbedding(query);

            var topChunks = chunks
                .Select(c => new { Chunk = c, Score = SimilarityHelper.CosineSimilarity(c.Embedding, queryEmbedding) })
                .OrderByDescending(x => x.Score)
                .Take(k)
                .Select(x => x.Chunk)
                .ToList();

            return topChunks;
        }
    }
}