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
                task.result = DLSingleFile(task) ? 1 : 2;
                minusCount();
            });
        }


        private static bool DLSingleFile(DLTask task)
        {
            try
            {
                FileInfo fi = new FileInfo(task.savepath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                if (fi.Exists)
                    fi.Delete();

                FileStream fs = new FileStream(task.savepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                HttpWebRequest request = WebRequest.Create(task.url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                byte[] bArr = new byte[1024];
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
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
