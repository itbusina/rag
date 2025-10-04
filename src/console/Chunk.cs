namespace console
{
    public class Chunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = [];
    }
}