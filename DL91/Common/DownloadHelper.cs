using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private static Object locker = new object();
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
                    using var httpClient = new System.Net.Http.HttpClient();
                    var response = httpClient.GetAsync(task.url).Result;
                    if (!response.IsSuccessStatusCode)
                        return false;
                    var jsonStr = response.Content.ReadAsStringAsync().Result;
                    var jsonObj = System.Text.Json.JsonDocument.Parse(jsonStr);
                    if (!jsonObj.RootElement.TryGetProperty("content", out var contentElem))
                        return false;
                    if (!contentElem.TryGetProperty("base64", out var base64Elem))
                        return false;
                    var base64Str = base64Elem.GetString();
                    if (string.IsNullOrEmpty(base64Str))
                        return false;
                    // 去除前缀
                    var idx = base64Str.IndexOf(",");
                    if (idx >= 0)
                        base64Str = base64Str.Substring(idx + 1);
                    byte[] imgBytes;
                    try
                    {
                        imgBytes = Convert.FromBase64String(base64Str);
                    }
                    catch
                    {
                        return false;
                    }
                    File.WriteAllBytes(task.savepath, imgBytes);
                    task.fileSize = imgBytes.Length;
                    return true;
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
    }
}
