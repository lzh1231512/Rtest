using DL91.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91.WebProcess
{
    public class ProcessImg
    {
        public static void DownloadImg()
        {
            var downloaded = 0;
            while (true)
            {
                LogTool.Instance.Info("download img " + downloaded);
                var tool = new PlaywrightTool();
                using (var db = new DB91Context())
                {
                    var lst = db.DB91s.Where(f => f.IsImgDownload == 0).Take(3);
                    if (!lst.Any())
                        break;

                    foreach (var item in lst)
                    {
                        item.imgUrl = item.imgUrl;
                    }
                    var imgBaseUrl = AutoProcessService.domain + "/api/videos/img/";
                    var dLst = new List<DownloadTask>();
                    foreach (var item in lst)
                    {
                        var encrypted = tool.ProcessAsync(item.id.ToString());
                        encrypted.Wait();
                        if (string.IsNullOrEmpty(encrypted.Result))
                            throw new Exception("img encrypt failed " + item.id);
                        item.imgUrl= imgBaseUrl + encrypted.Result;
                        dLst.Add(new DownloadTask()
                        {
                            url = item.imgUrl,
                            savepath = getImgSavePath(item)
                        });
                    }
                    var dLst2 = DownloadHelper.DL(dLst.ToList(), 8);

                    foreach (var item in lst)
                    {
                        item.IsImgDownload = dLst2.Single(f => f.url == item.imgUrl).result;
                    }
                    db.SaveChanges();
                    downloaded += lst.Count();
                }

                break;
            }
        }

        private static string getImgSavePath(DB91 task)
        {
            return "wwwroot/imgs/" + (task.id / 1000) + "/" + task.id + ".jpg";
        }
    }
}
