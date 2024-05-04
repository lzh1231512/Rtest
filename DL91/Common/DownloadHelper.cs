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


        private static bool DLSingleFile(DownloadTask task,bool isFinal)
        {
            try
            {
                FileInfo fi = new FileInfo(task.savepath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                if (fi.Exists)
                    fi.Delete();

                LogTool.Instance.Info("download " + task.url);

                using FileStream fs = new FileStream(task.savepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                var dl = HttpHelper.DownloadFile(task.url, fs);
                if (!dl.IsGood)
                {
                    return false;
                }
                task.fileSize = dl.Length;
                return true;
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
