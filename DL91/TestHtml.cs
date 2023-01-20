using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

                var url = nat.Attributes["href"].Value.Replace(Job.domain, "");
                var count = 0;
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

            var isExists = false;
            var dt91 = new DateTime(1990, 1, 1);
            for (int i = 0; i < categoryNodeList.Count; i++)
            {
                HtmlNode nat = categoryNodeList[i];
                var atag = nat.SelectNodes("a")[0];
                String href = atag.Attributes["href"].Value;
                href = href.Replace(Job.domain, "");
                var a = href.IndexOf('/', 2);
                var b = href.IndexOf('/', a + 1);
                var id = int.Parse(href.Substring(a + 1, b - a - 1));
                String title = atag.Attributes["title"].Value;
                String img = atag.SelectNodes("div")[0].SelectNodes("img")[0].Attributes["data-original"].Value;
                var time = atag.SelectNodes("div")[1].SelectNodes("div")[0].InnerText.Split(':');
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

            HtmlNode navNode = doc.GetElementbyId("tab_video_info");
            foreach (var atag in navNode.SelectNodes("div//a"))
            {
                var href = atag.Attributes["href"].Value;
                if (href.Contains("categories"))
                {
                    var typeName = atag.InnerText.Trim();
                }
            }
        }
    }
}
