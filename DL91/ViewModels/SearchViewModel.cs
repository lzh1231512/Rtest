using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace DL91.Models
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            isLike = 2;
        }
        public Pager Page { set; get; }
        public string title1 { set; get; }
        public string title2 { set; get; }
        public int isLike { set; get; }
        public int typeId { set; get; }
        public DateTime CachedTime { set; get; }
        public string HashCode
        {
            get
            {
                return isLike + MD5Encrypt16(title1 + ";;" + title2 + ";;" + typeId + ";;" + Page?.PageHashCode);
            }
        }

        public static string MD5Encrypt16(string password)
        {
            var md5 = MD5.Create();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password)), 4, 8);
            t2 = t2.Replace("-", string.Empty);
            return t2;
        }

        public List<DataViewModel> Data { set; get; }
        public string NextPageIDs { set; get; }

        public SearchViewModel ClonePage(int page)
        {
            if (Page == null || (page <= 0 || page > Page.PageCount) && page != -1)
            {
                return null;
            }
            return new SearchViewModel()
            {
                title1 = title1,
                title2 = title2,
                isLike = isLike,
                typeId = typeId,
                Page = new Pager()
                {
                    CurrentPage = page,
                    PageSize = Page.PageSize,
                    Sort = Page.Sort
                }
            };
        }
        [JsonIgnore]
        public SearchViewModel NextPage
        {
            get
            {
                return ClonePage((Page?.CurrentPage ?? 0) + 1);
            }
        }
        [JsonIgnore]
        public SearchViewModel PrevPage
        {
            get
            {
                return ClonePage((Page?.CurrentPage ?? 0) - 1);
            }
        }

        [JsonIgnore]
        public SearchViewModel LastPage
        {
            get
            {
                return ClonePage(Page?.PageCount ?? 0);
            }
        }
    }
}
