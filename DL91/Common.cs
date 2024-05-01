using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DL91
{
    public class Common
    {

        public static string GetFileSize(long filesize)
        {
            if (filesize <= 0)
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


        public static string GetTimeString(int time)
        {
            return string.Format("[{0:D2}:{1:D2}]", time / 60, time % 60);
        }


        public static string GetTypeName(int typeId, List<DBType> lst)
        {
            var result = lst.SingleOrDefault(f => f.id == typeId);
            if (result == null)
                return "";
            return "[" + result.name + "]";
        }

        public static bool TestHttp(string url)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
                    request.Method = "HEAD";
                    request.Accept = "*/*";
                    request.CookieContainer = new CookieContainer();
                    HttpWebResponse respons = (HttpWebResponse)request.GetResponse();
                    if (respons.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
                catch
                {
                }
                Thread.Sleep(500);
            }
            return false;
        }

        public static string M3u8fix(int id, int isHD)
        {
            var result = @"#EXTM3U
#EXT-X-TARGETDURATION:10
#EXT-X-ALLOW-CACHE:YES
#EXT-X-PLAYLIST-TYPE:VOD
#EXT-X-VERSION:3
#EXT-X-MEDIA-SEQUENCE:1
";
            using (var db = new DB91Context())
            {
                var obj = db.DB91s.FirstOrDefault(f => f.id == id);
                var time = obj.time;
                var url = "https://91rbnet.douyincontent.com" + GetM3u8Url(id, isHD, false).Replace("index.m3u8", "");
                for (int i = 1; time > 0; i++)
                {
                    var t = time >= 10 ? 10 : time;
                    result += @"#EXTINF:" + t + @".000,
" + url + "cdn-" + i + @"-v1-a1.ts
";
                    time -= 10;
                }
            }
            result += @"
#EXT-X-ENDLIST";
            return result;
        }

        public static string GetM3u8Url(int id, int isHD, bool isLocal)
        {
            if (isLocal)
            {
                return "/video/" + (id / 1000) + "/" + id + "/index.m3u8";
            }
            if (isHD == 1)
            {
                return "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_720p.mp4/index.m3u8";
            }
            if (isHD == 2)
            {
                return "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + "_1080p.mp4/index.m3u8";
            }
            return "/hls/contents/videos/" + ((id / 1000) * 1000) + "/" + id + "/" + id + ".mp4/index.m3u8";
        }

        public static (string, string) GetFixedM3u8(int id)
        {
            for (int i = 2; i >= 0; i--)
            {
                var url = "https://91rbnet.douyincontent.com" + GetM3u8Url(id, i,false).Replace("index.m3u8", "");
                if (Common.TestHttp(url + "cdn-1-v1-a1.ts"))
                {
                    return ("https://fj.lzhsb.cc/home/m3u8fix/" + i + "/" + id + "/index.m3u8", M3u8fix(id, i));
                }
            }
            return ("https://91rbnet.douyincontent.com" + GetM3u8Url(id, 0, false), null);
        }
    }
}
