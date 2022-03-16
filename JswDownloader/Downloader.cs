using JswDownloader;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Downloader
    {
        DownloadManager _downloadManager;
        Random random;

        public Downloader()
        {
            _downloadManager = new DownloadManager();
            random=new Random();
        }

        public async Task<JswFileInfo> GetFileInfo(NetworkStream ns, DownloadManager dm)
        {
            Command cmdGetFileInfo = new Command() { commandType = CommandType.RequestFileInfo};
            await ns.WriteAsync(cmdGetFileInfo.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            Byte[] ResponseBytes = new Byte[Marshal.SizeOf(typeof(Command))];
            await ns.ReadAsync(ResponseBytes, 0, Marshal.SizeOf(typeof(Command)));
            Command ResponseCommand = (Command)DownloadManager.BytesToStruct(ResponseBytes, typeof(Command));
            byte[] byteFileInfo = new byte[ResponseCommand.parameter1];
            await ns.ReadAsync(byteFileInfo, 0, ResponseCommand.parameter1);
            return dm.ToInstance<JswFileInfo>(Encoding.UTF8.GetString(byteFileInfo)); ;
        }

        public async Task GetDataBlock(NetworkStream ns, DownloadManager dm, JswFileInfo remoteFileInfo, int i)
        {
            Command cmdGetBlock = new Command() { commandType = CommandType.RequestBlock, parameter1=i };
            await ns.WriteAsync(cmdGetBlock.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
        }

        public async Task<bool> DownloadFileAsync(string ip, int port)
        {
            try
            {
                IPAddress address = IPAddress.Parse(ip);
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(address, port);
                    if (client.Connected)
                    {
                        Console.WriteLine("We've connected from the client");
                    }
                    //Debugger.Launch();
                    using (NetworkStream requestStream = client.GetStream())
                    {

                        JswFileInfo fileInfo = await GetFileInfo(requestStream, _downloadManager);

                        int randomStartBlcok = random.Next(0, fileInfo.totalBlocks -1);

                        for (int i = (randomStartBlcok+1) % fileInfo.totalBlocks; i != fileInfo.totalBlocks; i = (i + 1) % fileInfo.totalBlocks)
                        {
                            if (null != fileInfo.blockMap[i])
                            {
                                await GetDataBlock(requestStream, _downloadManager, fileInfo, i);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}