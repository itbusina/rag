namespace core.Models
{
    public class Chunk
    {
        public float[] Embedding { get; set; } = [];
        public required string Content { get; set; } = string.Empty;
        public required DataSourceType Type { get; set; }
        public required string Value { get; set; }
        public required Dictionary<string, string> Metadata { get; set; } = [];
    }
}