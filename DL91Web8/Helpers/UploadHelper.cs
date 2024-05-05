using DL91;
namespace DL91Web8.Helpers
{
    public static class UploadHelper
    {
        public static UploadedFile ToUploadedFile(this IFormFile file)
        {
            using var ms=new MemoryStream();
            file.CopyTo(ms);
            return new UploadedFile()
            {
                FileName = file.FileName,
                Data = ms.ToArray()
            };
        }
    }
}
