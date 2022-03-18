using Microsoft.VisualStudio.TestTools.UnitTesting;
using JswDownloader;
using MyApp;
using System;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DownloadManager downloadManager = new DownloadManager();
            JswFileInfo info =downloadManager.CreateFileInfo("water.jpg");
            Assert.AreEqual("water.jpg", info.fileName);
        }


        [TestMethod]
        public void TestMethod2()
        {
            DownloadManager downloadManager = new DownloadManager();
            JswFileInfo info = downloadManager.CreateFileInfo("water.jpg");
            string jsn = downloadManager.ToJason<JswFileInfo>(info);
            //Assert.AreEqual("water.jpg", info.fileName);
        }


        [TestMethod]
        public void TestMethod3()
        {
            DownloadManager downloadManager = new DownloadManager();
            JswFileInfo info = downloadManager.CreateFileInfo("water.jpg");
            downloadManager.SaveFileInfo(info);
        }

        [TestMethod]
        public void TestMethod4()
        {
            DownloadManager downloadManager = new DownloadManager();
            JswFileInfo info = downloadManager.ReadFileInfo("TestFileInfo.txt");
            Assert.AreEqual("water.jpg", info.fileName);
        }

        [TestMethod]
        public void TestMethod5()
        {
            DownloadManager downloadManager = new DownloadManager();
            JswFileInfo infoOriginal = downloadManager.CreateFileInfo("water.jpg");
            JswFileInfo infoOwned= downloadManager.CreateOwnedFileInfo(infoOriginal);

            ArraySegment<byte> arraySegment = new ArraySegment<byte>(downloadManager._dataContent);

            bool result = downloadManager.WriteDataBlock(0, arraySegment.Slice(0, (int)(infoOwned.blockEnd[0]- infoOwned.blockStart[0])).ToArray());

            Assert.AreEqual(true,result);
        }


    }
}