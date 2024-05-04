
namespace GetWebInfo
{
    /// <summary>
    /// 链接类
    /// </summary>
    public class Link
    {
        public string url;   //链接网址
        public string text;  //链接文字
        public Link(string _url, string _text)
        {
            url = _url;
            text = _text;
        }
    }
}
