namespace console.Models
{
    public class Chunk
    {
        public float[] Embedding { get; set; } = [];
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}