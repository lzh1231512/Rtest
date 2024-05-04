using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91.WebProcess
{
    public class ProcessVideo
    {
        public const int VideoDownloadTryTimtLimit = -50;

        public static void DownloadVideo()
        {
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.isLike == 0
                    && f.isVideoDownloaded == 1))
                {
                    if (Directory.Exists(getVideoSaveFolder(item.id)))
                    {
                        Console.WriteLine("Delete folder " + getVideoSaveFolder(item.id));
                        Directory.Delete(getVideoSaveFolder(item.id), true);
                    }
                    item.isVideoDownloaded = 0;
                    db.SaveChanges();
                }
            }
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.isLike == 1 && f.id > 0
                    && f.isVideoDownloaded > VideoDownloadTryTimtLimit))
                {
                    if (item.isVideoDownloaded == 1)
                    {
                        if (File.Exists(getVideoSavePath(item.id, CommonFunc.GetM3u8Url(item.id, 0, false))))
                        {
                            continue;
                        }
                    }
                    try
                    {
                        Console.WriteLine("download video " + item.id);
                        item.isVideoDownloaded = downloadM3u8(item.id, item.isVideoDownloaded, out long fileSize);
                        item.videoFileSize = fileSize;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("download video " + item.id + "Failed:" + e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    db.SaveChanges();
                }
            }
        }

        private static int downloadM3u8(int id, int downloadtime, out long fileSize)
        {
            fileSize = 0;
            var (m3url, html) = CommonFunc.GetFixedM3u8(id);
            Console.WriteLine("Start Download:" + m3url);
            if (string.IsNullOrEmpty(html))
            {
                var p = HttpHelper.GetHtml(m3url);
                if (!p.IsGood)
                {
                    return downloadtime - 1;
                }
                html = p.Html;
            }

            var info = html.Split("\n").Select(f => f.Trim(' ', '\r'));
            var urls = info.Where(f => !f.ToLower().StartsWith("#") && !string.IsNullOrEmpty(f.Trim()));
            var dLst = urls.Select(f => new DownloadTask()
            {
                url = f.ToLower().StartsWith("http") ? f : m3url.Replace("index.m3u8", f),
                savepath = getVideoSavePath(id, f)
            });
            var dLst2 = DownloadHelper.DL(dLst.ToList(), 1);
            if (dLst2.Any(f => f.result != 1))
            {
                return downloadtime - 1;
            }
            fileSize = dLst2.Sum(f => f.fileSize);
            using (FileStream fs = new FileStream(getVideoSavePath(id, m3url), FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var item in info)
                    {
                        if (item.ToLower().StartsWith("http"))
                        {
                            var name = item.Substring(item.LastIndexOf('/') + 1);
                            sw.Write(name + '\n');
                        }
                        else
                        {
                            sw.Write(item + '\n');
                        }
                    }
                    sw.Close();
                    fs.Close();
                }
            }
            Console.WriteLine("Download finish:" + m3url);
            return 1;
        }

        private static string getVideoSavePath(int id, string url)
        {
            var name = url.Substring(url.LastIndexOf('/') + 1);
            return getVideoSaveFolder(id) + "/" + name;
        }
        private static string getVideoSaveFolder(int id)
        {
            return "wwwroot/video/" + (id / 1000) + "/" + id;
        }
    }
}
