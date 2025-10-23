namespace core.Chunking
{
    public interface ITextChunker
    {
        List<string> ChunkText(string text);
    }
}