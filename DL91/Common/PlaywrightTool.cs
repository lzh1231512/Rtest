using Microsoft.Playwright;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using DL91;
using System.Collections.Generic;

public class PlaywrightTool
{
    private readonly string _htmlRoot;
    private static IPlaywright _playwright;
    private static IBrowser _browser;
    private static Timer _disposeTimer;
    private static readonly object _lock = new();
    private static bool _initialized = false;
    private const int DisposeTimeoutMs = 5000;
    private static IPage _page;
    private static List<string> keys= new List<string>();

    public PlaywrightTool()
    {
        // 获取程序当前路径下的 html 文件夹
        _htmlRoot = Path.Combine(AppContext.BaseDirectory, "HTML");
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
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
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
        keys.Clear();
        lock (_lock)
        {
            _disposeTimer?.Dispose();
            _disposeTimer = null;
        }
        Console.WriteLine("Playwright resources disposed.");
    }

    public async Task<string> ProcessAsync(string id)
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
                await setEvent(id, tcs);
                var indexPath = Path.Combine(_htmlRoot, "index.html");
                await _page.GotoAsync($"file:///{indexPath.Replace("\\", "/")}#/video/" + id);
            }
            else
            {
                await setEvent(id, tcs);
                string vueRoute = "/video/" + id;
                await _page.EvaluateAsync($"window.location.hash = '{vueRoute}';");
                // 这里需要重新等待加密结果
            }
            var result = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (tcs.Task.IsCompleted)
            {
                LogTool.Instance.Info($"{id} 加密后的URL: {tcs.Task.Result}");
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

    private async Task setEvent(string id, TaskCompletionSource<string> tcs)
    {
        await _page.UnrouteAsync("**/*");
        await _page.RouteAsync("**/*", async route =>
        {
            var url = route.Request.Url;
            try
            {
                if (url.EndsWith("main.js"))
                {
                    var jsPath = Path.Combine(_htmlRoot, "main.js");
                    var jsContent = await File.ReadAllTextAsync(jsPath);
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/javascript",
                        Body = jsContent
                    });
                    return;
                }
                if (url.Contains("encrypt"))
                {
                    var jsonPath = Path.Combine(_htmlRoot, "encrypt.json");
                    var jsonContent = await File.ReadAllTextAsync(jsonPath);
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = jsonContent
                    });
                    return;
                }
                if (url.Contains("api/videos/index_byall"))
                {
                    var jsonPath = Path.Combine(_htmlRoot, "main.json");
                    var jsonContent = await File.ReadAllTextAsync(jsonPath);
                    jsonContent = jsonContent.Replace("{id}", id);
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = jsonContent
                    });
                    return;
                }
                if (url.Contains("detail"))
                {
                    var jsonPath = Path.Combine(_htmlRoot, "detail.json");
                    var jsonContent = await File.ReadAllTextAsync(jsonPath);
                    jsonContent = jsonContent.Replace("{id}", id);
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = jsonContent
                    });
                    return;
                }
                if (url.Contains("related"))
                {
                    var jsonPath = Path.Combine(_htmlRoot, "detail.json");
                    var jsonContent = await File.ReadAllTextAsync(jsonPath);
                    jsonContent = jsonContent.Replace("{id}", id);
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = jsonContent
                    });
                    return;
                }
                if (url.Contains("api/videos/img/"))
                {
                    var parts = url.Split(new[] { "/img/" }, StringSplitOptions.None);
                    if (parts.Length > 1 && !keys.Contains(parts[1]))
                    {
                        keys.Add(parts[1]);
                        var encryptedUrl = parts[1];
                        tcs.TrySetResult(encryptedUrl);
                    }
                    await route.ContinueAsync();
                    return;
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