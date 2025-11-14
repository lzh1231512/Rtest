using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
                        ConnectTimeout = TimeSpan.FromSeconds(10),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(60),
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions()
                        {
                            RemoteCertificateValidationCallback = (sender, cer, chain, err) => true
                        }
                    });
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)");
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

        public static bool TestHttp(string url)
        {
            return Task.Run(() => TestHttpSync(url)).Result;
        }

        public static async Task<HttpStatus> DownloadFileSync(string url,FileStream fs)
        {
            var result = new HttpStatus();
            try
            {
                var response = await HttpClient.GetAsync(url);
                result.StatusCode = response.StatusCode;
                await response.Content.CopyToAsync(fs);
                result.Length = fs.Length;
            }
            catch(Exception e)
            {
                result.IsGood = false;
                LogTool.Instance.Error("Failed to access " + url, e);
            }
            return result;
        }

        public static HttpStatus DownloadFile(string url, FileStream fs)
        {
            return Task.Run(() => DownloadFileSync(url, fs)).Result;
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
