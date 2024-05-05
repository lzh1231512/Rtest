using DL91;
using DL91.Jobs;
using log4net.Config;
using log4net.Repository;
using log4net;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using DL91Web8.Helpers;
using DL91.BLL;

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
    // ���br��gzip��Provider
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    // ��չһЩ���� (MimeTypes����һЩ����������,���Դ�ϵ㿴��)
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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
LogTool.Init();
builder.Services.AddSingleton<ILogTool, LogTool>();//��Ϊ��־����Ҫ������������������ע���ʱ�������ڿ���ʹ�õ���
builder.Services.AddSingleton<IContent, Content>();
builder.Services.AddSingleton<IWebBLL, WebBLL>();

var app = builder.Build();
app.UseResponseCompression();
app.UseExceptionHandler();
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/x-mpegURL"; //m3u8��MIME
provider.Mappings[".ts"] = "video/MP2TL"; //.ts��MIME
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
