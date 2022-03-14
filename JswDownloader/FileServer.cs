using Born2Code.Net;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class FileServer
    {



        private async Task DealRequest(TcpListener server)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            byte[] bytes = new byte[256];
            string requestMessage;
            byte[] responseMessage;
            //using (var tcpStream = new ThrottledStream(client.GetStream(), 51200))
            using (var tcpStream = (client.GetStream()))
            {
                await tcpStream.ReadAsync(bytes, 0, bytes.Length);
                requestMessage = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty);

                Console.WriteLine();
                Console.WriteLine("Message Received From Client:");
                Console.WriteLine(requestMessage);
                byte[] requestedfile = File.ReadAllBytes(requestMessage);
                Console.WriteLine("read file OK!");
                responseMessage = Encoding.UTF8.GetBytes(requestedfile.Length.ToString());
                await tcpStream.WriteAsync(responseMessage, 0, responseMessage.Length);
                Console.WriteLine("send file size OK!");
                await tcpStream.WriteAsync(requestedfile, 0, requestedfile.Length);
                Console.WriteLine("send file OK!");
            }
            client.Close();
            //Thread.Sleep(10000);
            Console.WriteLine(requestMessage + " OK!");
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