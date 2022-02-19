using System;
using System.Collections.Generic;
using System.Text;

namespace DL91
{
    public class DB91
    {
        public int id { set; get; }
        public string title { set; get; }
        public string url { set; get; }
        public string imgUrl { set; get; }
        public int time { set; get; }
        public int isLike { set; get; }
        public int isVideoDownloaded { set; get; }

        public int IsImgDownload { set; get; }

        public long videoFileSize { set; get; }

        public int typeId { set; get; }

        public bool isHD { set; get; }
    }

    public class DBType
    {
        public int id { set; get; }

        public string name { set; get; }

        public int maxID { set; get; }

        public string url { set; get; }

        public int count { set; get; }

    }
}
