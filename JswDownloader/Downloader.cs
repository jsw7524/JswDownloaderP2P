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

        public Downloader(DownloadManager d)
        {
            _downloadManager = d;
            random = new Random();
        }

        public async Task<JswFileInfo> GetFileInfo(NetworkStream ns, DownloadManager dm)
        {
            Command cmdGetFileInfo = new Command() { commandType = CommandType.RequestFileInfo };
            await ns.WriteAsync(cmdGetFileInfo.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            Byte[] ResponseBytes = new Byte[Marshal.SizeOf(typeof(Command))];
            await ns.ReadAsync(ResponseBytes, 0, Marshal.SizeOf(typeof(Command)));
            Command ResponseCommand = (Command)DownloadManager.BytesToStruct(ResponseBytes, typeof(Command));
            byte[] byteFileInfo = new byte[ResponseCommand.parameter1];
            await ns.ReadAsync(byteFileInfo, 0, ResponseCommand.parameter1);
            return dm.ToInstance<JswFileInfo>(Encoding.UTF8.GetString(byteFileInfo)); ;
        }

        public async Task<bool> GetDataBlock(NetworkStream ns, DownloadManager dm, int i)
        {
            Command cmdGetBlock = new Command() { commandType = CommandType.RequestBlock, parameter1 = i };
            await ns.WriteAsync(cmdGetBlock.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            byte[] ResponseBytes = new byte[(int)(dm._originalFileInfo.blockEnd[i] - dm._originalFileInfo.blockStart[i])];
            await ns.ReadAsync(ResponseBytes, 0, ResponseBytes.Length);
            return dm.WriteDataBlock(i, ResponseBytes);
        }

        public async Task EndConnection(NetworkStream ns)
        {
            Command cmdGetBlock = new Command() { commandType = CommandType.EndConnection };
            await ns.WriteAsync(cmdGetBlock.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.DisconnectFromServer, message = "Disconnect from a peer." });
        }


        public async Task<bool> DownloadFileAsync(string ip, int port)
        {
            try
            {
                bool isSuccess = false;
                IPAddress address = IPAddress.Parse(ip);
                using (TcpClient client = new TcpClient())
                {
                    if (!client.ConnectAsync(address, port).Wait(1000))
                    {
                        _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Fail to connect the Server." });
                        return isSuccess;
                    }
                    _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.ConnectToServer, message = "connecting to a peer." });
                    //Debugger.Launch();
                    using (NetworkStream requestStream = client.GetStream())
                    {
                        JswFileInfo remoteFileInfo = await GetFileInfo(requestStream, _downloadManager);


                        _downloadManager._ownedFileInfo.peers=_downloadManager._ownedFileInfo.peers.Union(remoteFileInfo.peers).ToList(); 
                        _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.FindNewPeer });

                        int randomStartBlcok = random.Next(0, _downloadManager._originalFileInfo.totalBlocks - 1);
                        if (null != remoteFileInfo.blockMap[randomStartBlcok])
                        {
                            isSuccess = await GetDataBlock(requestStream, _downloadManager, randomStartBlcok);
                            if (!isSuccess)
                            {
                                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.BadBlock,data1=this.GetHashCode(), data2= requestStream, message = "Disconnecting due to Bad Block" });
                            }
                        }

                        for (int i = (randomStartBlcok + 1) % _downloadManager._originalFileInfo.totalBlocks; i != randomStartBlcok; i = (i + 1) % _downloadManager._originalFileInfo.totalBlocks)
                        {
                            if (null != remoteFileInfo.blockMap[i])
                            {
                                isSuccess = await GetDataBlock(requestStream, _downloadManager, i);
                                if (!isSuccess)
                                {
                                    _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.BadBlock, data1 = this.GetHashCode(), data2 = requestStream, message = "Disconnecting due to Bad Block" });
                                }
                            }
                        }
                        isSuccess = _downloadManager.CheckData(_downloadManager._ownedFileInfo, _downloadManager._dataContent);
                        if (!isSuccess)
                        {
                            _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Check Data failed" });
                        }
                        await EndConnection(requestStream);
                    }

                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}