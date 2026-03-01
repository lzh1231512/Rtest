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

                using (var db = new DB91Context())
                {
                    var lst = db.DB91s.Where(f => f.IsImgDownload == 0).Take(200);
                    if (!lst.Any())
                        break;

                    foreach (var item in lst)
                    {
                        item.imgUrl = item.imgUrl;
                    }

                    var dLst = lst.Select(f => new DownloadTask()
                    {
                        url = f.imgUrl,
                        savepath = getImgSavePath(f)
                    });
                    var dLst2 = DownloadHelper.DL(dLst.ToList(), 8);

                    foreach (var item in lst)
                    {
                        item.IsImgDownload = dLst2.Single(f => f.url == item.imgUrl).result;
                    }
                    db.SaveChanges();
                    downloaded += lst.Count();
                }
            }
        }

        public static void DownloadImgAPI()
        {
            var downloaded = 0;
            while (true)
            {
                LogTool.Instance.Info("download img " + downloaded);
                using (var db = new DB91Context())
                {
                    var lst = db.DB91s.Where(f => f.IsImgDownload == 0).Take(200);
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
                        var encrypted = ImgIndexHelper.ProcessAsync(item.id);
                        if (string.IsNullOrEmpty(encrypted))
                        {
                            LogTool.Instance.Error("img encrypt failed " + item.id);
                            continue;
                        }
                        item.imgUrl= imgBaseUrl + encrypted;
                        dLst.Add(new DownloadTask()
                        {
                            url = item.imgUrl,
                            isJsonImg = true,
                            savepath = getImgSavePathAPI(item.id)
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
            }
        }
        public static void DownloadImg(List<int> ids)
        {
            var imgBaseUrl = AutoProcessService.domain + "/api/videos/img/";
            var dLst = new List<DownloadTask>();
            foreach (var item in ids)
            {
                var encrypted = ImgIndexHelper.ProcessAsync(item);
                if (string.IsNullOrEmpty(encrypted))
                {
                    LogTool.Instance.Error("img encrypt failed " + item);
                    continue;
                }
                dLst.Add(new DownloadTask()
                {
                    url = imgBaseUrl + encrypted,
                    isJsonImg = true,
                    savepath = getImgSavePathAPI(item)
                });
            }
            DownloadHelper.DL(dLst.ToList(), 8);
        }
        private static string getImgSavePathAPI(int id)
        {
            return "wwwroot/imgs/" + (id / 1000) + "/" + id + ".jpg";
        }
        private static string getImgSavePath(DB91 task)
        {
            return "wwwroot/imgs/" + (task.id / 1000) + "/" + task.id + ".jpg";
        }
    }
}
