using DL91;

namespace DL91Web8.Helpers
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies["key"] != ConfigurationHelper.LoginKey &&
                context.Request.Path.Value.ToLower() != "/home/login" &&
                !context.Request.Path.Value.ToLower().StartsWith("/home/m3u8fix"))
            {
                context.Response.Redirect("/Home/Login");
                return;
            }
            await _next(context);
        }
    }
}
