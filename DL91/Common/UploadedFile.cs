using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91
{
    public class UploadedFile
    {
        public byte[] Data { set; get; }
        public string FileName { set; get; }

        public void CopyTo(Stream fs)
        {
            fs.Write(Data, 0, Data.Length);
        }
    }
}
