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
    }
}