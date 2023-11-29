﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
[assembly: AssemblyVersion("1.0.*")]
namespace DL91Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
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
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("host.json", optional: true)
              .Build();

            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(config);
            return host;
        }
    }
}
