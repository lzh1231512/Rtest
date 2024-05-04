using DL91.WebProcess;
using HtmlAgilityPack;
using log4net.Config;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace DL91
{
    public class Job
    {
        public static void Main(string[] args)
        {
            var logRep = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRep, new FileInfo("config/log4net.config"));

        }
        public static void Test2(string[] args)
        {
            var cpath = "E:\\UnboundedSharing\\";
            var mpath = "E:\\UnboundedSharing\\menu\\";
            using (var db = new DB91Context())
            {
                foreach (var item in db.DB91s.Where(f => f.id<0))
                {
                    if (Directory.Exists(cpath + item.id))
                    {
                        Directory.Delete(cpath + item.id, true);
                    }
                    Directory.CreateDirectory(cpath + item.id);

                    var path = "c:\\" + item.url.Substring(item.url.IndexOf("m3u8"));
                    var file = new FileInfo(path);
                    var m3u8 = File.ReadAllText(path);
                    var m3u8info = m3u8.Split('\n').Select(f => f.Replace("\r", ""))
                        .Where(f => !f.StartsWith("#") && !string.IsNullOrEmpty(f));
                    var vIndex = 0;
                    foreach (var ts in m3u8info)
                    {
                        m3u8 = m3u8.Replace(ts, "fileList?filename=" + item.id + "/" + vIndex + "&mime=application/video/MP2T");
                        File.Copy(file.Directory.FullName + "/" + ts, cpath + item.id+"/"+ vIndex);
                        vIndex++;
                    }
                    File.WriteAllText(cpath + item.id + "/m3", m3u8);

                    var obj = new
                    {
                        id = item.id + "",
                        item.createDate,
                        isLike = "0",
                        isHD = "0",
                        fileSize = "",
                        title = "[00:00][Upload]</br>" + item.title,
                        item.url
                    };
                    if (File.Exists(mpath + item.id))
                    {
                        File.Delete(mpath + item.id);
                    }
                    File.WriteAllText(mpath + item.id, JsonConvert.SerializeObject(obj));
                }
            }
        }
    }
}
