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
        JswFileInfo _originalFileInfo;

        JswFileInfo _ownedFileInfo;

        ArraySegment<byte> arraySegment;
        SHA256 mySHA256;

        public DownloadManager()
        {
            //CreateFileInfo(fileName);
            mySHA256 = SHA256.Create();
        }

        public JswFileInfo GetOwnedFileInfo()
        {
            return _ownedFileInfo;
        }

        public JswFileInfo ReadFileInfo(string fileName)
        {
            string jsn=File.ReadAllText(fileName);
            _originalFileInfo=ToInstance<JswFileInfo>(jsn);
            return _originalFileInfo;
        }
        public void SaveFileInfo(JswFileInfo fileInfo, string savefileName= "TestFileInfo.txt")
        {
            string jsn = ToJason(fileInfo);
            File.WriteAllText(savefileName, jsn);
        }

        public byte[] GetDataBlock(int blockIndex)
        {
            int start = (int)_originalFileInfo.blockStart[blockIndex];
            int end = (int)_originalFileInfo.blockEnd[blockIndex];
            return (arraySegment.Slice(start, end - start)).ToArray();
        }

        public bool WriteDataBlock(int blockIndex, byte[] data)
        {
            int tmp=BitConverter.ToInt32(mySHA256.ComputeHash(data, 0, data.Length));
            if (tmp != _originalFileInfo.blockMap[blockIndex])
            {
                return false;
            }
            data.CopyTo(_dataContent, blockIndex * _blockSize);

            _ownedFileInfo.blockMap[blockIndex] = tmp;
            _ownedFileInfo.ownedBlocks += 1;
            return true;
        }

        public JswFileInfo CreateFileInfo(string fileName)
        {

            _dataContent = File.ReadAllBytes(fileName);

            arraySegment = new ArraySegment<byte>(_dataContent);

            _originalFileInfo = new JswFileInfo();
            _originalFileInfo.fileName = fileName;
            _originalFileInfo.fileSize = _dataContent.Length;
            _originalFileInfo.blockSize = _blockSize;
            _originalFileInfo.totalBlocks = (int)Math.Ceiling((double)_dataContent.Length / _blockSize);
            _originalFileInfo.ownedBlocks = (int)Math.Ceiling((double)_dataContent.Length / _blockSize);

            _originalFileInfo.blockMap = new int?[_originalFileInfo.totalBlocks];
            _originalFileInfo.blockStart = new int?[_originalFileInfo.totalBlocks];
            _originalFileInfo.blockEnd = new int?[_originalFileInfo.totalBlocks];

            int i = 0;
            for (i = 0; i < _originalFileInfo.totalBlocks - 1; i++)
            {
                _originalFileInfo.blockStart[i] = i * _blockSize;
                _originalFileInfo.blockEnd[i] = i * _blockSize + _blockSize;
                _originalFileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(_dataContent, i * _blockSize, _blockSize));
            }
            _originalFileInfo.blockStart[i] = i * _blockSize;
            _originalFileInfo.blockEnd[i] = _originalFileInfo.fileSize - i * _blockSize;
            _originalFileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(_dataContent, i * _blockSize, _originalFileInfo.fileSize - i * _blockSize));

            _ownedFileInfo = _originalFileInfo;

            return _originalFileInfo;
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
