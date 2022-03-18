namespace MyApp // Note: actual namespace depends on the project name.
{
    public class JswFileInfo
    {
        public string fileName { get; set; }
        public int fileSize { get; set; }
        public int totalBlocks { get; set; }
        public int ownedBlocks { get; set; }
        public int blockSize { get; set; }
        public int?[] blockMap { get; set; }
        public int?[] blockStart { get; set; }
        public int?[] blockEnd { get; set; }
        public List<string> peers { get; set; }

        public JswFileInfo ShallowCopy()
        {
            return MemberwiseClone() as JswFileInfo;
        }
    }
}