namespace MyApp // Note: actual namespace depends on the project name.
{
    public enum CommandType
    {
        RequestFileInfo,
        ResponseFileInfo,
        RequestBlock,
        ResponseBlock,
    }
    public struct Command
    {
        public CommandType command;

        public int parameter1;

        public int parameter2;

        public int parameter3;

        public int parameter4;
    }
}