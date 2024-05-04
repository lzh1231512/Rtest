using GetWebInfo;
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
        public const int VideoDownloadTiemLimit = -50;

        public static bool EnableCacheProcess { set; get; } = false;


        public static void Main(string[] args)
        {
            NEVER_EAT_POISON_Disable_CertificateValidation();
            CacheManager.ClearCache();
            while (true)
            {
                try
                {
                    if (SyncFlag <= 0)
                    {
                        if (getList())
                        {
                            CacheManager.ClearCache();
                        }
                        DownloadImg();
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
                        DownloadVideo();
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

        static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };
        }
        static int downloadM3u8(int id,int downloadtime,out long fileSize)
        {
            fileSize = 0;
            var (m3url, html) = CommonFunc.GetFixedM3u8(id);
            Console.WriteLine(m3url);
            if (string.IsNullOrEmpty(html))
            {
                var p = HttpHelper.GetHtml(m3url);
                if(!p.IsGood)
                {
                    return downloadtime - 1;
                }
                html = p.Html;
            }

            var info = html.Split("\n").Select(f => f.Trim(' ', '\r'));
            var urls = info.Where(f => !f.ToLower().StartsWith("#") && !string.IsNullOrEmpty(f.Trim()));
            var dLst = urls.Select(f => new DownloadTask()
            {
                url = f.ToLower().StartsWith("http") ? f : m3url.Replace("index.m3u8", f),
                savepath = getVideoSavePath(id, f)
            });
            var dLst2 = DownloadHelper.DL(dLst.ToList(), 1);
            if (dLst2.Any(f => f.result != 1))
            {
                return downloadtime - 1;
            }
            fileSize = dLst2.Sum(f => f.fileSize);
            using (FileStream fs=new FileStream(getVideoSavePath(id, m3url), FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw=new StreamWriter(fs))
                {
                    foreach (var item in info)
                    {
                        if (item.ToLower().StartsWith("http"))
                        {
                            var name = item.Substring(item.LastIndexOf('/') + 1);
                            sw.Write(name + '\n');
                        }
                        else
                        {
                            sw.Write(item + '\n');
                        }
                    }
                    sw.Close();
                    fs.Close();
                }
            }
            return 1;
        }

        
        static void DownloadVideo()
        {
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.isLike == 0
                    && f.isVideoDownloaded == 1))
                {
                    if (Directory.Exists(getVideoSaveFolder(item.id)))
                    {
                        Console.WriteLine("Delete folder " + getVideoSaveFolder(item.id));
                        Directory.Delete(getVideoSaveFolder(item.id), true);
                    }
                    item.isVideoDownloaded = 0;
                    db.SaveChanges();
                }
            }
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.isLike == 1 && f.id > 0
                    && f.isVideoDownloaded > VideoDownloadTiemLimit))
                {
                    if (item.isVideoDownloaded == 1)
                    {
                        if (File.Exists(getVideoSavePath(item.id, CommonFunc.GetM3u8Url(item.id, 0,false))))
                        {
                            continue;
                        }
                    }
                    try
                    {
                        Console.WriteLine("download video " + item.id);
                        item.isVideoDownloaded = downloadM3u8(item.id, item.isVideoDownloaded, out long fileSize);
                        item.videoFileSize = fileSize;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("download video " + item.id + "Failed:" + e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    db.SaveChanges();
                }
            }
        }
        public static void ResetFailedVideo()
        {
            using (var db = new DB91Context())
            {
                foreach (var obj in db.DB91s.Where(f => f.isLike == 1 && f.isVideoDownloaded < 0))
                {
                    obj.isVideoDownloaded = 0;
                }
                db.SaveChanges();
            }
        }
        static void DownloadImg()
        {
            var downloaded = 0;
            while (true)
            {
                Console.WriteLine("download img " + downloaded);

                using (var db = new DB91Context())
                {
                    var lst = db.DB91s.Where(f => f.IsImgDownload == 0).Take(200);
                    if (!lst.Any())
                        break;

                    foreach (var item in lst)
                    {
                        item.imgUrl = item.imgUrl;
                    }

                    var dLst = lst.Select(f => new DownloadTask()
                    {
                        url = f.imgUrl,
                        savepath = getSavePath(f)
                    });
                    var dLst2 = DownloadHelper.DL(dLst.ToList(), 8);

                    foreach (var item in lst)
                    {
                        item.IsImgDownload = dLst2.Single(f => f.url == item.imgUrl).result;
                    }
                    db.SaveChanges();
                    downloaded += lst.Count();
                }
            }
        }

        static string getSavePath(DB91 task)
        {
            return "wwwroot/imgs/" + (task.id / 1000) + "/" + task.id + ".jpg";
        }

        static string getVideoSavePath(int id,string url)
        {
            var name = url.Substring(url.LastIndexOf('/') + 1);
            return getVideoSaveFolder(id) + "/" + name;
        }
        static string getVideoSaveFolder(int id)
        {
            return "wwwroot/video/" + (id / 1000) + "/" + id;
        }
        static string getUrl(int page)
        {
            if (page == 1)
            {
                return domain + "/latest-updates/?tt=" + Guid.NewGuid();

            }
            return domain + "/latest-updates/" + page + "/?tt=" + Guid.NewGuid();
        }

        static string getHtml(int page, out bool is404)
        {
            is404 = false;
            while (true)
            {
                var p = HttpHelper.GetHtml(getUrl(page));
                if (p.IsGood)
                {
                    return p.Html;
                }
                if (p.Is404)
                {
                    is404 = true;
                    return "";
                }
                Thread.Sleep(60000);
            }
        }

        static string getDetailHtml(string url)
        {
            var i = 0;
            while (i < 3)
            {
                var p = HttpHelper.GetHtml(url);
                if (p.IsGood)
                {
                    return p.Html;
                }
                Thread.Sleep(60000);
                i++;
            }
            return null;
        }

        static bool getList()
        {
            var result = false;
            if (getType())
            {
                result = getSingleList();
                getDetailType();
            }
            return result;
        }
        static bool getSingleList()
        {
            var hasNew = false;
            var pageCount = 1500;
            for (int page = 1, existsflag = 0; page <= pageCount && existsflag < 5; page++)
            {
                Console.WriteLine("Load Page " + page);
                var html = getHtml(page, out bool is404);
                if (is404)
                    break;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode navNode = doc.GetElementbyId("list_videos_latest_videos_list_items");
                HtmlNodeCollection categoryNodeList = navNode.SelectNodes("div");

                var isExists = true;
                var dt91 = new DateTime(1990, 1, 1);
                for (int i = 0; i < categoryNodeList.Count; i++)
                {
                    HtmlNode nat = categoryNodeList[i];
                    var atag = nat.SelectNodes("a")[0];
                    String href = atag.Attributes["href"].Value;
                    href = href.Replace(domain, "");
                    var a = href.IndexOf('/', 2);
                    var b = href.IndexOf('/', a + 1);
                    var id = int.Parse(href.Substring(a + 1, b - a - 1));
                    String title = atag.Attributes["title"].Value;
                    String img = atag.SelectNodes("div")[0].SelectNodes("img")[0].Attributes["data-original"].Value;
                    var time = atag.SelectNodes("div")[1].SelectNodes("div")[0].InnerText.Split(':');
                    var isHD = nat.SelectNodes("a//span[@class='is-hd']") != null;
                    int timeInt = 0;
                    if (time.Length == 3)
                    {
                        timeInt = int.Parse(time[0]) * 60 * 60 + int.Parse(time[1]) * 60 + int.Parse(time[2]);
                    }
                    else if (time.Length == 2)
                    {
                        timeInt = int.Parse(time[0]) * 60 + int.Parse(time[1]);
                    }
                    using (var db = new DB91Context())
                    {
                        if (!db.DB91s.Any(f => f.id == id))
                        {
                            isExists = false;
                            hasNew = true;
                            db.DB91s.Add(new DB91()
                            {
                                id = id,
                                time = timeInt,
                                title = title,
                                imgUrl = img,
                                typeId = 0,
                                url = href,
                                isHD = isHD,
                                createDate = (int)(DateTime.UtcNow - dt91).TotalMinutes
                            });
                            db.SaveChanges();
                        }
                    }
                }

                if (isExists)
                {
                    existsflag++;
                }
            }
            return hasNew;
        }

        static void getDetailType()
        {
            using (var db = new DB91Context())
            {
                var types = db.DBTypes.ToList();
                var count = db.DB91s.Where(f => f.typeId == 0).Count();
                var index = 0;
                foreach (var item in db.DB91s.Where(f => f.typeId == 0))
                {
                    index++;
                    if (index % 10 == 0)
                    {
                        Console.WriteLine("Load Detail " + index + "/" + count);
                    }
                    var typeName = "";
                    var typeID = -1;
                    var html = getDetailHtml(domain + item.url);
                    if (html != null)
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);
                        HtmlNode navNode = doc.GetElementbyId("tab_video_info");
                        foreach (var atag in navNode.SelectNodes("div//a"))
                        {
                            var href = atag.Attributes["href"].Value;
                            if (href.Contains("categories"))
                            {
                                typeName = atag.InnerText.Trim();
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        var type = types.FirstOrDefault(f => f.name == typeName);
                        if (type == null)
                        {
                            var ntype = new DBType()
                            {
                                url = "",
                                name = typeName,
                                count = 0,
                                maxID = 0
                            };
                            db.DBTypes.Add(ntype);
                            db.SaveChanges();
                            types = db.DBTypes.ToList();
                            type = types.FirstOrDefault(f => f.name == typeName);
                        }
                        if (type != null)
                        {
                            typeID = type.id;
                        }
                    }
                    item.typeId = typeID;
                    db.SaveChanges();
                }
               
            }
        }



        static bool getType()
        {
            try
            {
                Console.WriteLine("Get Types");
                

                var p = HttpHelper.GetHtml(domain + "/categories/");
                if (!p.IsGood)
                {
                    return false;
                }
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(p.Html);

                var categoryNodeList = doc.DocumentNode.SelectNodes("//div[@id='list_categories_categories_list_items']/a[@class='item']");
                for (int i = 0; i < categoryNodeList.Count; i++)
                {
                    HtmlNode nat = categoryNodeList[i];
                    var url = nat.Attributes["href"].Value.Replace(domain, "");
                    var count = "0";
                    var name = nat.Attributes["title"].Value;

                    using (var db = new DB91Context())
                    {
                        var type = db.DBTypes.FirstOrDefault(f => f.name == name);
                        if (type == null)
                        {
                            type = new DBType()
                            {
                                url = url,
                                name = name,
                                count = int.Parse(count),
                                maxID = 0
                            };
                            db.DBTypes.Add(type);
                        }
                        else
                        {
                            type.count = int.Parse(count);
                        }
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
    }
}
