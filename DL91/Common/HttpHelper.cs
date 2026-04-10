using MihaZupan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DL91
{
    public class HttpHelper
    {
        private static HttpClient _httpClient;
        private static HttpClient HttpClient
        {
            get
            {
                if(_httpClient == null)
                {
                    _httpClient = new HttpClient(new SocketsHttpHandler()
                    {
                        UseCookies = false,
                        //Proxy = new HttpToSocks5Proxy("127.0.0.1", 1080),
                        ConnectTimeout = TimeSpan.FromSeconds(10),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(60),
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions()
                        {
                            RemoteCertificateValidationCallback = (sender, cer, chain, err) => true
                        }
                    });
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                }
                return _httpClient;
            }
        }

        public static async Task<bool> TestHttpSync(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await HttpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public static async Task<bool> TestImageUrlAsync(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await HttpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );

                if (!response.IsSuccessStatusCode)
                    return false;

                if (response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true)
                    return true;

                // 如果 HEAD 没返回 Content-Type，则尝试 GET
                using var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
                using var getResponse = await HttpClient.SendAsync(
                    getRequest,
                    HttpCompletionOption.ResponseHeadersRead
                );

                var contentType = getResponse.Content.Headers.ContentType?.MediaType;
                return contentType?.StartsWith("image/") == true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TestHttp(string url)
        {
            return Task.Run(() => TestHttpSync(url)).Result;
        }
        public static bool TestImageUrl(string url)
        {
            return Task.Run(() => TestImageUrlAsync(url)).Result;
        }
        public static async Task<HttpStatus> DownloadFileSync(string url, FileStream fs)
        {
            var result = new HttpStatus();

            try
            {
                url += (url.Contains("?") ? "&" : "?") + "_r=" + Guid.NewGuid();

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                // 禁止 gzip/deflate/br，避免写入压缩流
                request.Headers.AcceptEncoding.Clear();
                request.Headers.UserAgent.ParseAdd(
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0 Safari/537.36");
                using var response = await HttpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead);

                result.StatusCode = response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    result.IsGood = false;
                    return result;
                }

                var expectedLength = response.Content.Headers.ContentLength;

                using var stream = await response.Content.ReadAsStreamAsync();

                var buffer = new byte[81920];
                int read;

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fs.WriteAsync(buffer, 0, read);
                }

                await fs.FlushAsync();

                //// 校验长度（如果服务器提供）
                //if (expectedLength.HasValue && fs.Length != expectedLength.Value)
                //{
                //    result.IsGood = false;
                //    return result;
                //}

                result.Length = fs.Length;
                result.IsGood = true;
            }
            catch (Exception e)
            {
                result.IsGood = false;
                LogTool.Instance.Error("Failed to access " + url, e);
            }

            return result;
        }

        public static HttpStatus DownloadFile(string url, FileStream fs)
        {
            // 避免 .Result 死锁，用 GetAwaiter().GetResult() 更稳
            return DownloadFileSync(url, fs).GetAwaiter().GetResult();
        }

        public static async Task<HttpStatus> GetHtmlSync(string url)
        {
            var result = new HttpStatus();
            try
            {
                var response = await HttpClient.GetAsync(url);
                result.StatusCode = response.StatusCode;
                result.Html = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.IsGood = false;
                LogTool.Instance.Error("Failed to access " + url, e);
            }
            return result;
        }

        public static HttpStatus GetHtml(string url)
        {
            return Task.Run(() => GetHtmlSync(url)).Result;
        }

        public static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };
        }
    }

    public class HttpStatus
    { 
        public HttpStatusCode StatusCode { get; set; }
        public string Html { get; set; }

        public bool Is404 => (int)StatusCode == 404;

        public long Length { get; set; }
        public bool IsGood { set; get; } = true;
    }

    
}
