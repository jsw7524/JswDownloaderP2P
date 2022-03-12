using CommandLine;
using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    [Verb("download", HelpText = "Add file contents to the index.")]
    class DownloadOptions
    {
        [Value(0)]
        public string File
        {
            get;
            set;
        }
    }
    [Verb("Server", isDefault: true, HelpText = "Record changes to the repository.")]
    class DefulatOptions
    {
        //commit options here
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

            CommandLine.Parser.Default.ParseArguments<DownloadOptions, DefulatOptions, CloneOptions>(args)
             .MapResult(
               (DownloadOptions opts) =>
               {

                   Downloader downloader = new Downloader();
                   downloader.DownloadFileAsync("172.23.176.1", 54321, opts.File);

                   return 0;
               },
               (DefulatOptions opts) =>
               {

                   FileServer server = new FileServer();
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