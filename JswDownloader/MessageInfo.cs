using JswDownloader;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public enum MessageType
    {
        EstablishServer,
        ConnectToServer,
        DisconnectFromServer,
        DownloadFileCompleted,
        GetFileIndo,
        GetDataBlock,
        FindNewPeer,
        ClientLeave,
        Misc
    }


    public class MessageInfo
    {
        public MessageType type;
        public string message;
        public int data;
    }


    public class MessageInfoManager
    {


        DownloadManager _downloadManager;
        FileServer _fileServer;
        List<Downloader> _downloaders;


        List<MessageInfoHandler> messageInfoHandlers;
        public MessageInfoManager(DownloadManager d, FileServer fs, List<Downloader> ld)
        {
            _downloadManager = d;
            _fileServer = fs;
            _downloaders = ld;
            messageInfoHandlers = new List<MessageInfoHandler>();
            messageInfoHandlers.Add(new MiscHandler());
            messageInfoHandlers.Add(new EstablishServerHandler());
            messageInfoHandlers.Add(new ConnectToServerHandler());
            messageInfoHandlers.Add(new DisconnectFromServerHandler());
            messageInfoHandlers.Add(new DownloadFileCompletedHandler(d));
            messageInfoHandlers.Add(new FindNewPeerHandler(_downloaders, _downloadManager));
        }

        public void RunHandlers()
        {

            //MessageInfo messageInfo = _downloadManager.messages.Dequeue();

            if (_downloadManager.messages.Any())
            {
                MessageInfo messageInfo = _downloadManager.messages.Dequeue();
                foreach (MessageInfoHandler handler in messageInfoHandlers)
                {
                    handler.Deal(messageInfo);
                }
            }
        }
    }

    public interface MessageInfoHandler
    {
        public void Deal(MessageInfo message);
    }

    public class MiscHandler : MessageInfoHandler
    {
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.Misc)
            {
                Console.WriteLine(message.message);
            }
        }
    }

    public class EstablishServerHandler : MessageInfoHandler
    {
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.EstablishServer)
            {
                Console.WriteLine("Establish Server");
            }
        }
    }

    public class ConnectToServerHandler : MessageInfoHandler
    {
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.ConnectToServer)
            {
                Console.WriteLine("Connect To Server");
            }
        }
    }

    public class DisconnectFromServerHandler : MessageInfoHandler
    {
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.DisconnectFromServer)
            {
                Console.WriteLine("Disconnect From Server");
            }
        }
    }

    public class DownloadFileCompletedHandler : MessageInfoHandler
    {
        DownloadManager _dm;
        public DownloadFileCompletedHandler(DownloadManager dm)
        {
            _dm=dm;
        }
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.DownloadFileCompleted)
            {
                File.WriteAllBytes(DateTime.Now.ToString("yyyyMMddhhmmss") + _dm._originalFileInfo.fileName, _dm._dataContent);

                Console.WriteLine("Download File Completed");
            }
        }
    }

    public class FindNewPeerHandler : MessageInfoHandler
    {
        static int numberOfPeers = 0;
        List<Downloader> _ld;
        DownloadManager _dm;
        object _obj=false;
        Random _random = new Random();
        public FindNewPeerHandler(List<Downloader> ld, DownloadManager dm)
        {
            _ld = ld;
            _dm = dm;
        }
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.FindNewPeer)
            {
                if (numberOfPeers < 1)
                {
                    Console.WriteLine("Find New Peer");
                    Downloader downloader = new Downloader(_dm);
                    lock (_obj)
                    {
                        _ld.Add(downloader);
                    }

                    Task.Run(async () =>
                    {
                        while (!await downloader.DownloadFileAsync(_dm._ownedFileInfo.peers[_random.Next(_dm._ownedFileInfo.peers.Count)], 54321)) ;
                    });
                    numberOfPeers++;
                }




            }
        }
    }



}
