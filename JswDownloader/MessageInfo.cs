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
        GetDataBlock
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
        List<MessageInfoHandler> messageInfoHandlers;
        public MessageInfoManager(DownloadManager d)
        {
            _downloadManager = d;
            messageInfoHandlers = new List<MessageInfoHandler>();
            messageInfoHandlers.Add(new EstablishServerHandler());
            messageInfoHandlers.Add(new ConnectToServerHandler());
            messageInfoHandlers.Add(new DisconnectFromServerHandler());
            messageInfoHandlers.Add(new DownloadFileCompletedHandler());
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
        public void Deal(MessageInfo message)
        {
            if (message.type == MessageType.DownloadFileCompleted)
            {
                Console.WriteLine("Download File Completed");
            }
        }
    }

}
