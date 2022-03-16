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
        public FileServer(string fileNmae)
        {
            _downloadManager = new DownloadManager();
            _downloadManager.CreateFileInfo(fileNmae);
        }

        public async Task RespondFileInfo(NetworkStream ns, DownloadManager dm)
        {
            string jsn = dm.ToJason(_downloadManager.GetFileInfo());
            Command cmdResponseFileInfo = new Command() { commandType = CommandType.ResponseFileInfo, parameter1 = jsn.Length };
            await ns.WriteAsync(cmdResponseFileInfo.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            byte[] responseBytes = Encoding.UTF8.GetBytes(jsn);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        public async Task RespondDataBlock(NetworkStream ns, DownloadManager dm, int i)
        {
            Command cmdResponseDataBlock = new Command() { commandType = CommandType.RequestBlock };
            await ns.WriteAsync(cmdResponseDataBlock.ToBytes(), 0, Marshal.SizeOf(typeof(Command)));
            byte[] responseBytes = _downloadManager.GetDataBlock(i);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);

        }

        private async Task DealRequest(TcpListener server)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            byte[] requestCommandByte = new byte[Marshal.SizeOf(typeof(Command))];
            string requestMessage;
            byte[] responseMessage;

            using (var tcpStream = (client.GetStream()))
            {
                await tcpStream.ReadAsync(requestCommandByte, 0, Marshal.SizeOf(typeof(Command)));
                Command requestCommand = (Command)DownloadManager.BytesToStruct(requestCommandByte, typeof(Command));
                //Debugger.Launch();
                switch (requestCommand.commandType)
                {
                    case CommandType.RequestFileInfo:
                        await RespondFileInfo(tcpStream, _downloadManager);
                        break;

                    case CommandType.RequestBlock:
                        await RespondDataBlock(tcpStream, _downloadManager, requestCommand.parameter1);
                        break;
                }
            }
            client.Close();
        }


        public async void Start()
        {
            try
            {
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