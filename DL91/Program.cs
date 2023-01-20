using GetWebInfo;
using HtmlAgilityPack;
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

        public static void Main(string[] args)
        {
            NEVER_EAT_POISON_Disable_CertificateValidation();
            while (true)
            {
                try
                {
                    if (SyncFlag <= 0)
                    {
                        getList();
                        DownloadImg();
                        SyncFlag = 60 * 6;
                    }
                    SyncFlag--;

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
        static bool downloadM3u8(int id,out long fileSize)
        {
            fileSize = 0;
            WebPage p = new WebPage(getM3u8Url(id));
            if (!p.IsGood)
            {
                return false;
            }
            var info = p.Html.Split("\n");
            var urls = info.Where(f => f.ToLower().StartsWith("http"));
            var dLst = urls.Select(f => new DLTask()
            {
                url = f,
                savepath = getVideoSavePath(id, f)
            });
            var dLst2 = DLHelper.DL(dLst.ToList(), 8);
            fileSize = dLst2.Sum(f => f.fileSize);
            using (FileStream fs=new FileStream(getVideoSavePath(id, p.URL), FileMode.Create, FileAccess.ReadWrite))
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
            return true;
        }

        static string getM3u8Url(int id)
        {
            var domain = "https://cdn.163cdn.net";
            //var domain = id < 72125 ? "https://cust91rb.163cdn.net" : "https://cust91rb2.163cdn.net";
            var result = domain + "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_720p.mp4/index.m3u8";
            if (testHttp(result))
                return result;
            return domain + "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8";
        }
        static bool testHttp(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "*/*";
                request.CookieContainer = new CookieContainer();
                HttpWebResponse respons = (HttpWebResponse)request.GetResponse();
                return respons.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }
        static void DownloadVideo()
        {
            while (true)
            {
                using (var db = new DB91Context())
                {
                    var obj = db.DB91s.Where(f => f.isLike == 1 && (f.isVideoDownloaded == 0)).FirstOrDefault();
                    if (obj == null)
                        break;
                    Console.WriteLine("download video " + obj.id);
                    obj.isVideoDownloaded = downloadM3u8(obj.id, out long fileSize) ? 1 : 2;
                    obj.videoFileSize = fileSize;
                    db.SaveChanges();
                }
            }
        }
        public static void ResetFailedVideo()
        {
            using (var db = new DB91Context())
            {
                foreach (var obj in db.DB91s.Where(f => f.isLike == 1 && f.isVideoDownloaded == 2))
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

                    var dLst = lst.Select(f => new DLTask()
                    {
                        url = f.imgUrl,
                        savepath = getSavePath(f)
                    });
                    var dLst2 = DLHelper.DL(dLst.ToList(), 8);

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
            return "wwwroot/video/" + (id / 1000) + "/" + id + "/" + name;
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
                WebPage p = new WebPage(getUrl( page));
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
                WebPage p = new WebPage(url);
                if (p.IsGood)
                {
                    return p.Html;
                }
                Thread.Sleep(60000);
                i++;
            }
            return null;
        }

        static void getList()
        {
            if (getType())
            {
                getSingleList();
                getDetailType();
            }
        }
        static void getSingleList()
        {
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
                                var typeName = atag.InnerText.Trim();
                                var type = types.FirstOrDefault(f => f.name == typeName);
                                if (type != null)
                                {
                                    item.typeId = type.id;
                                    db.SaveChanges();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        item.typeId = -1;
                    }
                }
               
            }
        }

        

        static bool getType()
        {
            Console.WriteLine("Get Types");

            WebPage p = new WebPage(domain + "/categories/");
            if (!p.IsGood)
                return false;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(p.Html);

            var categoryNodeList = doc.DocumentNode.SelectNodes("//div[@id='list_categories_categories_list_items']/a[@class='item']");
            for (int i = 0; i < categoryNodeList.Count; i++)
            {
                HtmlNode nat = categoryNodeList[i];
                var url= nat.Attributes["href"].Value.Replace(domain, "");
                var count = "0";
                var name = nat.Attributes["title"].Value;

                using (var db = new DB91Context())
                {
                    var type = db.DBTypes.SingleOrDefault(f => f.url == url);
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
        
    }
}
