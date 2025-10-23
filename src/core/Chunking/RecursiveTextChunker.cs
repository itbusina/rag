using LangChain.Splitters.Text;

namespace core.Chunking
{
    public class RecursiveTextChunker(int chunkSize, int overlap) : ITextChunker
    {

        private readonly RecursiveCharacterTextSplitter _splitter = new(
                                    separators: ["\n\n", "\n", " ", ""],
                                    chunkSize: chunkSize,
                                    chunkOverlap: overlap
                                );

        public List<string> ChunkText(string text)
        {
            var chunks = _splitter.SplitText(text).ToList();
            return chunks;
        }
    }
}