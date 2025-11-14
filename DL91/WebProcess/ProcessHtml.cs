using DL91.Jobs;
using HtmlAgilityPack;
using Humanizer.Localisation;
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
            result = DownloadList();
            DownloadDetails();
            return result;
        }

        private static bool DownloadTypes()
        {
            try
            {
                LogTool.Instance.Info("Get Types");
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

        public static bool DownloadList(bool isTest = false)
        {
            var hasNew = false;
            var pageCount = 1500;
            for (int page = 1, existsflag = 0; page <= pageCount && existsflag < 5; page++)
            {
                LogTool.Instance.Info("Load Page " + page);
                var json = getListHtml(page, out bool is404);
                if (is404)
                    break;
                var isExists = true;
                var dt91 = new DateTime(1990, 1, 1);
                var data = JsonParsingDemo.helper.parseJson(json);
                foreach (var item in data.Content.Data)
                {
                    using (var db = new DB91Context())
                    {
                        if (!db.DB91s.Any(f => f.id == item.VideoId))
                        {
                            isExists = false;
                            hasNew = true;
                            db.DB91s.Add(new DB91()
                            {
                                id = item.VideoId,
                                time = item.Duration,
                                title = item.Title,
                                imgUrl = "",
                                typeId = 0,
                                url = "",
                                isHD = item.IsHd > 0,
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
                if (isTest)
                    break;
            }
            return hasNew;
        }

        public static void DownloadDetails()
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
                        LogTool.Instance.Info("Load Detail " + index + "/" + count);
                    }
                    var typeName = "";
                    var typeID = -1;
                    try
                    {
                        var html = getDetailHtml(AutoProcessService.domain+ "/api/videos/detail?id=" + item.id);
                        if (html != null)
                        {
                            var json = DetailHelper.parseJson(html);

                            typeName = json?.Content.Categories[0].Title;
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
                    }
                    catch (Exception ex)
                    {
                        LogTool.Instance.Error("Failed to get detail for " + item.id + " " + item.url + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
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
            return AutoProcessService.domain + "/api/videos/index?page=" + page + "&size=24&sort=last_time_view_date&tags=&&tt=" + Guid.NewGuid();
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
