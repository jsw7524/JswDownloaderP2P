using Microsoft.VisualStudio.TestTools.UnitTesting;
using JswDownloader;
using MyApp;

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
            JswFileInfo info = downloadManager.WriteDataBlock()
            Assert.AreEqual("water.jpg", info.fileName);
        }


    }
}