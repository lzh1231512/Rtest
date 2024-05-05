using DL91.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91.BLL
{
    public interface IWebBLL
    {
        string Add(string url, List<UploadedFile> files);
        DB91 Edit(int id, string title, UploadedFile files, out string message);
        void Delete(int id);
        void Like(int id, int isLike);
        void ResetFailedVideo();
        List<DBType> GetTypes();
        DB91 GetByID(int id);
        void Search(SearchViewModel model, int currentPage, int pageSize);
    }
}
