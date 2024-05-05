using DL91.BLL;

namespace DL91Web8.Helpers
{
    public class Content : IContent
    {
        private readonly IWebHostEnvironment _env;
        public Content(IWebHostEnvironment env)
        {
            _env = env;
        }
        public string ContentPath => _env.WebRootPath;
    }
}
