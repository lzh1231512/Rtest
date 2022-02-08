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

namespace DL91Web.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
        }

        public IActionResult Index(SearchViewModel model, int currentPage = 1, bool isAjax = false)
        {
            model.Page = new Pager()
            {
                CurrentPage = currentPage,
                PageSize = new CookiesHelper().GetPageSize(HttpContext)
            };

            using (var db = new DB91Context())
            {
                var c1 = (model.title1 ?? "").Split(' ');
                var c2 = (model.title2 ?? "").Split(' ');
                var dt3 = db.DB91s.Where(f =>
                    c1.All(z => f.title.Contains(z)) &&
                    c2.All(z => z == "" || !f.title.Contains(z)) &&
                    model.isLike == 2 || f.isLike == model.isLike
                ).OrderByDescending(f => f.id);
                model.Data = dt3.Skip((currentPage - 1) * model.Page.PageSize).Take(model.Page.PageSize)
                    .Select(f => new DataViewModel()
                    {
                        Id = f.id,
                        Title = getTimeString(f.time) +" "+ f.title
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
            using (var db = new DB91Context())
            {
                var obj = db.DB91s.Where(f => f.id == id).FirstOrDefault();
                ViewBag.title = obj?.title;
                ViewBag.isLike = obj?.isLike;
                if (obj.isVideoDownloaded == 1)
                {
                    ViewBag.url2 = url + "/video/" + (id / 1000) + "/" + id + "/index.m3u8";
                }
            }
            ViewBag.url = getM3u8(id);
            ViewBag.id = id;
            return View();
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

        private string getM3u8(int id)
        {
            var domain = id < 72125 ? "https://cust91rb.163cdn.net" : "https://cust91rb2.163cdn.net";
            var result = domain + "/hls/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_720p.mp4/index.m3u8";
            if (testHttp(result))
                return result;
            return domain + "/hls/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8";
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
            return string.Format("{0:D2}:{1:D2}", time / 60, time % 60);
        }

    }
}
