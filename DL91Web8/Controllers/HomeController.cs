using DL91;
using DL91.Models;
using DL91Web.Helpers;
using DL91Web8.BLL;
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
        private WebBLL bll;
        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
            bll = new WebBLL(ContentPath);
        }

        public string ContentPath => _env.WebRootPath;

        public bool IsLogined => Request.Cookies["key"] == ConfigurationHelper.LoginKey;

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
            if (!IsLogined)
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
            if (!IsLogined)
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
            using (var db = new DB91Context())
            {
                foreach (var obj in db.DB91s.Where(f => f.isLike == 1 && f.isVideoDownloaded < 0))
                {
                    obj.isVideoDownloaded = 0;
                }
                db.SaveChanges();
            }
            return Json(1);
        }

        public IActionResult GetImg(string imgs)
        {
            if (string.IsNullOrEmpty(imgs))
            {
                return Content("NoPIC");
            }
            var fileName = "img" + SearchViewModel.MD5Encrypt16(imgs);
            if (System.IO.File.Exists(CommonFunc.cachePath + fileName))
            {
                return File(cacheVirtualPath + fileName, "image/Jpeg");
            }
            CommonFunc.MergeImgs(imgs, fileName);
            return File(cacheVirtualPath + fileName, "image/Jpeg");
        }

        public IActionResult Add(string url, List<IFormFile> files)
        {
            var message = "";
            if (!string.IsNullOrEmpty(url))
            {
                message = bll.Add(url, files);
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
                obj = bll.Edit(id, title, files, out message);
            }
            else
            {
                obj = bll.GetByID(id);
                if (obj == null)
                {
                    message = "not fount";
                }
            }
            ViewBag.msg = message;
            return View(obj);
        }


        public IActionResult Delete(int id)
        {
            bll.Delete(id);
            return Json(1);
        }

        public IActionResult m3u8fix(int id, int isHD)
        {
            if (isHD == -1)
            {
                var (url, cont) = CommonFunc.GetFixedM3u8(id);
                return Content(cont, "application/x-mpegURL");
            }

            var result = CommonFunc.M3u8fix(id, isHD);
            return Content(result, "application/x-mpegURL");
        }

        public IActionResult GetTypes()
        {
            return Json(bll.GetTypes());
        }

        private static string _swcache = "";
        public IActionResult SwCache()
        {
            if (string.IsNullOrEmpty(_swcache))
            {
                string path = ContentPath;
                path = path.TrimEnd('/', '\\') + "/_sw-cache.js";
                _swcache = System.IO.File.ReadAllText(path);
                _swcache = _swcache.Replace("{version}", VersionHelper.CompileTime.ToString());
            }

            return Content(_swcache, "application/javascript");
        }
    }
}
