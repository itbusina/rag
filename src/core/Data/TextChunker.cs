using System.Text.RegularExpressions;

namespace core.Data
{
    public static class TextChunker
    {
        public static List<string> ChunkText(string text, int maxSentences = 5, int overlap = 1)
        {
            // Simple chunker based on sentence boundaries
            var sentencesRegex = @"(?<=[.!?])\s+";

            var sentences = Regex.Split(text, sentencesRegex);
            var chunks = new List<string>();
            var current = new List<string>();
            int estimate = 0;

            foreach (var sentence in sentences)
            {
                if (estimate > maxSentences)
                {
                    chunks.Add(string.Join(" ", current));
                    current.Clear();
                    estimate = 0;
                }

                current.Add(sentence);
                estimate += 1;
            }

            if (current.Count > 0)
                chunks.Add(string.Join(" ", current));

            // Add overlapping context (optional)
            for (int i = 1; i < chunks.Count; i++)
            {
                var overlapSentences = string.Join(" ", Regex.Split(chunks[i - 1], sentencesRegex).TakeLast(overlap));
                chunks[i] = overlapSentences + " " + chunks[i];
            }

            return chunks;
        }
    }
}