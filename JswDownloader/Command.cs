using System.Runtime.InteropServices;

namespace MyApp // Note: actual namespace depends on the project name.
{


    public enum CommandType
    {
        RequestFileInfo,
        ResponseFileInfo,
        RequestBlock,
        ResponseBlock,
        EndConnection
    }
    public struct Command
    {
        public CommandType commandType;

        public int parameter1;

        public int parameter2;

        public int parameter3;

        public int parameter4;

        public byte[] ToBytes()
        {
            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(Command))];
            GCHandle pinStructure = GCHandle.Alloc(this, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                pinStructure.Free();
            }
        }


    }

}
