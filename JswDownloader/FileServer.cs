using Born2Code.Net;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class FileServer
    {
        private async Task DealRequest(TcpClient client)
        {
            byte[] bytes = new byte[256];
            using (var tcpStream = new ThrottledStream(client.GetStream(), 51200))
            //using (var tcpStream = (client.GetStream()))
            {
                await tcpStream.ReadAsync(bytes, 0, bytes.Length);
                var requestMessage = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty);

                Console.WriteLine();
                Console.WriteLine("Message Received From Client:");
                Console.WriteLine(requestMessage);
                var requestedfile = File.ReadAllBytes(requestMessage);
                var responseMessage = Encoding.UTF8.GetBytes(requestedfile.Length.ToString());
                await tcpStream.WriteAsync(responseMessage, 0, responseMessage.Length);

                await tcpStream.WriteAsync(requestedfile, 0, requestedfile.Length);

            }

        }


        public async void Start()
        {
            try
            {
                bool done = false;
                int port = 54321;
                IPAddress address = IPAddress.Any;
                TcpListener server = new TcpListener(address, port);
                server.Start();
                var loggedNoRequest = false;

                while (!done)
                {
                    if (!server.Pending())
                    {
                        if (!loggedNoRequest)
                        {
                            Console.WriteLine();
                            Console.WriteLine("No pending requests as of yet");
                            Console.WriteLine("Server listening...");
                            loggedNoRequest = true;
                        }
                    }
                    else
                    {
                        loggedNoRequest = false;
                        byte[] bytes = new byte[256];

                        using (var client = await server.AcceptTcpClientAsync())
                        {
                            using (var tcpStream = new ThrottledStream(client.GetStream(), 51200))
                            //using (var tcpStream = (client.GetStream()))
                            {
                                await tcpStream.ReadAsync(bytes, 0, bytes.Length);
                                var requestMessage = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty);

                                if (requestMessage.Equals("TERMINATE"))
                                {
                                    done = true;
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Message Received From Client:");
                                    Console.WriteLine(requestMessage);
                                    var requestedfile = File.ReadAllBytes(requestMessage);
                                    var responseMessage = Encoding.UTF8.GetBytes(requestedfile.Length.ToString());
                                    await tcpStream.WriteAsync(responseMessage, 0, responseMessage.Length);

                                    await tcpStream.WriteAsync(requestedfile, 0, requestedfile.Length);
                                }
                            }
                        }
                    }
                }
                server.Stop();
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}