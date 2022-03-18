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
        public string IpAddress
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
            MessageInfoManager messageInfoManager = new MessageInfoManager(downloadManager);
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
                   Downloader downloader = new Downloader(downloadManager);
                   downloader.DownloadFileAsync(opts.IpAddress, 54321);

                   return 0;
               },
               (DefulatOptions opts) =>
               {
                   //Console.WriteLine($"Server running for {opts.File}");
                   FileServer server = new FileServer(downloadManager,opts.File);
                   server.Start();
                   return 0;
               },
               (CloneOptions opts) => { Console.WriteLine("C"); return 0; },
               errs => 1);
            while (true) ;
            return 0;
        }
    }
}