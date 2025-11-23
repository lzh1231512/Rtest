using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DL91.Models
{
    public class DataViewModel
    {
        public int Id { set; get; }
        public int CreateDate { set; get; }
        public int time { set; get; }
        public string Title { set; get; }

        public int IsLike { set; get; }
        public int IsHD { set; get; }
        public string FileSize { set; get; }
        public string Url { set; get; }

    }
}
