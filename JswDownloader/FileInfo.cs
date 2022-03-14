namespace MyApp // Note: actual namespace depends on the project name.
{
    public class JswFileInfo
    {
        public string fileName;
        public int fileSize;
        public int totalBlocks;
        public int ownedBlocks;
        public int blockSize;
        public int?[] blockMap;
        public List<string> peers;
    }
}