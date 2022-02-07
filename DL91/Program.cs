using GetWebInfo;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Threading;

namespace DL91
{
    public class Job
    {
        static string domain = "https://www.91rb.net";
        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    getList();
                    DownloadImg();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(60000 * 60 * 12); ;
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


        static string getUrl(int page)
        {
            if (page == 1)
            {
                return domain + "/latest-updates/";

            }
            return domain + "/latest-updates/" + page + "/";
        }
        static string getHtml(int page)
        {
            while (true)
            {
                WebPage p = new WebPage(getUrl(page));
                if (p.IsGood)
                {
                    return p.Html;
                }
                Thread.Sleep(60000 * 10);
            }
        }

        static void getList()
        {
            var maxID = 4;
            using (var db = new DB91Context())
            {
                maxID = db.getMaxID();
            }
            var newMaxID = 0;
            for (int page = 1;; page++)
            {
                Console.WriteLine("Load Page" + page);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(getHtml(page));
                HtmlNode navNode = doc.GetElementbyId("list_videos_latest_videos_list_items");
                HtmlNodeCollection categoryNodeList = navNode.SelectNodes("div");
                var isBreak = false;
                for (int i = 0; i < categoryNodeList.Count; i++)
                {
                    HtmlNode nat = categoryNodeList[i];
                    var atag = nat.SelectNodes("a")[0];
                    String href = atag.Attributes["href"].Value;
                    href = href.Replace(domain, "");
                    var a = href.IndexOf('/', 2);
                    var b = href.IndexOf('/', a + 1);
                    var id = int.Parse(href.Substring(a + 1, b - a - 1));
                    if (newMaxID == 0)
                    {
                        newMaxID = id;
                    }
                    String title = atag.Attributes["title"].Value;
                    String img = atag.SelectNodes("div")[0].SelectNodes("img")[0].Attributes["data-original"].Value;
                    var time = atag.SelectNodes("div")[1].SelectNodes("div")[0].InnerText.Split(':');

                    int timeInt = int.Parse(time[0]) * 60 + int.Parse(time[1]);

                    using (var db = new DB91Context())
                    {
                        if (!db.DB91s.Any(f => f.id == id))
                        {
                            db.DB91s.Add(new DB91()
                            {
                                id = id,
                                time = timeInt,
                                title = title,
                                imgUrl = img,
                                url = href
                            });
                            db.SaveChanges();
                        }
                    }

                    if (id <= maxID)
                    {
                        isBreak = true;
                    }
                }
                if (isBreak)
                {
                    break;
                }

            }
            if (maxID < newMaxID)
            {
                using (var db = new DB91Context())
                {
                    db.setMaxID(newMaxID);
                }
            }
        }
    }
}
