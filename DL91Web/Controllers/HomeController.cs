using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DL91.Models;
using DL91Web.Helpers;
using DL91;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using System.Drawing.Imaging;
using ImageMagick;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DL91Web.Controllers
{
    public class HomeController : Controller
    {

        private const string cachePath = "wwwroot/cache/";
        private const string cacheVirtualPath = "~/cache/";

        public HomeController()
        {
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
            using (var db = new DB91Context())
            {
                var TypeLst = db.DBTypes.Select(f => new DBType()
                {
                    id = f.id,
                    name = f.name
                }).ToList();

                TypeLst.Insert(0, new DBType()
                {
                    id = 0,
                    name = "ALL"
                });
                ViewBag.TypeLst = TypeLst;
            }
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

            using (var db = new DB91Context())
            {
                var cache = CacheManager.GetData(model, out int isCached);
                model.Data = cache.Data;
                model.NextPageIDs = cache.NextPageIDs;
                model.Page.RecordCount = cache.Page.RecordCount;
                ViewBag.fromCache = isCached;
            }
            CacheManager.Cache(model.NextPage);
            CacheManager.Cache(model.LastPage);
            CacheManager.Cache(model.PrevPage);

            return Json(model);
        }

        private static string GetFileSize(long filesize)
        {
            if (filesize < 0)
            {
                return "0";
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
        private string getM3u8(int id,bool isHD,bool isLocal)
        {
            //var domain = "https://cdn.163cdn.net";
            if (isLocal)
            {
                return "/video/" + (id / 1000) + "/" + id + "/index.m3u8";
            }
            if (isHD)
            {
                return "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_720p.mp4/index.m3u8";
            }
            return "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8";
        }

        private string getTimeString(int time)
        {
            return string.Format("[{0:D2}:{1:D2}]", time / 60, time % 60);
        }

        private string getCreateDateStr(int time)
        {
            var dt = new DateTime(1990, 1, 1).AddMinutes(time);
            var result = DateTime.UtcNow - dt;

            if (result.TotalDays > 365)
                return "[1年前]";
            if (result.TotalDays > 7)
                return "["+ (int)(result.TotalDays / 7) + "周前]";
            if (result.TotalHours > 24)
                return "[" + (int)(result.TotalDays) + "天前]";
            return "[" + (int)(result.TotalHours) + "小时前]";
        }


        private string getTypeName(int typeId,List<DBType> lst)
        {
            var result = lst.SingleOrDefault(f => f.id == typeId);
            if (result == null)
                return "";
            return "[" + result.name + "]";
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
            if (System.IO.File.Exists(cachePath+ fileName))
            {
                return  File(cacheVirtualPath + fileName, "image/Jpeg");
            }
            var allImg = imgs.Split(',');
            MagickReadSettings settings = new MagickReadSettings();
            settings.Width = 320;
            settings.Height = 180 * allImg.Count();
            MagickImage canvas = new MagickImage("xc:white", settings);
            canvas.Format = MagickFormat.Jpeg;
            var index = 0;
            var nopic = new MagickImage("wwwroot/images/NOPIC.jpg");
            foreach (var item in allImg.Select(f => int.TryParse(f, out int res) ? res : 0))
            {
                var imgpath1 = new FileInfo("wwwroot/imgs/" + (item < 0 ? "-1" : (item / 1000).ToString()) + "/" + item + ".jpg");
                if (imgpath1.Exists)
                {
                    var first = new MagickImage(imgpath1.FullName);
                    canvas.Composite(first, 0, index++ * 180);
                }
                else
                {
                    canvas.Composite(nopic, 0, index++ * 180);
                }
            }
            canvas.Resize(256, 144 * allImg.Count());
            canvas.Write(cachePath + fileName);
            return File(cacheVirtualPath + fileName, "image/Jpeg");
        }

        public IActionResult Add(string url,List<IFormFile> files)
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
                    string path = MyServiceProvider.ServiceProvider.GetRequiredService<IHostingEnvironment>().WebRootPath;
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
                        var fileName = file.FileName.Replace(".jpg","");
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

        public IActionResult Delete(int id)
        {
            using (var db = new DB91Context())
            {
                string path = MyServiceProvider.ServiceProvider.GetRequiredService<IHostingEnvironment>().WebRootPath;
                var img = path.TrimEnd('/', '\\') + "/imgs/"+ (id < 0 ? "-1" : (id / 1000).ToString()) + "/"+ id+".jpg";
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

    }
}
