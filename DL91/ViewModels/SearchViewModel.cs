using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public string HashCode
        {
            get
            {
                return title1 + ";;" + title2 + ";;" + isLike + ";;" + typeId + ";;" + Page?.PageHashCode;
            }
        }

        public List<DataViewModel> Data { set; get; }
        public string NextPageIDs { set; get; }

        public SearchViewModel ClonePage(int page)
        {
            if (Page == null || page <= 0 || page > Page.PageCount)
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

        public SearchViewModel NextPage
        {
            get
            {
                return ClonePage((Page?.CurrentPage ?? 0) + 1);
            }
        }

        public SearchViewModel PrevPage
        {
            get
            {
                return ClonePage((Page?.CurrentPage ?? 0) - 1);
            }
        }

        public SearchViewModel LastPage
        {
            get
            {
                return ClonePage(Page?.PageCount ?? 0);
            }
        }
    }
}
