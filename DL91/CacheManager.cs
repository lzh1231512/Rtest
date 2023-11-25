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

        private static Dictionary<string, SearchViewModel> cachedData = new Dictionary<string, SearchViewModel>();
        private const int cacheSizeLimit = 15;
        private const int cacheTimeLimit = 10;

        //private static Object cacheLocker = new object();
        private const string cachePath = "wwwroot/cache/";

        private static bool isRunding = false;
        public static SearchViewModel GetData(SearchViewModel model,out int isCached)
        {
            Job.EnableCacheProcess = true;
            var pageKey = model.HashCode;
            isCached = 1;
            if (!cachedData.ContainsKey(pageKey))
            {
                if (File.Exists(cachePath + model.HashCode))
                {
                    var result = deSerialize(model);
                    if (result != null)
                    {
                        isCached = 2;
                        RunAddCache(result);
                    }
                }
                if (isCached != 2)
                {
                    Cache(model, true);
                    isCached = 0;
                }
            }
            while (!cachedData.ContainsKey(pageKey))
            {
                Thread.Sleep(100);
            }
            cachedData[pageKey].CachedTime = DateTime.Now;
            return cachedData[pageKey];
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
                catch
                {
                    return null;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
        private static void serialize(SearchViewModel model)
        {
            if(File.Exists(cachePath + model.HashCode))
            {
                return;
            }

            using (FileStream fs = new FileStream(cachePath + model.HashCode, FileMode.Create, FileAccess.ReadWrite))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, model);
                }
                catch (Exception e)
                {
                    Console.WriteLine("serialize Failed:" + e.Message);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public static void Cache(SearchViewModel model, bool isUrgent = false)
        {
            if (model == null)
                return;
            Console.WriteLine("add cache:" + model.HashCode);
            CacheAddTask(model, isUrgent);
            if (!isRunding)
            {
                Task.Factory.StartNew(() =>
                {
                    Run();
                });
            }
        }
        private static void CacheAddTask(SearchViewModel model, bool isUrgent)
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
        
        public static void ClearCache()
        {
            lock (cachedData)
            {
                foreach (var item in Directory.GetFiles(cachePath))
                {
                    try
                    {
                        File.Delete(item);
                    }
                    catch { }
                }
                cachedData.Clear();
            }
        }
        private static void ClearLikeCache()
        {
            lock (cachedData)
            {
                foreach (var item in Directory.GetFiles(cachePath, "1*"))
                {
                    try
                    {
                        File.Delete(item);
                    }
                    catch { }
                }
                cachedData.Where(f => f.Value.isLike == 1).Select(f => f.Key).ToList().ForEach((item) =>
                      {
                          cachedData.Remove(item);
                      });
            }
        }
        public static bool NeedClearLikeCache { set; get; } = false;
        public static void ProcessCache()
        {
            Console.WriteLine("ProcessCache");
            List<SearchViewModel> lst = null;
            lock (cachedData)
            {
                if (cachedData.Count == 0)
                {
                    return;
                }
                lst = cachedData.Values.ToList();
            }
            foreach (var item in lst)
            {
                serialize(item);
            }
            var keeplst = lst.Where(f=>(DateTime.Now-f.CachedTime).TotalMinutes< cacheTimeLimit)
                .OrderByDescending(f => f.CachedTime).Take(cacheSizeLimit).Select(f => f.HashCode).ToList();
            lock (cachedData)
            {
                foreach (var item in lst.Where(f => !keeplst.Any(z => f.HashCode == z)))
                {
                    cachedData.Remove(item.HashCode);
                }
                if (cachedData.Count == 0)
                {
                    Job.EnableCacheProcess = false;
                }
            }
            if (NeedClearLikeCache)
            {
                ClearLikeCache();
                NeedClearLikeCache = false;
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
                    var model = RunPopTaks();
                    if (model == null)
                    {
                        break;
                    }
                    try
                    {
                        Console.WriteLine("begin cache:" + model.HashCode);
                        if (cachedData.ContainsKey(model.HashCode))
                        {
                            continue;
                        }
                        var pageCountModel = model.ClonePage(-1);
                        var recordCount = -1;
                        if (cachedData.ContainsKey(pageCountModel.HashCode))
                        {
                            recordCount = cachedData[pageCountModel.HashCode].Page.RecordCount;
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
                                    CreateDate = f.createDate,
                                    IsHD = f.isHD ? 1 : 0,
                                    IsLike = f.isLike,
                                    FileSize = GetFileSize(f.videoFileSize),
                                    Title = (f.isHD ? "[HD]" : "") + getTimeString(f.time) + getTypeName(f.typeId, typeLst) + "</br>" + f.title
                                }).ToList();
                            model.NextPageIDs = string.Join(',', dbdata.Skip(model.Page.PageSize).Select(f => f.id));
                            if (recordCount == -1)
                            {
                                recordCount = dt3.Count();
                            }
                            model.Page.RecordCount = recordCount;
                            pageCountModel.Page.RecordCount = recordCount;
                        }
                        RunAddCache(model);
                        RunAddCache(pageCountModel);
                        Console.WriteLine("end cache:" + model.HashCode);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch { }
            isRunding = false;
        }
        private static string GetFileSize(long filesize)
        {
            if (filesize <= 0)
            {
                return "";
            }
            else if (filesize >= 1024 * 1024 * 1024) //文件大小大于或等于1024MB
            {
                return string.Format("{0:0.00} GB", (double)filesize / (1024 * 1024 * 1024));
            }
            else if (filesize >= 1024 * 1024) //文件大小大于或等于1024KB
            {
                return string.Format("{0:0.00} MB", (double)filesize / (1024 * 1024));
            }
            else if (filesize >= 1024) //文件大小大于等于1024bytes
            {
                return string.Format("{0:0.00} KB", (double)filesize / 1024);
            }
            else
            {
                return string.Format("{0:0.00} bytes", filesize);
            }
        }
        private static SearchViewModel RunPopTaks()
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
        private static void RunAddCache(SearchViewModel model)
        {
            lock (cachedData)
            {
                if (cachedData.ContainsKey(model.HashCode))
                {
                    return;
                }
                model.CachedTime = DateTime.Now;
                cachedData.Add(model.HashCode, model);
            }
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
