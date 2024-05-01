#if DEBUG
using Microsoft.AspNetCore.StaticFiles;

Task.Factory.StartNew(() =>
{
    DL91.Job.Test(args);
});
#else
Task.Factory.StartNew(() =>
{
    DL91.Job.Main(args);
});
#endif

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/x-mpegURL"; //m3u8µÄMIME
provider.Mappings[".ts"] = "video/MP2TL"; //.tsµÄMIME
app.UseCors(
    options => options.WithOrigins("*").AllowAnyMethod()
);

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "m3u8fix",
    pattern: "Home/m3u8fix/{isHD}/{id}/index.m3u8", 
    defaults: new { Controller = "Home", Action = "m3u8fix" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "swcache",
    pattern: "sw-cache.js",
    defaults: new { Controller = "Home", Action = "SwCache" });

app.Run();
