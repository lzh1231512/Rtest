using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;

namespace DL91
{
    public class DownloadTask
    {
        public bool isJsonImg { set; get; } = false;
        public string url { set; get; }
        public string savepath { set; get; }
        public int result { set; get; }
        public long fileSize { set; get; }
    }
    public class DownloadHelper
    {
        private static Object locker = new Object();
        private static int _runingCount = 0;
        private static int runingCount
        {
            get
            {
                lock (locker)
                {
                    return _runingCount;
                }
            }
        }
        private static void addCount()
        {
            lock (locker)
            {
                _runingCount++;
            }
        }
        private static void minusCount()
        {
            lock (locker)
            {
                _runingCount--;
            }
        }
        
        public static List<DownloadTask> DL(List<DownloadTask> tasks, int theadCount)
        {
            foreach (var item in tasks)
            {
                while (runingCount >= theadCount)
                {
                    Thread.Sleep(10);
                }
                addTask(item);
            }
            while (runingCount > 0)
            {
                Thread.Sleep(10);
            }
            return tasks;
        }

        private static void addTask(DownloadTask task)
        {
            addCount();
            Task.Factory.StartNew(() =>
            {
                task.result = 2;
                for (int i = 0; i < 10; i++)
                {
                    if (DLSingleFile(task, i == 9))
                    {
                        task.result = 1;
                        break;
                    }
                }
                minusCount();
            });
        }


        private static bool DLSingleFile(DownloadTask task, bool isFinal)
        {
            try
            {
                FileInfo fi = new FileInfo(task.savepath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                if (fi.Exists)
                    fi.Delete();

                LogTool.Instance.Info("download " + task.url);

                if (task.isJsonImg)
                {
                    return downJsonImg(task);
                }
                else
                {
                    using FileStream fs = new FileStream(task.savepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    var dl = HttpHelper.DownloadFile(task.url, fs);
                    if (!dl.IsGood)
                    {
                        return false;
                    }
                    task.fileSize = dl.Length;
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (isFinal)
                {
                    LogTool.Instance.Error("DLSingleFile Failed", ex);
                }
                return false;
            }
        }

        private static bool downJsonImg(DownloadTask task)
        {
            using var httpClient = new System.Net.Http.HttpClient();
            var response = httpClient.GetAsync(task.url).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            var jsonStr = response.Content.ReadAsStringAsync().Result;
            using JsonDocument doc = JsonDocument.Parse(jsonStr);
            string base64WithPrefix = doc.RootElement
                .GetProperty("content")
                .GetProperty("base64")
                .GetString();

            // 去掉前缀
            string base64 = base64WithPrefix.Substring(base64WithPrefix.IndexOf(",") + 1);

            // 转换为字节数组
            byte[] imgBytes = Convert.FromBase64String(base64);

            // 使用 Magick.NET 处理图片
            using var image = new MagickImage(imgBytes);

            int maxSize = 1000;
            if (image.Width > maxSize || image.Height > maxSize)
            {
                double scale = Math.Min((double)maxSize / image.Width, (double)maxSize / image.Height);
                int newWidth = (int)(image.Width * scale);
                int newHeight = (int)(image.Height * scale);
                image.Resize(newWidth, newHeight);
            }

            image.Write(task.savepath);
            task.fileSize = new FileInfo(task.savepath).Length;
            return true;
        }
    }
}
