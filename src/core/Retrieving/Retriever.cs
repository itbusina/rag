using core.Models;

namespace core.Retrieving
{
    public class Retriever
    {
        public static List<Chunk> Search(List<Chunk> chunks, float[] query, int limit = 3)
        {
            var topChunks = chunks
                .Select(c => new { Chunk = c, Score = SimilarityHelper.CosineSimilarity(c.Embedding, query) })
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .Select(x => x.Chunk)
                .ToList();

            return topChunks;
        }
    }
}