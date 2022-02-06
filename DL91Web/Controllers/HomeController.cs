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
                    c1.All(z => f.title.Contains(z)) && c2.All(z => z == "" || !f.title.Contains(z))
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
        public IActionResult img(int id)
        {
            try
            {
                return File(FileToByte(getSavePath(id)), @"image/jpg");
            }
            catch
            {
                return null;
            }
        }

        public IActionResult IndexForAjax(SearchViewModel model, int currentPage = 1)
        {
            return Index(model, currentPage, true);
        }

        public IActionResult play(int id)
        {
            using (var db = new DB91Context())
            {
                ViewBag.title = db.DB91s.Where(f => f.id == id).FirstOrDefault()?.title;
            }
            var domain = id < 72125 ? "https://cust91rb.163cdn.net" : "https://cust91rb2.163cdn.net";
            ViewBag.url = domain + "/hls/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8";
            return View();
        }

        private string getTimeString(int time)
        {
            return string.Format("{0:D2}:{1:D2}", time / 60, time % 60);
        }


        private byte[] FileToByte(string fileUrl)
        {
            try
            {
                using (FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read))
                {
                    byte[] byteArray = new byte[fs.Length];
                    fs.Read(byteArray, 0, byteArray.Length);
                    return byteArray;
                }
            }
            catch
            {
                return null;
            }
        }
        private string getSavePath(int id)
        {
            return "imgs/" + (id / 1000) + "/" + id + ".jpg";
        }   
    }
}
