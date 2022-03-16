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
        byte[] _dataContent;
        JswFileInfo _fileInfo;
        ArraySegment<byte> arraySegment;
        SHA256 mySHA256;

        public DownloadManager(string fileName)
        {
            CreateFileInfo(fileName);
            SHA256 mySHA256 = SHA256.Create();
        }

        public JswFileInfo GetFileInfo()
        {
            return _fileInfo;
        }

        public byte[] GetDataBlock(int blockIndex)
        {
            int start = (int)_fileInfo.blockStart[blockIndex];
            int end = (int)_fileInfo.blockEnd[blockIndex];
            return (arraySegment.Slice(start, end - start)).ToArray();
        }

        public bool WriteDataBlock(int blockIndex, byte[] data)
        {
            int tmp=BitConverter.ToInt32(mySHA256.ComputeHash(data, 0, data.Length));
            if (tmp != _fileInfo.blockMap[blockIndex])
            {
                return false;
            }
            data.CopyTo(_dataContent, blockIndex * _blockSize);
            return true;
        }

        private JswFileInfo CreateFileInfo(string fileName)
        {
            _dataContent = File.ReadAllBytes(fileName);

            arraySegment = new ArraySegment<byte>(_dataContent);

            _fileInfo = new JswFileInfo();
            _fileInfo.fileName = fileName;
            _fileInfo.fileSize = _dataContent.Length;
            _fileInfo.blockSize = _blockSize;
            _fileInfo.totalBlocks = (int)Math.Ceiling((double)_dataContent.Length / _blockSize);
            _fileInfo.ownedBlocks = (int)Math.Ceiling((double)_dataContent.Length / _blockSize);

            _fileInfo.blockMap = new int?[_fileInfo.totalBlocks];
            _fileInfo.blockStart = new int?[_fileInfo.totalBlocks];
            _fileInfo.blockEnd = new int?[_fileInfo.totalBlocks];

            int i = 0;
            for (i = 0; i < _fileInfo.totalBlocks - 1; i++)
            {
                _fileInfo.blockStart[i] = i * _blockSize;
                _fileInfo.blockEnd[i] = i * _blockSize + _blockSize;
                _fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(_dataContent, i * _blockSize, _blockSize));
            }
            _fileInfo.blockStart[i] = i * _blockSize;
            _fileInfo.blockEnd[i] = _fileInfo.fileSize - i * _blockSize;
            _fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(_dataContent, i * _blockSize, fileInfo.fileSize - i * _blockSize));

            return _fileInfo;
        }

        public string ToJason<T>(T obj)
        {
            return JsonSerializer.Serialize<T>(obj);
        }

        public T ToInstance<T>(string jsn)
        {
            return JsonSerializer.Deserialize<T>(jsn);
        }
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

    }
}
