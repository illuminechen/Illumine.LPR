using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Linq;

namespace Illumine.LPR
{
    public class HttpServerHelper
    {
        //Reuquest物件
        public class CompactRequest
        {
            public enum ContentType { urlencoded, multipart, json }
            public enum ClientType { iPhone, iPad, iPod, Android, AndroidPad, Other }
            public string Method, Url, Protocol, RemoteIp;
            public string PureUrl { get => Url.Split('?')[0]; }
            public string Extesion { get => PureUrl.Split('.').Last(); }
            public string IpString { get => RemoteIp.Split(':')[0]; }
            public string Port { get => RemoteIp.Split(':')[1]; }
            public string Token
            {
                get
                {
                    string token = "";
                    if (Headers.ContainsKey("cookie") && Headers["cookie"] != "")
                    {
                        string[] cs = Headers["cookie"].Split(';');
                        foreach (var s in cs)
                        {
                            var css = s.Split('=');
                            if (css.Length <= 1)
                            {

                            }
                            else if (css[0].Trim() == "Token")
                            {
                                token = s.Split('=')[1];
                                break;
                            }
                        }
                    }
                    return token;
                }
            }
            public ClientType clientType
            {
                get
                {
                    String s1 = Headers["user-agent"];
                    if (s1.Contains("Android") && s1.Contains("Mobile"))
                    {
                        return ClientType.Android;
                    }
                    else if (s1.Contains("Android"))
                    {
                        return ClientType.AndroidPad;
                    }
                    else if (s1.Contains("iPhone"))
                    {
                        return ClientType.iPhone;
                    }
                    else if (s1.Contains("iPad"))
                    {
                        return ClientType.iPad;
                    }
                    else if (s1.Contains("iPod"))
                    {
                        return ClientType.iPod;
                    }
                    else
                    {
                        return ClientType.Other;
                    }

                }
            }

            public string QueryString { get => (Url.LastIndexOf('?') >= 0) ? Url.Substring(Url.LastIndexOf('?') + 1) : ""; }
            public Dictionary<string, byte[]> multipartFiles;
            public Dictionary<string, string> Headers;
            public Dictionary<string, string> Payloads;
            public ContentType contentType;

