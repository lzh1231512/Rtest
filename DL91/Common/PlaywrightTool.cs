using Microsoft.Playwright;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using DL91;

public class PlaywrightTool
{
    private static IPlaywright _playwright;
    private static IBrowser _browser;
    private static Timer _disposeTimer;
    private static readonly object _lock = new();
    private static bool _initialized = false;
    private const int DisposeTimeoutMs = 5000;
    private static IPage _page;

    public PlaywrightTool()
    {
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { 
            Headless = true ,
            Channel = "chrome", // 或者 "msedge" 根据需要选择浏览器
            Args = new[] { $"--proxy-server=socks5://127.0.0.1:1080" } // 设置 SOCKS5 代理服务器地址和端口
        });
    }

    private void ResetDisposeTimer()
    {
        lock (_lock)
        {
            _disposeTimer?.Dispose();
            _disposeTimer = new Timer(async _ => await DisposePlaywrightAsync(), null, DisposeTimeoutMs, Timeout.Infinite);
        }
    }

    private async Task DisposePlaywrightAsync()
    {
        lock (_lock)
        {
            if (!_initialized) return;
            _initialized = false;
        }
        if (_page != null && !_page.IsClosed)
        {
            await _page.CloseAsync();
            _page = null;
        }
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
        lock (_lock)
        {
            _disposeTimer?.Dispose();
            _disposeTimer = null;
        }
        Console.WriteLine("Playwright resources disposed.");
    }

    public async Task<string> ProcessAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            ResetDisposeTimer();

            var tcs = new TaskCompletionSource<string>();

            if (_page == null || _page.IsClosed)
            {
                _page = await _browser.NewPageAsync();
                // 拦截所有请求
                await setEvent(tcs);
                await _page.GotoAsync($"https://www.91rb.com/videos/271486/s-s-2/",new PageGotoOptions()
                {
                    WaitUntil= WaitUntilState.DOMContentLoaded
                });
            }
            var result = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (tcs.Task.IsCompleted)
            {
                return tcs.Task.Result;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private async Task setEvent(TaskCompletionSource<string> tcs)
    {
        await _page.RouteAsync("**/*", async route =>
        {
            var url = route.Request.Url;
            try
            {
                Console.WriteLine($"{url}");
                if ((url.Contains(".ts")|| url.Contains(".m3u8"))&&url.Contains("/hls/"))
                {
                    var index = url.IndexOf("/hls/");

                    tcs.TrySetResult(url.Substring(0,index));
                }
                await route.ContinueAsync();
            }
            catch (Exception ex)
            {
                LogTool.Instance.Error($"Route handling error: {ex}");
                await route.ContinueAsync();
            }
        });
    }
}