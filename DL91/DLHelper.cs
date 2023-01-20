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
    public class DLTask
    {
        public string url { set; get; }
        public string savepath { set; get; }
        public int result { set; get; }
        public long fileSize { set; get; }
    }
    public class DLHelper
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
        
        public static List<DLTask> DL(List<DLTask> tasks, int theadCount)
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

        private static void addTask(DLTask task)
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


        private static bool DLSingleFile(DLTask task,bool isFinal)
        {
            try
            {
                FileInfo fi = new FileInfo(task.savepath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                if (fi.Exists)
                    fi.Delete();

                Console.WriteLine("download " + task.url);

                FileStream fs = new FileStream(task.savepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                HttpWebRequest request = WebRequest.Create(task.url) as HttpWebRequest;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                byte[] bArr = new byte[102400];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                long fileSize = size;
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    fileSize += size;
                }
                fs.Close();
                responseStream.Close();
                task.fileSize = fileSize;
                return true;
            }
            catch (Exception ex)
            {
                if (isFinal)
                {
                    Console.WriteLine(ex.Message);
                }
                return false;
            }
        }
    }
}
