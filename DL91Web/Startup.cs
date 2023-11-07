using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DL91Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            //------以下添加MIME---------
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".m3u8"] = "application/x-mpegURL"; //m3u8的MIME
            provider.Mappings[".ts"] = "video/MP2TL"; //.ts的MIME

            app.Use(next =>
            {
                return new RequestDelegate(async (httpContext) => {
                    var url = httpContext.Request.Path.Value;
                    if (!string.IsNullOrEmpty(url) && url.EndsWith(".ts"))
                    {
                        return;
                    }
                    using (var memoryStream = new MemoryStream())
                    {
                        var responseStream = httpContext.Response.Body;
                        httpContext.Response.Body = memoryStream;

                        await next(httpContext);

                        using (var compressedStream = new GZipStream(responseStream, CompressionLevel.Optimal))
                        {
                            httpContext.Response.Headers.Remove("Content-Length");
                            httpContext.Response.Headers.Add("Content-Encoding", new[] { "gzip" });
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            await memoryStream.CopyToAsync(compressedStream);
                        }
                    }
                });
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            //------以上添加MIME---------
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
