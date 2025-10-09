using System.Text.RegularExpressions;
using core.Models;

namespace core.Data
{
    public static class TextChunker
    {
        public static List<Chunk> ChunkText(string text, int maxTokens, int overlap)
        {
            // Simple chunker based on sentence boundaries
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
            var chunks = new List<Chunk>();

            var current = new List<string>();
            int tokenEstimate = 0;

            foreach (var sentence in sentences)
            {
                int tokens = sentence.Split(' ').Length; // rough token estimate
                if (tokenEstimate + tokens > maxTokens)
                {
                    chunks.Add(new Chunk { Content = string.Join(" ", current) });
                    current.Clear();
                    tokenEstimate = 0;
                }
                current.Add(sentence);
                tokenEstimate += tokens;
            }

            if (current.Count > 0)
                chunks.Add(new Chunk { Content = string.Join(" ", current) });

            // Add overlapping context (optional)
            for (int i = 1; i < chunks.Count; i++)
            {
                var overlapText = string.Join(" ", chunks[i - 1].Content.Split(' ').TakeLast(overlap));
                chunks[i].Content = overlapText + " " + chunks[i].Content;
            }

            return chunks;
        }
    }
}