            //傳入StreamReader，讀取Request傳入的內容
            public CompactRequest(StreamReader sr, string ip)
            {
                try
                {
                    //string[] lines = sr.ReadToEnd().Replace("\r\n","\n").Split('\n');            
                    //第一列格式如: GET /index.html HTTP/1.1
                    string firstLine = sr.ReadLine();
                    string[] p = firstLine.Split(' ');
                    Method = p[0];
                    Url = (p.Length > 1) ? p[1] : "NA";
                    Protocol = (p.Length > 2) ? p[2] : "NA";
                    RemoteIp = ip;
                    //讀取其他Header，格式為HeaderName: HeaderValue            
                    Headers = new Dictionary<string, string>();
                    Payloads = new Dictionary<string, string>();
                    string line = null;
                    //for(int i = 1;i<lines.Length;i++)
                    while (!String.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        //line = lines[i];
                        int pos = line.IndexOf(":");
                        if (pos > -1)
                            Headers.Add(line.Substring(0, pos).ToLower(),
                                line.Substring(pos + 1));
                    }

                    if (Headers.ContainsKey("content-length"))
                    {
                        int ContentLength = 0;
                        if (int.TryParse(Headers["content-length"], out ContentLength) && ContentLength > 0)
                        {
                            byte[] byteAry = new byte[ContentLength];
                            Encoding encoding = Encoding.Default;
                            char[] c = new char[ContentLength];
                            sr.Read(c, 0, ContentLength);
                            //Console.WriteLine(c);
                            string contents = new string(c);
                            string type = Headers["content-type"];
                            if (type.Contains("application/x-www-form-urlencoded"))
                            {
                                contentType = ContentType.urlencoded;
                                string[] cs = contents.Split('&');
                                foreach (string content in cs)
                                {
                                    Payloads.Add(System.Web.HttpUtility.UrlDecode(content.Split('=')[0]), System.Web.HttpUtility.UrlDecode(content.Split('=')[1]));
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            private string findstr(int cursor, string total, string fromStr, string toStr)
            {
                if (total.IndexOf(fromStr) == -1)
                    return "";
                int pFrom = total.IndexOf(fromStr) + fromStr.Length;
                if (total.IndexOf(toStr) == -1)
                    return "";
                int pTo = total.IndexOf(toStr, pFrom + 1);
                string result = total.Substring(pFrom, pTo - pFrom);
                return result;
            }

        }
        //Response物件
        public class CompactResponse
        {
            public static CompactResponse Error = new CompactResponse
            {
                StatusText = HttpStatus.Http500,
            };
            public static CompactResponse OK = new CompactResponse
            {
                StatusText = HttpStatus.Http200,
            };
            public static CompactResponse PartialContent = new CompactResponse
            {
                StatusText = HttpStatus.Http206,
            };
            public static CompactResponse NotModified = new CompactResponse
            {
                StatusText = HttpStatus.Http304,
            };
            public static CompactResponse BadRequest = new CompactResponse
            {
                StatusText = HttpStatus.Http400,
            };
            public static CompactResponse NotFound = new CompactResponse
            {
                StatusText = HttpStatus.Http404,
            };
            public class HttpStatus
            {
                public static string Http200 = "200 OK";
                public static string Http206 = "206 Partial Content";
                public static string Http304 = "304 Not Modified";
                public static string Http400 = "400 Bad Request";
                public static string Http403 = "403 Forbidden";
                public static string Http404 = "404 Not Found";
                public static string Http500 = "500 Error";
            }

            public string StatusText = HttpStatus.Http200;
            public string ContentType = "text/plain";
            //可回傳Response Header
            public Dictionary<string, string> Headers
                = new Dictionary<string, string>();
            //傳回內容，以byte[]表示
            public byte[] Data = new byte[] { };
            public string Name = "";
        }

        //簡陋但堪用的HTTP Server
        public class MicroHttpServer
        {

            private Thread serverThread;
            TcpListener listener;
            //呼叫端要準備一個函數，接收CompactRequest，回傳CompactResponse
            public MicroHttpServer(/*string pfxpath,*/IPAddress ipAddr, int port,
                Func<CompactRequest, CompactResponse> reqProc)
            {
                listener = new TcpListener(ipAddr, port);
                //另建Thread執行
                serverThread = new Thread(() =>
                {
                    listener.Start();
                    while (true)
                    {
                        Socket s = null;
                        try
                        {
                            s = listener.AcceptSocket();
                            string ip = s.RemoteEndPoint.ToString().Split(':')[0];

                            if (!Container.Get<LPRSetting>().WhiteList.Split(';').Contains(ip))
                            {
                                LogHelper.Log("Unauthorized IP:" + ip);
                                s.Shutdown(SocketShutdown.Both);
                                continue;
                            }
                            LogHelper.Log("Remote Connected:" + ip);

                            NetworkStream ns = new NetworkStream(s);
                            //解讀Request內容
                            //SslStream sslStream = new SslStream(ns, false);
                            //X509Certificate serverCertificate = new X509Certificate2(pfxpath, "PASSWORD", X509KeyStorageFlags.MachineKeySet);

                            //sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, enabledSslProtocols: System.Security.Authentication.SslProtocols.Default, checkCertificateRevocation: true);
                            //sslStream.ReadTimeout = 5000;
                            //sslStream.WriteTimeout = 5000;

                            StreamReader sr = new StreamReader(ns);

                            CompactRequest req = new CompactRequest(sr, s.RemoteEndPoint.ToString());
                            //呼叫自訂的處理邏輯，得到要回傳的Response
                            CompactResponse resp = reqProc(req);
                            //傳回Response
                            StreamWriter sw = new StreamWriter(ns);
                            sw.WriteLine("HTTP/1.1 {0}", resp.StatusText);
                            sw.WriteLine("Content-Type: " + resp.ContentType);
                            foreach (string k in resp.Headers.Keys)
                                sw.WriteLine("{0}: {1}", k, resp.Headers[k]);
                            sw.WriteLine("Content-Length: {0}", resp.Data.Length);
                            sw.WriteLine();
                            sw.Flush();
                            //寫入資料本體
                            s.Send(resp.Data);
                            //結束連線
                            s.Shutdown(SocketShutdown.Both);
                            ns.Close();
                        }
                        catch (AuthenticationException e)
                        {
                            LogHelper.Log("Exception: {0}", e.Message);
                            if (e.InnerException != null)
                            {
                                LogHelper.Log("Inner exception: {0}", e.InnerException.Message);
                            }
                            LogHelper.Log("Authentication failed - closing the connection.");
                            //sslStream.Close();
                            s?.Close();
                            return;
                        }
                        catch (Exception e)
                        {
                            LogHelper.Log("Exception: {0}", e.Message);
                            if (e.InnerException != null)
                            {
                                LogHelper.Log("Inner exception: {0}", e.InnerException.Message);
                            }
                            LogHelper.Log("Somthing failed - closing the connection.");
                            //sslStream.Close();
                            s?.Close();
                            return;
                        }
                    }
                });
                serverThread.Start();
            }
            public void Stop()
            {
                listener.Stop();
                serverThread.Abort();
            }
        }
    }
}
