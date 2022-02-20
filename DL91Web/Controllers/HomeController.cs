using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DL91Web.Models;
using DL91Web.Helpers;
using DL91;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

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
                var dt3 = lst.OrderByDescending(f => f.id);
                model.Data = dt3.Skip((currentPage - 1) * model.Page.PageSize).Take(model.Page.PageSize)
                    .Select(f => new DataViewModel()
                    {
                        Id = f.id,
                        Title =(f.isHD?"[HD]":"")+  getTimeString(f.time) + getTypeName(f.typeId, TypeLst) + "</br>"+ f.title
                    }).ToList();
                model.Page.RecordCount = dt3.Count();
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
            var urls = getM3u8(id);
            using (var db = new DB91Context())
            {
                var TypeLst = db.DBTypes.Select(f => new DBType()
                {
                    id = f.id,
                    name = f.name
                }).ToList();
                var obj = db.DB91s.Where(f => f.id == id).FirstOrDefault();
                ViewBag.title = (obj.isHD ? "[HD]" : "") + getTypeName(obj.typeId, TypeLst) + obj?.title;
                ViewBag.isLike = obj?.isLike;
                if (obj.isVideoDownloaded == 1)
                {
                    ViewBag.fileSize = GetFileSize(obj.videoFileSize);
                    urls.Add(url + "/video/" + (id / 1000) + "/" + id + "/index.m3u8");
                }
            }
            ViewBag.urls = urls;
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
                db.SaveChanges();
            }
            return Json(1);
        }

        private List<string> getM3u8(int id)
        {
            var result = new List<string>();
            var domain = id < 72125 ? "https://cust91rb.163cdn.net" : "https://cust91rb2.163cdn.net";
            var url1 = domain + "/hls/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_720p.mp4/index.m3u8";
            if (testHttp(url1))
                result.Add(url1);
            result.Add(domain + "/hls/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8");
            return result;
        }

        private bool testHttp(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "*/*";
                request.CookieContainer = new CookieContainer();
                HttpWebResponse respons = (HttpWebResponse)request.GetResponse();
                return respons.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private string getTimeString(int time)
        {
            return string.Format("[{0:D2}:{1:D2}]", time / 60, time % 60);
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
    }
}
