using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DL91Web.Helpers
{
    public class CookiesHelper
    {
        private int _PageSize = 0;

        public int GetPageSize(HttpContext Current)
        {
            var Request = Current.Request;

            var inPageSize = Request.Query["inPageSize"].ToString();
            if (string.IsNullOrEmpty(inPageSize))
            {
                try
                {
                    inPageSize = Request.Form["inPageSize"].ToString();
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(inPageSize))
            {
                int newPageSize;
                if (int.TryParse(inPageSize, out newPageSize))
                {
                    SetPageSize(Current, newPageSize);
                }
            }
            if (_PageSize > 0)
                return _PageSize;
            if (Request.Cookies["PageSize"] != null)
                return int.Parse(Request.Cookies["PageSize"]);
            _PageSize = 24;
            return _PageSize;
        }

        public void SetPageSize(HttpContext Current, int value)
        {
            var Response = Current.Response;
            if (value > 0)
            {
                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                {
                    Path = "/",
                    HttpOnly = false,
                    IsEssential = true, //<- there
                    Expires = DateTime.Now.AddMonths(1),
                };
                _PageSize = value;
                Response.Cookies.Append("PageSize", value.ToString(), cookieOptions);
            }
        }

    }
}
