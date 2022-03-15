using JswDownloader;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Downloader
    {
        DownloadManager _downloadManager;

        public Downloader()
        {
            _downloadManager = new DownloadManager();
        }

        public async Task<JswFileInfo> GetFileInfo(NetworkStream rs, DownloadManager dm)
        {
            byte[] cmdGetFileInfo = Encoding.UTF8.GetBytes("GetFileInfo");
            await rs.WriteAsync(cmdGetFileInfo, 0, cmdGetFileInfo.Length);
            var sizeInfoBytes = new byte[4];
            await rs.ReadAsync(sizeInfoBytes, 0, 4);
            int sizeInfo = BitConverter.ToInt32(sizeInfoBytes);
            Console.WriteLine(sizeInfo);
            var infoFileBytes = new byte[sizeInfo];
            await rs.ReadAsync(infoFileBytes, 0, infoFileBytes.Length);
            string jsn = Encoding.UTF8.GetString(infoFileBytes);
            return _downloadManager.ToInstance<JswFileInfo>(jsn);
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

                        int ikkk = 0;
                        //await requestStream.WriteAsync(bytes, 0, bytes.Length);
                        //Console.WriteLine("file request");

                        //var responseBytes = new byte[256];
                        //await requestStream.ReadAsync(responseBytes, 0, responseBytes.Length);

                        //Console.WriteLine("file size"+Encoding.UTF8.GetString(responseBytes));

                        //int sizeFile = int.Parse(Encoding.UTF8.GetString(responseBytes));
                        //byte[] buffer = new byte[sizeFile];
                        //await requestStream.ReadAsync(buffer, 0, sizeFile);
                        //Console.WriteLine("get file Data");
                        //using (var destination = new FileStream(DateTime.Now.ToString("yyyyMMddhhmmss") + filename, FileMode.Create))
                        //{
                        //    await destination.WriteAsync(buffer, 0, buffer.Length);
                        //    destination.Flush();

                        //}
                        //Console.WriteLine("save file");
                        //StreamReader sr = new StreamReader(requestStream);
                        //sr.Read()

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