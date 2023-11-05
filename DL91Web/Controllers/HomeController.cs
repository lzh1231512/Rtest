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

namespace DL91Web.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index(SearchViewModel model, int currentPage = 1, bool isAjax = false)
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

                var cache = CacheManager.GetCache(model);
                if (cache != null)
                {
                    model.Data = cache.Data;
                    model.NextPageIDs = cache.NextPageIDs;
                    model.Page.RecordCount = cache.Page.RecordCount;
                }
                else
                {
                    IndexSearchFromDB(model, currentPage, db, TypeLst);
                    CacheManager.Cache(model);
                }
            }
            if (model.GetNextPage() != null)
            {
                CacheManager.Cache(model.GetNextPage());
            }
            if (isAjax)
            {
                return PartialView("_List", model);
            }
            else
            {
                return View(model);
            }
        }

        private void IndexSearchFromDB(SearchViewModel model, int currentPage, DB91Context db, List<DBType> TypeLst)
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
            model.Data = dt3.Skip((currentPage - 1) * model.Page.PageSize).Take(model.Page.PageSize)
                .Select(f => new DataViewModel()
                {
                    Id = f.id,
                    Title = getCreateDateStr(f.createDate) + (f.isHD ? "[HD]" : "") + getTimeString(f.time) + getTypeName(f.typeId, TypeLst) + "</br>" + f.title
                }).ToList();
            model.NextPageIDs = string.Join(',', dt3.Skip((currentPage) * model.Page.PageSize).Take(model.Page.PageSize).Select(f => f.id));
            model.Page.RecordCount = dt3.Count();
        }

        public IActionResult IndexForAjax(SearchViewModel model, int currentPage = 1)
        {
            return Index(model, currentPage, true);
        }

        public IActionResult play(int id,string url)
        {
            if (!checkLogin())
            {
                return Redirect("~/Home/Login");
            }
            ViewBag.url = getM3u8(id,false,false);
            
            using (var db = new DB91Context())
            {
                var TypeLst = db.DBTypes.Select(f => new DBType()
                {
                    id = f.id,
                    name = f.name
                }).ToList();
                var obj = db.DB91s.Where(f => f.id == id).FirstOrDefault();
                if (obj != null)
                {
                    ViewBag.title = (obj.isHD ? "[HD]" : "") + getTypeName(obj.typeId, TypeLst) + obj?.title;
                    ViewBag.isLike = obj?.isLike;
                    if (obj.isHD)
                    {
                        ViewBag.hdurl = getM3u8(id, true, false);
                    }
                    if (obj.isVideoDownloaded == 1)
                    {
                        ViewBag.fileSize = GetFileSize(obj.videoFileSize);
                        ViewBag.localurl = url + getM3u8(id, true, true);
                    }
                }
                else
                {
                    ViewBag.hdurl = getM3u8(id, true, false);
                }
            }
            ViewBag.id = id;
            return View();
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
            if (isLike == 1)
            {
                DL91.Job.DownloadVideoFlag = 0;
            }
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


        public IActionResult GetImg(string imgs,bool notOut=false)
        {
            if (string.IsNullOrEmpty(imgs))
            {
                return null;
            }
            var allImg = imgs.Split(',');
            MagickReadSettings settings = new MagickReadSettings();
            settings.Width = 320;
            settings.Height = 180 * allImg.Count();
            MagickImage canvas = new MagickImage("xc:yellow", settings);
            canvas.Format = MagickFormat.Jpeg;
            var index = 0;
            var nopic = new MagickImage("wwwroot/images/NOPIC.jpg");
            foreach (var item in allImg.Select(f => int.TryParse(f, out int res) ? res : 0))
            {
                var imgpath1 = new FileInfo("wwwroot/imgs/" + (item / 1000) + "/" + item + ".jpg");
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
            if (notOut)
            {
                canvas.ToByteArray(MagickFormat.Jpg);
                return Content("test");
            }
            else
            {
                return File(canvas.ToByteArray(MagickFormat.Jpg), "image/Jpeg");
            }
        }

    }
}
