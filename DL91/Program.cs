using DL91.WebProcess;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace DL91
{
    public class Job
    {
        public static string domain = "https://www.rm2029.com";
        //static string domain = "https://www.91rb.net";

        public static int SyncFlag { set; get; } = 0;
        public static int DownloadVideoFlag { set; get; } = 0;

        public static bool EnableCacheProcess { set; get; } = false;


        public static void Main(string[] args)
        {
            HttpHelper.NEVER_EAT_POISON_Disable_CertificateValidation();
            CacheManager.ClearCache();
            while (true)
            {
                try
                {
                    if (SyncFlag <= 0)
                    {
                        if (ProcessHtml.DownloadNewData())
                        {
                            CacheManager.ClearCache();
                        }
                        ProcessImg.DownloadImg();
                        SyncFlag = 60 * 6;

                        Thread.Sleep(1000);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    SyncFlag--;
                    if (EnableCacheProcess)
                    {
                        CacheManager.ProcessCache();
                    }
                    if (DownloadVideoFlag <= 0)
                    {
                        ProcessVideo.DownloadVideo();
                        DownloadVideoFlag = 10;
                    }
                    DownloadVideoFlag--;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                Thread.Sleep(60000);
            }
        }
        public static void Test2(string[] args)
        {
            var cpath = "E:\\UnboundedSharing\\";
            var mpath = "E:\\UnboundedSharing\\menu\\";
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.id<0))
                {
                    if (Directory.Exists(cpath + item.id))
                    {
                        Directory.Delete(cpath + item.id, true);
                    }
                    Directory.CreateDirectory(cpath + item.id);

                    var path = "c:\\" + item.url.Substring(item.url.IndexOf("m3u8"));
                    var file = new FileInfo(path);
                    var m3u8 = File.ReadAllText(path);
                    var m3u8info = m3u8.Split('\n').Select(f => f.Replace("\r", ""))
                        .Where(f => !f.StartsWith("#") && !string.IsNullOrEmpty(f));
                    var vIndex = 0;
                    foreach (var ts in m3u8info)
                    {
                        m3u8 = m3u8.Replace(ts, "fileList?filename=" + item.id + "/" + vIndex + "&mime=application/video/MP2T");
                        File.Copy(file.Directory.FullName + "/" + ts, cpath + item.id+"/"+ vIndex);
                        vIndex++;
                    }
                    File.WriteAllText(cpath + item.id + "/m3", m3u8);

                    var obj = new
                    {
                        id = item.id + "",
                        item.createDate,
                        isLike = "0",
                        isHD = "0",
                        fileSize = "",
                        title = "[00:00][Upload]</br>" + item.title,
                        item.url
                    };
                    if (File.Exists(mpath + item.id))
                    {
                        File.Delete(mpath + item.id);
                    }
                    File.WriteAllText(mpath + item.id, JsonConvert.SerializeObject(obj));
                }
            }
        }
        public static void Test(string[] args)
        {
            while (true)
            {
                Thread.Sleep(60000);
                try
                {
                    CacheManager.ProcessCache();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
