using DL91.Jobs;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91.BLL
{
    public class WebBLL: IWebBLL
    {
        private IContent content { set; get; }
        public WebBLL(IContent content)
        {
            this.content = content;
        }

        public string Add(string url, List<UploadedFile> files)
        {
            string message;
            if (files == null || files.Count == 0)
            {
                message = "need cover";
            }
            else if (files.Any(f => !f.FileName.ToLower().EndsWith(".jpg")))
            {
                message = "cover must be .jpg";
            }
            else
            {
                var id = -1;
                using (var db = new DB91Context())
                {
                    var theType = db.DBTypes.Where(f => f.id == -2).Select(f => new DBType()
                    {
                        id = f.id,
                        name = f.name
                    }).FirstOrDefault();
                    if (theType == null)
                    {
                        db.DBTypes.Add(new DBType()
                        {
                            id = -2,
                            name = "Upload"
                        });
                        db.SaveChanges();
                    }
                    var min = db.DB91s.OrderBy(f => f.id).FirstOrDefault()?.id;
                    if (min.HasValue && min <= id)
                    {
                        id = min.Value - 1;
                    }
                }
                string path = content.ContentPath;
                var folder = path.TrimEnd('/', '\\') + "/imgs/-1/";
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                MagickReadSettings settings = new MagickReadSettings();
                settings.Width = 320;
                settings.Height = 180;
                foreach (var file in files)
                {
                    var fileName = file.FileName.Replace(".jpg", "");
                    string fullPath = path.TrimEnd('/', '\\') + "/imgs/-1/" + id + ".jpg";
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                        stream.Close();
                    }

                    MagickImage canvas = new MagickImage("xc:white", settings);
                    canvas.Format = MagickFormat.Jpeg;

                    var first = new MagickImage(fullPath);
                    var h = (int)(320 * first.Height / first.Width);
                    if (h < 180)
                    {
                        first.Resize(320, h);
                        canvas.Composite(first, 0, (180 - h) / 2);
                    }
                    else
                    {
                        var w = (int)(180 * first.Width / first.Height);
                        first.Resize(w, 180);
                        canvas.Composite(first, (320 - w) / 2, 0);
                    }
                    first.Dispose();
                    canvas.Write(fullPath);

                    var dt91 = new DateTime(1990, 1, 1);
                    using (var db = new DB91Context())
                    {
                        db.DB91s.Add(new DB91()
                        {
                            id = id,
                            title = fileName,
                            url = url.TrimEnd('/') + "/" + fileName + ".m3u8",
                            typeId = -2,
                            IsImgDownload = 1,
                            createDate = (int)(DateTime.UtcNow - dt91).TotalMinutes
                        });
                        db.SaveChanges();
                    }
                    id -= 1;
                }

                CacheManager.ClearCache();
                message = "add successful:" + id;
            }

            return message;
        }

        public DB91 Edit(int id, string title, UploadedFile files, out string message)
        {
            DB91 result = null;
            message = "";
            if (files != null && !files.FileName.ToLower().EndsWith(".jpg"))
            {
                message = "cover must be .jpg";
            }
            else
            {
                using (var db = new DB91Context())
                {
                    result = db.DB91s.FirstOrDefault(f => f.id == id);
                    if (result == null)
                    {
                        message = "not fount";
                        return result;
                    }
                    result.title = title;
                    db.SaveChanges();
                }
                if (files != null)
                {
                    var folder = content.ContentPath.TrimEnd('/', '\\') + "/imgs/-1/";
                    if (!System.IO.Directory.Exists(folder))
                    {
                        System.IO.Directory.CreateDirectory(folder);
                    }
                    MagickReadSettings settings = new MagickReadSettings();
                    settings.Width = 320;
                    settings.Height = 180;

                    string fullPath = content.ContentPath.TrimEnd('/', '\\') + "/imgs/" + (id < 0 ? -1 : (((id / 1000) * 1000))) + "/" + id + ".jpg";
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        files.CopyTo(stream);
                        stream.Close();
                    }
                    MagickImage canvas = new MagickImage("xc:white", settings);
                    canvas.Format = MagickFormat.Jpeg;
                    var first = new MagickImage(fullPath);
                    var h = (int)(320 * first.Height / first.Width);
                    if (h < 180)
                    {
                        first.Resize(320, h);
                        canvas.Composite(first, 0, (180 - h) / 2);
                    }
                    else
                    {
                        var w = (int)(180 * first.Width / first.Height);
                        first.Resize(w, 180);
                        canvas.Composite(first, (320 - w) / 2, 0);
                    }
                    first.Dispose();
                    canvas.Write(fullPath);
                }
                message = "update successful";
            }
            CacheManager.ClearCache();
            return result;
        }

        public void Delete(int id)
        {
            using (var db = new DB91Context())
            {
                var img = content.ContentPath.TrimEnd('/', '\\') + "/imgs/" + (id < 0 ? "-1" : (id / 1000).ToString()) + "/" + id + ".jpg";
                if (System.IO.File.Exists(img))
                    System.IO.File.Delete(img);

                var obj = db.DB91s.FirstOrDefault(f => f.id == id);
                if (obj != null)
                {
                    db.DB91s.Remove(obj);
                }
                db.SaveChanges();
            }
            CacheManager.ClearCache();
        }

        public void Like(int id, int isLike)
        {
            using (var db = new DB91Context())
            {
                var obj = db.DB91s.Where(f => f.id == id).FirstOrDefault();
                obj.isLike = isLike;
                if (obj.isVideoDownloaded == 2)
                    obj.isVideoDownloaded = 0;
                db.SaveChanges();
            }
            AutoProcessService.DownloadVideoFlag = 0;
            CacheManager.NeedClearLikeCache = true;
        }
        public void ResetFailedVideo()
        {
            using (var db = new DB91Context())
            {
                foreach (var obj in db.DB91s.Where(f => f.isLike == 1 && f.isVideoDownloaded < 0))
                {
                    obj.isVideoDownloaded = 0;
                }
                db.SaveChanges();
            }
        }


        public List<DBType> GetTypes()
        {
            var typeLst = new List<DBType>();
            using (var db = new DB91Context())
            {
                typeLst = db.DBTypes.Where(f => !ConfigurationHelper.DisabledType.Contains(f.id)).Select(f => new DBType()
                {
                    id = f.id,
                    name = f.name
                }).OrderBy(f => f.name).ToList();
                typeLst.Insert(0, new DBType()
                {
                    id = 0,
                    name = "ALL"
                });
                return typeLst.ToList();
            }
        }

        public DB91 GetByID(int id)
        {
            using (var db = new DB91Context())
            {
                return db.DB91s.FirstOrDefault(f => f.id == id);
            }
        }
    }
}
