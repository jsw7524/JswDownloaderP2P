using Born2Code.Net;
using JswDownloader;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace MyApp // Note: actual namespace depends on the project name.
{

    public class FileServer
    {
        private DownloadManager _downloadManager;
        public FileServer(DownloadManager d)
        {
            _downloadManager = d;
        }

        public async Task RespondFileInfo(NetworkStream ns, DownloadManager dm)
        {
            string jsn = dm.ToJason(_downloadManager._ownedFileInfo);
            Command cmdResponseFileInfo = new Command() { commandType = CommandType.ResponseFileInfo, parameter1 = jsn.Length };
            await ns.WriteAsync(cmdResponseFileInfo.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            byte[] responseBytes = Encoding.UTF8.GetBytes(jsn);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        public async Task RespondDataBlock(NetworkStream ns, DownloadManager dm, int i)
        {
            byte[] responseBytes = _downloadManager.GetDataBlock(i);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);
            _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Send Block " + i });
        }

        private async Task DealRequest(TcpListener server)
        {
            try
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Dealing a Request from " + client.Client.RemoteEndPoint });
                byte[] requestCommandByte = new byte[Marshal.SizeOf(typeof(Command))];
                bool jobDone = false;
                using (var tcpStream = (client.GetStream()))
                {
                    while (!jobDone)
                    {
                        await tcpStream.ReadAsync(requestCommandByte, 0, Marshal.SizeOf(typeof(Command)));
                        Command requestCommand = (Command)DownloadManager.BytesToStruct(requestCommandByte, typeof(Command));
                        //Debugger.Launch();
                        switch (requestCommand.commandType)
                        {
                            case CommandType.RequestFileInfo:
                                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Send FileInfo to " + client .Client.RemoteEndPoint});
                                await RespondFileInfo(tcpStream, _downloadManager);
                                break;

                            case CommandType.RequestBlock:
                                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Send DataBlock to " +client.Client.RemoteEndPoint });
                                await RespondDataBlock(tcpStream, _downloadManager, requestCommand.parameter1);
                                break;
                            case CommandType.EndConnection:
                                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc , message = "Disconnect with " + client.Client.RemoteEndPoint });
                                jobDone = true;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public void Start()
        {
            try
            {
                _downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.Misc, message = "Establishing Server." });
                TcpListener server = new TcpListener(IPAddress.Any, 54321);

                server.Start();
                while (true)
                {
                    if (server.Pending())
                    {
                        DealRequest(server);
                    }
                }
                server.Stop();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}