using DL91;
using DL91.BLL;
using DL91.Jobs;
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

        private IWebBLL bll;
        private readonly IContent _content;

        public HomeController(IWebBLL bll, IContent content)
        {
            this._content = content;
            this.bll = bll;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
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
            bll.Search(model, currentPage, new CookiesHelper().GetPageSize(HttpContext));
            return Json(model);
        }

        public IActionResult like(int id, int isLike)
        {
            bll.Like(id, isLike);
            return Json(1);
        }

        public IActionResult sync()
        {
            AutoProcessService.SyncFlag = 0;
            return Json(1);
        }
        public IActionResult ResetFailedVideo()
        {
            bll.ResetFailedVideo();
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
                message = bll.Add(url, files.Select(f => f.ToUploadedFile()).ToList());
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
                obj = bll.Edit(id, title, files.ToUploadedFile(), out message);
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
            var (url, cont, domain) = CommonFunc.GetFixedM3u8(id);
            if (isHD == -1)
            {
                return Content(cont, "application/x-mpegURL");
            }
            var result = CommonFunc.M3u8fix(id, isHD, domain);
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
                string path = _content.ContentPath;
                path = path.TrimEnd('/', '\\') + "/_sw-cache.js";
                _swcache = System.IO.File.ReadAllText(path);
                _swcache = _swcache.Replace("{version}", VersionHelper.CompileTime.ToString());
            }

            return Content(_swcache, "application/javascript");
        }
    }
}
