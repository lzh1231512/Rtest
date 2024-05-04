﻿using DL91.WebProcess;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DL91.Jobs
{

    public class AutoProcessService : BackgroundService
    {
        public const string domain = "https://www.rm2029.com";
        public static int SyncFlag { set; get; } = 0;
        public static int DownloadVideoFlag { set; get; } = 0;
        public static bool EnableCacheProcess { set; get; } = false;


        public static Task AutoDownload()
        {
            return Task.Run(() =>
            {
                Console.WriteLine("start AutoDownload");
                HttpHelper.NEVER_EAT_POISON_Disable_CertificateValidation();
                CacheManager.ClearCache();
                while (true)
                {
                    try
                    {
                        if (SyncFlag <= 0)
                        {
                            if (ProcessHtml.DownloadNewData())
                            {
                                CacheManager.ClearCache();
                            }
                            ProcessImg.DownloadImg();
                            SyncFlag = 60 * 6;

                            Thread.Sleep(1000);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        SyncFlag--;
                        if (EnableCacheProcess)
                        {
                            CacheManager.ProcessCache();
                        }
                        if (DownloadVideoFlag <= 0)
                        {
                            ProcessVideo.DownloadVideo();
                            DownloadVideoFlag = 10;
                        }
                        DownloadVideoFlag--;

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    Thread.Sleep(60000);
                }
            });
        }

        public static Task AutoProcesCache()
        {
            return Task.Run(() =>
            {
                Console.WriteLine("start AutoProcesCache");
                while (true)
                {
                    Thread.Sleep(60000);
                    try
                    {
                        CacheManager.ProcessCache();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                }
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            await AutoProcesCache();
#else
            await AutoDownload();
#endif
        }
    }
}
