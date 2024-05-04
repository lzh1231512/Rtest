using DL91.Jobs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddResponseCompression(options =>
{
    //options.EnableForHttps = true;
    // 添加br与gzip的Provider
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    // 扩展一些类型 (MimeTypes中有一些基本的类型,可以打断点看看)
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/html; charset=utf-8",
        "application/xhtml+xml",
        "application/atom+xml",
        "image/Jpeg",
        "application/x-mpegURL",
        "image/svg+xml"
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<AutoProcessService>();

var app = builder.Build();
app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/x-mpegURL"; //m3u8的MIME
provider.Mappings[".ts"] = "video/MP2TL"; //.ts的MIME
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
