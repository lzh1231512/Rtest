using DL91;
using DL91.Models;
using DL91Web.Helpers;
using DL91Web8.Helpers;
using DL91Web8.Models;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Diagnostics;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DL91Web8.Controllers
{
    public class HomeController : Controller
    {
        private const string cacheVirtualPath = "~/cache/";

        private readonly IWebHostEnvironment _env;
        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
            if (!checkLogin())
            {
                return Redirect("~/Home/Login");
            }
            var model = new SearchViewModel();
            model.Page = new Pager()
            {
                CurrentPage = 1,
                PageSize = new CookiesHelper().GetPageSize(HttpContext)
            };
            return View(model);
        }

        public IActionResult IndexForAjax(SearchViewModel model, int currentPage = 1)
        {
            if (!checkLogin())
            {
                return Redirect("~/Home/Login");
            }

            model.Page = new Pager()
            {
                CurrentPage = currentPage,
                PageSize = new CookiesHelper().GetPageSize(HttpContext)
            };
            var cache = CacheManager.GetData(model, out int isCached);
            model.Data = cache.Data;
            model.NextPageIDs = cache.NextPageIDs;
            model.Page.RecordCount = cache.Page.RecordCount;
            ViewBag.fromCache = isCached;
            CacheManager.Cache(model.NextPage);
            CacheManager.Cache(model.LastPage);
            CacheManager.Cache(model.PrevPage);

            return Json(model);
        }

        public IActionResult like(int id, int isLike)
        {
            using (var db = new DB91Context())
            {
                var obj = db.DB91s.Where(f => f.id == id).FirstOrDefault();
                obj.isLike = isLike;
                if (obj.isVideoDownloaded == 2)
                    obj.isVideoDownloaded = 0;
                db.SaveChanges();
            }
            DL91.Job.DownloadVideoFlag = 0;
            CacheManager.NeedClearLikeCache = true;
            return Json(1);
        }

        public IActionResult sync()
        {
            DL91.Job.SyncFlag = 0;
            return Json(1);
        }
        public IActionResult ResetFailedVideo()
        {
            DL91.Job.ResetFailedVideo();
            return Json(1);
        }

        private static string loginkey { set; get; }
        private bool checkLogin()
        {
            if (loginkey == null)
            {
                var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config/appsettings.json").Build();
                loginkey = configuration["App:key"];
            }
            return Request.Cookies["key"] == loginkey;
        }


        public IActionResult GetImg(string imgs)
        {
            if (string.IsNullOrEmpty(imgs))
            {
                return Content("NoPIC");
            }
            var fileName = "img" + SearchViewModel.MD5Encrypt16(imgs);
            if (System.IO.File.Exists(Common.cachePath + fileName))
            {
                return File(cacheVirtualPath + fileName, "image/Jpeg");
            }
            Common.MergeImgs(imgs, fileName);
            return File(cacheVirtualPath + fileName, "image/Jpeg");
        }

        public IActionResult Add(string url, List<IFormFile> files)
        {
            var message = "";
            if (!string.IsNullOrEmpty(url))
            {
                if (files == null || files.Count == 0)
                {
                    message = "need cover";
                }
                else if (files.Any(f => !f.FileName.ToLower().EndsWith(".jpg")))
                {
                    message = "cover must be .jpg";
                }
                else
                {
                    var id = -1;
                    using (var db = new DB91Context())
                    {
                        var theType = db.DBTypes.Where(f => f.id == -2).Select(f => new DBType()
                        {
                            id = f.id,
                            name = f.name
                        }).FirstOrDefault();
                        if (theType == null)
                        {
                            db.DBTypes.Add(new DBType()
                            {
                                id = -2,
                                name = "Upload"
                            });
                            db.SaveChanges();
                        }
                        var min = db.DB91s.OrderBy(f => f.id).FirstOrDefault()?.id;
                        if (min.HasValue && min <= id)
                        {
                            id = min.Value - 1;
                        }
                    }
                    string path = _env.ContentRootPath + "/wwwroot/";
                    var folder = path.TrimEnd('/', '\\') + "/imgs/-1/";
                    if (!System.IO.Directory.Exists(folder))
                    {
                        System.IO.Directory.CreateDirectory(folder);
                    }
                    MagickReadSettings settings = new MagickReadSettings();
                    settings.Width = 320;
                    settings.Height = 180;
                    foreach (var file in files)
                    {
                        var fileName = file.FileName.Replace(".jpg", "");
                        string fullPath = path.TrimEnd('/', '\\') + "/imgs/-1/" + id + ".jpg";
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                            stream.Close();
                        }

                        MagickImage canvas = new MagickImage("xc:white", settings);
                        canvas.Format = MagickFormat.Jpeg;

                        var first = new MagickImage(fullPath);
                        var h = (int)(320 * first.Height / first.Width);
                        if (h < 180)
                        {
                            first.Resize(320, h);
                            canvas.Composite(first, 0, (180 - h) / 2);
                        }
                        else
                        {
                            var w = (int)(180 * first.Width / first.Height);
                            first.Resize(w, 180);
                            canvas.Composite(first, (320 - w) / 2, 0);
                        }
                        first.Dispose();
                        canvas.Write(fullPath);

                        var dt91 = new DateTime(1990, 1, 1);
                        using (var db = new DB91Context())
                        {
                            db.DB91s.Add(new DB91()
                            {
                                id = id,
                                title = fileName,
                                url = url.TrimEnd('/') + "/" + fileName + ".m3u8",
                                typeId = -2,
                                IsImgDownload = 1,
                                createDate = (int)(DateTime.UtcNow - dt91).TotalMinutes
                            });
                            db.SaveChanges();
                        }
                        id -= 1;
                    }

                    CacheManager.ClearCache();
                    message = "add successful:" + id;
                }

            }
            ViewBag.url = url;
            ViewBag.msg = message;
            return View();
        }

        public IActionResult Add2()
        {
            return View();
        }

        public IActionResult Edit(int id, string title, IFormFile files)
        {
            DB91 obj = null;
            string message = "";
            if (!string.IsNullOrEmpty(title))
            {
                if (files != null && !files.FileName.ToLower().EndsWith(".jpg"))
                {
                    message = "cover must be .jpg";
                }
                else
                {
                    using (var db = new DB91Context())
                    {
                        obj = db.DB91s.FirstOrDefault(f => f.id == id);
                        if (obj == null)
                        {
                            message = "not fount";
                        }
                        obj.title = title;
                        db.SaveChanges();
                    }
                    if (files != null)
                    {
                        string path = _env.ContentRootPath + "/wwwroot/";
                        var folder = path.TrimEnd('/', '\\') + "/imgs/-1/";
                        if (!System.IO.Directory.Exists(folder))
                        {
                            System.IO.Directory.CreateDirectory(folder);
                        }
                        MagickReadSettings settings = new MagickReadSettings();
                        settings.Width = 320;
                        settings.Height = 180;

                        string fullPath = path.TrimEnd('/', '\\') + "/imgs/" + (id < 0 ? -1 : (((id / 1000) * 1000))) + "/" + id + ".jpg";
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            files.CopyTo(stream);
                            stream.Close();
                        }
                        MagickImage canvas = new MagickImage("xc:white", settings);
                        canvas.Format = MagickFormat.Jpeg;
                        var first = new MagickImage(fullPath);
                        var h = (int)(320 * first.Height / first.Width);
                        if (h < 180)
                        {
                            first.Resize(320, h);
                            canvas.Composite(first, 0, (180 - h) / 2);
                        }
                        else
                        {
                            var w = (int)(180 * first.Width / first.Height);
                            first.Resize(w, 180);
                            canvas.Composite(first, (320 - w) / 2, 0);
                        }
                        first.Dispose();
                        canvas.Write(fullPath);
                    }
                    message = "update successful";
                }
                CacheManager.ClearCache();
            }
            else
            {
                using (var db = new DB91Context())
                {
                    obj = db.DB91s.FirstOrDefault(f => f.id == id);
                    if (obj == null)
                    {
                        message = "not fount";
                    }
                }
            }

            ViewBag.msg = message;

            return View(obj);
        }


        public IActionResult Delete(int id)
        {
            using (var db = new DB91Context())
            {
                string path = _env.ContentRootPath + "/wwwroot/";
                var img = path.TrimEnd('/', '\\') + "/imgs/" + (id < 0 ? "-1" : (id / 1000).ToString()) + "/" + id + ".jpg";
                if (System.IO.File.Exists(img))
                    System.IO.File.Delete(img);

                var obj = db.DB91s.FirstOrDefault(f => f.id == id);
                if (obj != null)
                {
                    db.DB91s.Remove(obj);
                }
                db.SaveChanges();
            }
            CacheManager.ClearCache();
            return Json(1);
        }

        public IActionResult m3u8fix(int id, int isHD)
        {
            if (isHD == -1)
            {
                var (url, cont) = Common.GetFixedM3u8(id);
                return Content(cont, "application/x-mpegURL");
            }

            var result = Common.M3u8fix(id, isHD);
            return Content(result, "application/x-mpegURL");
        }


        public IActionResult GetByIDs(string ids)
        {
            if (!checkLogin())
            {
                return Redirect("~/Home/Login");
            }
            var typeLst = new List<DBType>();
            var idList = ids.Split(',').Select(f => int.Parse(f)).ToArray();
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
                var lst = db.DB91s.AsQueryable().Where(f => idList.Contains(f.id));
                return Json(lst
                    .Select(f => new DataViewModel()
                    {
                        Id = f.id,
                        CreateDate = f.createDate,
                        IsHD = f.isHD ? 1 : 0,
                        IsLike = f.isLike,
                        FileSize = Common.GetFileSize(f.videoFileSize),
                        Url = f.url,
                        Title = (f.isHD ? "[HD]" : "") + Common.GetTimeString(f.time) + Common.GetTypeName(f.typeId, typeLst) + "</br>" + f.title
                    }).ToList()
                    );
            }
        }

        public IActionResult GetTypes()
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
                return Json(typeLst.ToList());
            }
        }

        private static string _swcache = "";
        public IActionResult SwCache()
        {
            if (string.IsNullOrEmpty(_swcache))
            {
                string path = _env.ContentRootPath+ "/wwwroot/";
                path = path.TrimEnd('/', '\\') + "/_sw-cache.js";
                _swcache = System.IO.File.ReadAllText(path);
                _swcache = _swcache.Replace("{version}", VersionHelper.CompileTime.ToString());
            }

            return Content(_swcache, "application/javascript");
        }
    }
}
