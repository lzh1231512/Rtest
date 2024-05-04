using DL91.Jobs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DL91.WebProcess
{
    public class ProcessHtml
    {
        public static bool DownloadNewData()
        {
            var result = false;
            if (DownloadTypes())
            {
                result = DownloadList();
                DownloadDetails();
            }
            return result;
        }

        private static bool DownloadTypes()
        {
            try
            {
                Console.WriteLine("Get Types");
                var p = HttpHelper.GetHtml(AutoProcessService.domain + "/categories/");
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
                    var url = nat.Attributes["href"].Value.Replace(AutoProcessService.domain, "");
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

        private static bool DownloadList()
        {
            var hasNew = false;
            var pageCount = 1500;
            for (int page = 1, existsflag = 0; page <= pageCount && existsflag < 5; page++)
            {
                Console.WriteLine("Load Page " + page);
                var html = getListHtml(page, out bool is404);
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
                    href = href.Replace(AutoProcessService.domain, "");
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

        private static void DownloadDetails()
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
                    var html = getDetailHtml(AutoProcessService.domain + item.url);
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

        private static string getListHtml(int page, out bool is404)
        {
            is404 = false;
            while (true)
            {
                var p = HttpHelper.GetHtml(getListUrl(page));
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

        private static string getListUrl(int page)
        {
            if (page == 1)
            {
                return AutoProcessService.domain + "/latest-updates/?tt=" + Guid.NewGuid();

            }
            return AutoProcessService.domain + "/latest-updates/" + page + "/?tt=" + Guid.NewGuid();
        }

        private static string getDetailHtml(string url)
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

    }
}
