using MyApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JswDownloader
{
    public class DownloadManager
    {
        public bool _seeding=false;

        public Queue<MessageInfo> messages = new Queue<MessageInfo>();

        public int _blockSize = 2 * 1024 * 1024;
        public byte[] _dataContent;
        public JswFileInfo _originalFileInfo;
        public JswFileInfo _ownedFileInfo;

        public ArraySegment<byte> arraySegment;
        public SHA256 mySHA256;

        public DownloadManager()
        {
            mySHA256 = SHA256.Create();
        }

        public JswFileInfo CreateOwnedFileInfo(JswFileInfo origin)
        {
            JswFileInfo tmp=origin.ShallowCopy();
            tmp.blockMap = new int?[tmp.totalBlocks];
            tmp.ownedBlocks = 0;
            return tmp;
        }
        public List<string> GetLocalIPAddress()
        {
            List<string> tmp= new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    tmp.Add(ip.ToString());
                }
            }
            return tmp;
        }


        public JswFileInfo ReadFileInfo(string fileName)
        {
            return ReadFileInfo(fileName, _originalFileInfo);
        }

        public JswFileInfo ReadFileInfo(string fileName, JswFileInfo fileInfo)
        {
            string jsn=File.ReadAllText(fileName);
            fileInfo = ToInstance<JswFileInfo>(jsn);
            return fileInfo;
        }
        public void SaveFileInfo(JswFileInfo fileInfo, string savefileName= "TestFileInfo.txt")
        {
            string jsn = ToJason(fileInfo);
            File.WriteAllText(savefileName, jsn);
        }

        public byte[] GetDataBlock(int blockIndex)
        {
            return GetDataBlock(blockIndex, _ownedFileInfo);
        }

        public byte[] GetDataBlock(int blockIndex, JswFileInfo fileInfo)
        {
            int start = (int)fileInfo.blockStart[blockIndex];
            int end = (int)fileInfo.blockEnd[blockIndex];
            return (arraySegment.Slice(start, end - start)).ToArray();
        }

        public bool WriteDataBlock(int blockIndex, byte[] data)
        {
            return WriteDataBlock(blockIndex, data, _originalFileInfo, _ownedFileInfo, _dataContent);
        }

        public bool WriteDataBlock(int blockIndex, byte[] data, JswFileInfo soruceFileInfo, JswFileInfo fileInfo, byte[] WriteToDataArea)
        {
            int tmp=BitConverter.ToInt32(mySHA256.ComputeHash(data, 0, data.Length));
            if (tmp != soruceFileInfo.blockMap[blockIndex])
            {
                messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "hash dismatched and Discard downloading block" });
                return false;
            }
            data.CopyTo(WriteToDataArea, blockIndex * _blockSize);

            fileInfo.blockMap[blockIndex] = tmp;
            fileInfo.ownedBlocks += 1;
            return true;
        }

        public JswFileInfo CreateFileInfo(string fileName)
        {
            _originalFileInfo=CreateFileInfo(fileName, ref _dataContent);
            _ownedFileInfo = _originalFileInfo;
            _originalFileInfo.peers = new List<string>();
#if DEBUG
            //_originalFileInfo.peers.Add("127.0.0.1");
#endif
            _originalFileInfo.peers.AddRange(GetLocalIPAddress());
            File.WriteAllTextAsync(fileName+".fif", ToJason(_originalFileInfo));
            return _originalFileInfo;
        }

        public bool CheckData(JswFileInfo localOwnedFileInfo,byte[] data)
        {
            for (int i = 0; i < localOwnedFileInfo.blockMap.Count(); i++)
            {
                if (null == localOwnedFileInfo.blockMap[i])
                {
                    return false;
                }
            }
            bool done = localOwnedFileInfo.ownedBlocks == localOwnedFileInfo.totalBlocks ? true : false;
            if (done)
            {
                messages.Enqueue(new MessageInfo() { type = MessageType.DownloadFileCompleted, message= "Download file Completed." });
            }
            return done;
        }


        public JswFileInfo CreateFileInfo(string fileName,ref byte[] dataArea)
        {

            dataArea = File.ReadAllBytes(fileName);

            arraySegment = new ArraySegment<byte>(dataArea);

            JswFileInfo fileInfo = new JswFileInfo();
            fileInfo.fileName = fileName;
            fileInfo.fileSize = dataArea.Length;
            fileInfo.blockSize = _blockSize;
            fileInfo.totalBlocks = (int)Math.Ceiling((double)dataArea.Length / _blockSize);
            fileInfo.ownedBlocks = (int)Math.Ceiling((double)dataArea.Length / _blockSize);

            fileInfo.blockMap = new int?[fileInfo.totalBlocks];
            fileInfo.blockStart = new int?[fileInfo.totalBlocks];
            fileInfo.blockEnd = new int?[fileInfo.totalBlocks];

            int i = 0;
            for (i = 0; i < fileInfo.totalBlocks - 1; i++)
            {
                fileInfo.blockStart[i] = i * _blockSize;
                fileInfo.blockEnd[i] = i * _blockSize + _blockSize -1 ;
                fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(dataArea, i * _blockSize, _blockSize-1)); //index from 0
            }
            fileInfo.blockStart[i] = i * _blockSize;
            fileInfo.blockEnd[i] = fileInfo.fileSize-1;
            fileInfo.blockMap[i] = BitConverter.ToInt32(mySHA256.ComputeHash(dataArea, i * _blockSize, (fileInfo.fileSize - 1) - i * _blockSize));

            _ownedFileInfo = fileInfo;

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
