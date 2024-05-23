using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    class HttpClientHelper
    {

        static public async Task<string> PostMultiPartAsync(string url, Dictionary<string, string> headers, Dictionary<string, object> payload)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false }))
            {
                MultipartFormDataContent content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

                foreach (var kv in headers)
                {
                    if (kv.Key == "Authorization")
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", kv.Value);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }
                }

                foreach (var kv in payload)
                {
                    if (kv.Key == "file" || kv.Key == "image")
                    {
                        byte[] bs = File.ReadAllBytes(kv.Value.ToString());
                        var fileContent = new ByteArrayContent(bs);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                        content.Add(fileContent, kv.Key, kv.Key);
                    }
                    //if (kv.Value is string str)
                    else
                    {
                        content.Add(new StringContent(kv.Value.ToString()), kv.Key);
                    }
                    //else if (kv.Value is Image img)
                    //{
                    //    using (var ms = new MemoryStream())
                    //    {
                    //        img.Save(ms, img.RawFormat);
                    //        ms.Seek(0, SeekOrigin.Begin);
                    //        var stream = new StreamContent(ms);
                    //        stream.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                    //        content.Add(stream, kv.Key, kv.Key);
                    //    }
                    //}
                }

                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                var res = await client.SendAsync(req);
                return await res.Content.ReadAsStringAsync();
            }
        }


        static public async Task<string> PostFormUrlEncodedAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> payload)
        {
            //建立 HttpClient
            HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false });

            HttpContent content = new FormUrlEncodedContent(payload);
            foreach (var kv in headers)
            {
                if (kv.Key == "Authorization")
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", kv.Value);
                }
                else
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
            }

            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var res = await client.SendAsync(req);

            return await res.Content.ReadAsStringAsync();
        }

        static public async Task<string> GetFormUrlEncodedAsync(string url, Dictionary<string, string> headers)
        {
            //建立 HttpClient
            HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false });

            foreach (var kv in headers)
            {
                if (kv.Key == "Authorization")
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", kv.Value);
                }
                else
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
            }

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            var res = await client.SendAsync(req);

            return await res.Content.ReadAsStringAsync();
        }
    }
}
