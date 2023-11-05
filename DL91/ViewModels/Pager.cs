using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DL91.Models
{
    [Serializable]
    public class Pager
    {
        public int PageCount
        {
            get
            {
                return RecordCount / PageSize + (RecordCount % PageSize == 0 ? 0 : 1);
            }
        }

        private string _sort;

        public string Sort
        {
            get { return _sort; }
            set
            {
                if (value != null)
                {
                    if (value.Length > 50)
                        throw new Exception("sort column is too long");
                    if (value.Split(' ').Length > 2
                        || value.Contains("\r")
                        || value.Contains("\n")
                        || (!value.EndsWith(" asc") && !value.EndsWith(" desc")))
                    {
                        _sort = "";
                    }
                    else
                    {
                        _sort = value;
                    }
                }
            }
        }

        public int CurrentPage { get; set; }
        public int RecordCount { get; set; }
        public int PageSize { get; set; }

        public string PageHashCode
        {
            get
            {
                return CurrentPage + "_" + PageSize + "_" + Sort;
            }
        }
    }
}
