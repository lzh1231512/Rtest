using DL91.Jobs;
using HtmlAgilityPack;
using Humanizer.Localisation;
using JsonParsingDemo;
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

        public static RootModel GetRelated(int id, int currentPage, int pageSize)
        {
            var url = getRelatedUrl(currentPage, pageSize, id);
            var p = HttpHelper.GetHtml(url);
            if (p.IsGood)
            {
                return JsonParsingDemo.helper.parseJson(p.Html);
            }
            return null;
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
                    DateTime? created = null;
                    try
                    {
                        var html = getDetailHtml(AutoProcessService.domain+ "/api/videos/detail?id=" + item.id);
                        if (html != null)
                        {
                            var json = DetailHelper.parseJson(html);

                            typeName = json?.Content.Categories[0].Title;
                            try
                            {
                                created = DateTime.ParseExact(json?.Content.PostDate, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch(Exception e)
                            {
                                LogTool.Instance.Error("Failed to parse date for "+ json?.Content.PostDate);
                                created = null;
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
                    }
                    catch (Exception ex)
                    {
                        LogTool.Instance.Error("Failed to get detail for " + item.id + " " + item.url + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    item.typeId = typeID;
                    if(created!=null)
                    {
                        item.createDate = (int)(created.Value.ToUniversalTime()
                            .AddHours(-8) - new DateTime(1990, 1, 1)).TotalMinutes;
                    }
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
            return AutoProcessService.domain + "/api/videos/index?page=" + page + "&size=24&sort=post_date&tags=&&tt=" + Guid.NewGuid();
        }
        private static string getRelatedUrl(int page,int size, int id)
        {
            return AutoProcessService.domain + "/api/videos/related?page=" + page + "&size="+ size + "&id="+ id + "&&tt=" + Guid.NewGuid();
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
