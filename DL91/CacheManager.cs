using DL91.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DL91
{
    public class CacheManager
    {
        private static List<SearchViewModel> cacheTask = new List<SearchViewModel>();
        //private static Dictionary<string, SearchViewModel> cachedData = new Dictionary<string, SearchViewModel>();

        private static Object cacheLocker = new object();
        private const string cachePath = "wwwroot/cache/";

        private static bool isRunding = false;
        public static SearchViewModel GetData(SearchViewModel model,out bool isCached)
        {
            var pageKey = model.HashCode;
            isCached = true;
            if (!File.Exists(cachePath + pageKey))
            {
                Cache(model,true);
                isCached = false;
            }
            while (!File.Exists(cachePath + pageKey))
            {
                Thread.Sleep(100);
            }
            return deSerialize(model);
        }
        private static SearchViewModel deSerialize(SearchViewModel model)
        {
            using (FileStream fs = new FileStream(cachePath + model.HashCode, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    return (SearchViewModel)bf.Deserialize(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }
        private static void serialize(SearchViewModel model)
        {
            using (FileStream fs = new FileStream(cachePath + model.HashCode, FileMode.Create, FileAccess.ReadWrite))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs,model);
                }
                catch(Exception e)
                {
                    Console.WriteLine("serialize Failed:" + e.Message);
                }
                finally
                {
                    fs.Close();
                }
            }
        }


        private static void AddTask(SearchViewModel model,bool isUrgent)
        {
            lock (cacheTask)
            {
                if (!cacheTask.Any(f => f.HashCode == model.HashCode))
                {
                    if (isUrgent)
                    {
                        cacheTask.Insert(0, model);
                    }
                    else
                    {
                        cacheTask.Add(model);
                    }
                }
            }
        }

        private static SearchViewModel PopTaks()
        {
            lock (cacheTask)
            {
                var first = cacheTask.FirstOrDefault();
                if (first != null)
                {
                    cacheTask.Remove(first);
                }
                return first;
            }
        }

        public static void Cache(SearchViewModel model,bool isUrgent=false)
        {
            if (model == null)
                return;
            Console.WriteLine("add cache:"+ model.HashCode);
            AddTask(model, isUrgent);
            if (!isRunding)
            {
                Task.Factory.StartNew(() =>
                {
                    Run();
                });
            }
        }
        public static void ClearCache()
        {
            lock (cacheLocker)
            {
                foreach (var item in Directory.GetFiles(cachePath))
                {
                    File.Delete(item);
                } 
                Cache(new SearchViewModel() { isLike = 2, Page = new Pager() { CurrentPage = 1, PageSize = 24 } });
            }
        }

        private static void Run()
        {
            if (isRunding)
                return;
            isRunding = true;

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

                while (true)
                {
                    var model = PopTaks();
                    if (model == null)
                    {
                        break;
                    }
                    try
                    {
                        lock (cacheLocker)
                        {
                            Console.WriteLine("begin cache:" + model.HashCode);
                            if (File.Exists(cachePath + model.HashCode))
                            {
                                continue;
                            }
                            var pageCountModel = model.ClonePage(-1);
                            var recordCount = -1;
                            if (File.Exists(cachePath + pageCountModel.HashCode))
                            {
                                recordCount = deSerialize(pageCountModel).Page.RecordCount;
                            }
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
                                var dbdata = dt3.Skip((model.Page.CurrentPage - 1) * model.Page.PageSize).Take(model.Page.PageSize * 2).ToList();

                                model.Data = dbdata.Take(model.Page.PageSize)
                                    .Select(f => new DataViewModel()
                                    {
                                        Id = f.id,
                                        Title = getCreateDateStr(f.createDate) + (f.isHD ? "[HD]" : "") + getTimeString(f.time) + getTypeName(f.typeId, typeLst) + "</br>" + f.title
                                    }).ToList();
                                model.NextPageIDs = string.Join(',', dbdata.Skip(model.Page.PageSize).Select(f => f.id));
                                if (recordCount == -1)
                                {
                                    recordCount = dt3.Count();
                                }
                                model.Page.RecordCount = recordCount;
                                pageCountModel.Page.RecordCount = recordCount;
                            }
                            serialize(model);
                            if (!File.Exists(cachePath + pageCountModel.HashCode))
                            {
                                serialize(pageCountModel);
                            }
                            Console.WriteLine("end cache:" + model.HashCode);
                        }
                    }
                    catch(Exception e)
                    {
                    }
                }
            }
            catch { }
            isRunding = false;
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
