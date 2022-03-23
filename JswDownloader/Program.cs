using CommandLine;
using JswDownloader;
using System;
using System.Timers;

namespace MyApp // Note: actual namespace depends on the project name.
{


    [Verb("download", HelpText = "Add file contents to the index.")]
    class DownloadOptions
    {
        [Value(0)]
        public string InfoFileTxt
        {
            get;
            set;
        }


    }
    [Verb("Server", isDefault: true, HelpText = "Record changes to the repository.")]
    class DefulatOptions
    {
        [Value(0)]
        public string File
        {
            get;
            set;
        }
    }
    [Verb("clone", HelpText = "Clone a repository into a new directory.")]
    class CloneOptions
    {
        //clone options here
    }

    internal class Program
    {



        static int Main(string[] args)
        {

            DownloadManager downloadManager = new DownloadManager();
            FileServer server = new FileServer(downloadManager);
            List<Downloader> downloaders = new List<Downloader>();
            MessageInfoManager messageInfoManager = new MessageInfoManager(downloadManager, server, downloaders);
            System.Timers.Timer timer = new System.Timers.Timer();

            timer.Interval = 1000;
            timer.Elapsed += (object? sender, ElapsedEventArgs e) =>
            {
                messageInfoManager.RunHandlers();
            };
            timer.Start();



            CommandLine.Parser.Default.ParseArguments<DownloadOptions, DefulatOptions, CloneOptions>(args)
             .MapResult(
               (DownloadOptions opts) =>
               {
                   JswFileInfo fif = downloadManager.ToInstance<JswFileInfo>(File.ReadAllText(opts.InfoFileTxt));
                   downloadManager._originalFileInfo = fif;
                   downloadManager._ownedFileInfo = downloadManager.CreateOwnedFileInfo(fif);
                   downloadManager._dataContent = new byte[fif.fileSize];
                   downloadManager._seeding = false;
                   downloadManager.messages.Enqueue(new MessageInfo() { type = MessageType.FindNewPeer });
                   return 0;
               },
               (DefulatOptions opts) =>
               {
                   downloadManager.CreateFileInfo(opts.File);
                   downloadManager._seeding = true;
                   return 0;
               },
               (CloneOptions opts) => { Console.WriteLine("error!"); return 0; },
               errs => 1);
            server.Start();
            return 0;
        }
    }
}