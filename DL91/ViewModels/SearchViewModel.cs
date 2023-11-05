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
                return title1 + ";;" + title2 + ";;" + isLike + ";;" + typeId;
            }
        }

        public List<DataViewModel> Data { set; get; }
        public string NextPageIDs { set; get; }

        public SearchViewModel GetNextPage()
        {
            if (Page.NextPage >= 0)
            {
                return new SearchViewModel()
                {
                    title1 = title1,
                    title2 = title2,
                    isLike = isLike,
                    typeId = typeId,
                    Page = new Pager()
                    {
                        CurrentPage = Page.NextPage,
                        PageSize = Page.PageSize,
                        Sort = Page.Sort
                    }
                };
            }
            return null;
        }
    }
}
