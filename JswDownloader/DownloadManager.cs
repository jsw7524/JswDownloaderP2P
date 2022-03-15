using MyApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JswDownloader
{
    public class DownloadManager
    {
        int _blockSize = 2 * 1024 * 1024;

        public static object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public JswFileInfo CreateFileInfo(string fileName)
        {
            byte[] content = File.ReadAllBytes(fileName);
            JswFileInfo fileInfo = new JswFileInfo();
            fileInfo.fileName = fileName;
            fileInfo.fileSize = content.Length;
            fileInfo.blockSize = _blockSize;
            fileInfo.totalBlocks = (int)Math.Ceiling((double)content.Length / _blockSize);
            fileInfo.ownedBlocks = (int)Math.Ceiling((double)content.Length / _blockSize);

            fileInfo.blockMap = new int?[fileInfo.totalBlocks];
            fileInfo.blockStart = new int?[fileInfo.totalBlocks];
            fileInfo.blockEnd = new int?[fileInfo.totalBlocks];
            using (SHA256 mySHA256 = SHA256.Create())
            {
                int i = 0;
                for (i = 0; i < fileInfo.totalBlocks - 1; i++)
                {
                    fileInfo.blockStart[i] = i * _blockSize;
                    fileInfo.blockEnd[i] = i * _blockSize + _blockSize;
                    fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(content, i * _blockSize, _blockSize));
                }
                fileInfo.blockStart[i] = i * _blockSize;
                fileInfo.blockEnd[i] = fileInfo.fileSize - i * _blockSize;
                fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(content, i * _blockSize, fileInfo.fileSize - i * _blockSize));
            }
            return fileInfo;
        }

        public string ToJason<T>(T obj)
        {
            return JsonSerializer.Serialize<T>(obj);
        }

        public T ToInstance<T>(string jsn)
        {
            return JsonSerializer.Deserialize<T>(jsn);
        }


    }
}
