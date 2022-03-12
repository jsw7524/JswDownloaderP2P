using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Downloader
    {
        public async Task<bool> DownloadFileAsync(string ip, int port, string filename)
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
                    var bytes = Encoding.UTF8.GetBytes(filename);
                    using (var requestStream = client.GetStream())
                    //using (MemoryStream ms = new MemoryStream())
                    
                    {
                        await requestStream.WriteAsync(bytes, 0, bytes.Length);
                        var responseBytes = new byte[256];
                        await requestStream.ReadAsync(responseBytes, 0, responseBytes.Length);

                        Console.WriteLine(Encoding.UTF8.GetString(responseBytes));

                        int sizeFile = int.Parse(Encoding.UTF8.GetString(responseBytes));
                        byte[] buffer = new byte[sizeFile];
                        await requestStream.ReadAsync(buffer, 0, sizeFile);
                        using (var destination = new FileStream(DateTime.Now.ToString("yyyyMMddhhmmss") + filename, FileMode.Create))
                        {
                            await destination.WriteAsync(buffer, 0, buffer.Length);
                            destination.Flush();

                        }
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