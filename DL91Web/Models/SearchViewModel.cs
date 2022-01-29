using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DL91Web.Models
{
    public class SearchViewModel
    {
        public Pager Page { set; get; }
        public List<DataViewModel> Data { set; get; }
        public string title1 { set; get; }
        public string title2 { set; get; }
    }
}
