using DL91.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace DL91
{
    public class CacheManager
    {
        private static List<SearchViewModel> cacheTask = new List<SearchViewModel>();
        private static Dictionary<string, SearchViewModel> cachedData = new Dictionary<string, SearchViewModel>();
        private static bool isRunding = false;
        public static SearchViewModel GetCache(SearchViewModel model)
        {
            var pageKey = model.HashCode + "_" + model.Page.HashCode;
            if (cachedData.ContainsKey(pageKey))
                return cachedData[pageKey];
            return null;
        }
        public static void Cache(SearchViewModel model)
        {
            Console.WriteLine("add cache:"+ model.HashCode + "_" + model.Page.HashCode);
            lock (cacheTask)
            {
                if (!cacheTask.Any(f => f.HashCode == model.HashCode))
                {
                    cacheTask.Add(model);
                }
            }
            if (!isRunding)
            {
                Task.Factory.StartNew(() =>
                {
                    Run(false);
                });
            }
        }
        public static void ClearCache()
        {
            lock (cachedData)
            {
                cachedData.Clear();
            }
        }

        private static void Run(bool isNext)
        {
            if (isRunding && !isNext)
                return;
            isRunding = true;
            var tlst = new List<SearchViewModel>();
            lock (cacheTask)
            {
                if (cacheTask.Count == 0)
                {
                    isRunding = false;
                    return;
                }
                tlst.AddRange(cacheTask);
                cacheTask.Clear();
            }
            
            try
            {
                var typeLst = new List<DBType>();
                using (var db = new DB91Context())
                {
                    typeLst = db.DBTypes.Select(f => new DBType()
                    {
                        id = f.id,
                        name = f.name
                    }).ToList();
                    typeLst.Insert(0, new DBType()
                    {
                        id = 0,
                        name = "ALL"
                    });
                }
                lock (cachedData)
                {
                    foreach (var model in tlst)
                    {
                        var pageKey = model.HashCode + "_" + model.Page.HashCode;
                        Console.WriteLine("begin cache:" + model.HashCode + "_" + model.Page.HashCode);
                        if (cachedData.ContainsKey(pageKey))
                        {
                            continue;
                        }
                        if (model.Data == null)
                        {
                            using (var db = new DB91Context())
                            {
                                var lst = db.DB91s.AsQueryable();
                                if (!string.IsNullOrEmpty(model.title1))
                                {
                                    var c1 = (model.title1 ?? "").Split(' ');
                                    lst = lst.Where(f => c1.All(z => f.title.Contains(z)));
                                }
                                if (!string.IsNullOrEmpty(model.title2))
                                {
                                    var c2 = (model.title2 ?? "").Split(' ');
                                    lst = lst.Where(f => c2.All(z => !f.title.Contains(z)));
                                }
                                if (model.isLike != 2)
                                {
                                    lst = lst.Where(f => f.isLike == model.isLike);
                                }
                                if (model.typeId != 0)
                                {
                                    lst = lst.Where(f => f.typeId == model.typeId);
                                }
                                var dt3 = lst.OrderByDescending(f => f.createDate);
                                model.Data = dt3.Skip((model.Page.CurrentPage - 1) * model.Page.PageSize).Take(model.Page.PageSize)
                                    .Select(f => new DataViewModel()
                                    {
                                        Id = f.id,
                                        Title = getCreateDateStr(f.createDate) + (f.isHD ? "[HD]" : "") + getTimeString(f.time) + getTypeName(f.typeId, typeLst) + "</br>" + f.title
                                    }).ToList();
                                model.NextPageIDs = string.Join(',', dt3.Skip((model.Page.CurrentPage) * model.Page.PageSize).Take(model.Page.PageSize).Select(f => f.id));
                                model.Page.RecordCount = dt3.Count();
                            }
                        }
                        cachedData.Add(pageKey, model);
                        Console.WriteLine("end cache:" + model.HashCode + "_" + model.Page.HashCode);
                    }
                }
            }
            catch { }
            Run(true);
        }
        private static string getTimeString(int time)
        {
            return string.Format("[{0:D2}:{1:D2}]", time / 60, time % 60);
        }

        private static string getCreateDateStr(int time)
        {
            var dt = new DateTime(1990, 1, 1).AddMinutes(time);
            var result = DateTime.UtcNow - dt;

            if (result.TotalDays > 365)
                return "[1年前]";
            if (result.TotalDays > 7)
                return "[" + (int)(result.TotalDays / 7) + "周前]";
            if (result.TotalHours > 24)
                return "[" + (int)(result.TotalDays) + "天前]";
            return "[" + (int)(result.TotalHours) + "小时前]";
        }


        private static string getTypeName(int typeId, List<DBType> lst)
        {
            var result = lst.SingleOrDefault(f => f.id == typeId);
            if (result == null)
                return "";
            return "[" + result.name + "]";
        }

        
    }
}
