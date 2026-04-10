using DL91.Jobs;
using DL91.WebProcess;
using HtmlAgilityPack;
using MihaZupan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DL91
{
    public class TestHtml
    {
        public static void TestType()
        {
            var Html = File.ReadAllText("htmlsample/type.txt");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(Html);

            var categoryNodeList = doc.DocumentNode.SelectNodes("//div[@id='list_categories_categories_list_items']/a[@class='item']");
            for (int i = 0; i < categoryNodeList.Count; i++)
            {
                HtmlNode nat = categoryNodeList[i];

                var url = nat.Attributes["href"].Value.Replace(AutoProcessService.domain, "");
                var name = nat.Attributes["title"].Value;
            }
        }

        public static void TestList()
        {
            var html = File.ReadAllText("htmlsample/list.txt");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode navNode = doc.GetElementbyId("list_videos_latest_videos_list_items");
            HtmlNodeCollection categoryNodeList = navNode.SelectNodes("div");

            var dt91 = new DateTime(1990, 1, 1);
            for (int i = 0; i < categoryNodeList.Count; i++)
            {
                HtmlNode nat = categoryNodeList[i];
                var atag = nat.SelectNodes("a")[0];
                String href = atag.Attributes["href"].Value;
                href = href.Replace(AutoProcessService.domain, "");
                var a = href.IndexOf('/', 2);
                var b = href.IndexOf('/', a + 1);
                var id = int.Parse(href.Substring(a + 1, b - a - 1));
                String title = atag.Attributes["title"].Value;
                String img = atag.SelectNodes("div")[0].SelectNodes("img")[0].Attributes["data-original"].Value;
                var time = atag.SelectNodes("div")[0].SelectNodes("div")[0].SelectNodes("div")[1].InnerText.Trim('\r', '\n', ' ').Split(':');
                var isHD = nat.SelectNodes("a//span[@class='is-hd']") != null;
                int timeInt = 0;
                if (time.Length == 3)
                {
                    timeInt = int.Parse(time[0]) * 60 * 60 + int.Parse(time[1]) * 60 + int.Parse(time[2]);
                }
                else if (time.Length == 2)
                {
                    timeInt = int.Parse(time[0]) * 60 + int.Parse(time[1]);
                }

            }
        }

        public static void TestDetail()
        {
            var Html = File.ReadAllText("htmlsample/detail.txt");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(Html);

            var navNode = doc.DocumentNode.SelectNodes("//div[@class='top-options flex']");
            if (navNode != null&& navNode.Count>0)
            {
                var types = navNode[0].SelectNodes("div")[1].SelectNodes("a")[0].InnerHtml;
            }
        }

        public static string TestDetailImgURL()
        {
            try
            {
                var Html = File.ReadAllText("htmlsample/detail.txt");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Html);

                var navNode = doc.GetElementbyId("list_videos_related_videos_items");
                if (navNode != null)
                {
                    return navNode.SelectNodes(".//img[@class='lazy-load']")[0].Attributes["data-original"].Value;
                }
            }
            catch { }
            return null;
        }

        public static async Task<string> TestHttp()
        {
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 1080);
            var handler = new HttpClientHandler { Proxy = proxy };
            HttpClient httpClient = new HttpClient(handler, true);


            var dLst = new List<DownloadTask>();
            dLst.Add(new DownloadTask()
            {
                url = "https://www.91rb.com/contents/videos_screenshots/315000/315492/320x180/1.jpg?_=123123",
                isJsonImg = false,
                savepath = "D://test.jpg"
            });
            DownloadHelper.DL(dLst.ToList(), 1);



            //var keyword = "";

            //var result = await httpClient.SendAsync(
            //    new HttpRequestMessage(HttpMethod.Get, "https://www.r1091.com/search/"+ keyword + "/"));

            //var html = await result.Content.ReadAsStringAsync();

            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml(html);

            //var navNode = doc.GetElementbyId("list_videos_videos_list_search_result")
            //    ?.SelectSingleNode(".//a[@title='" + keyword + "']");
            //if (navNode != null)
            //{
            //    var url = navNode.Attributes["href"];
            //    var imgURL= navNode.SelectSingleNode(".//img[@class='lazy-load']")?.Attributes["data-original"].Value;
            //}

            return "";
        }
    }
}
