using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using static Illumine.LPR.HttpServerHelper;

namespace Illumine.LPR
{
    public class JotangiServerService
    {
        MicroHttpServer microHttpServer;

        string _ip;
        int _port;

        public JotangiServerService(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Start()
        {
            microHttpServer = new MicroHttpServer(IPAddress.Parse(_ip), _port,
                (req) =>
                {
                    try
                    {
                        //LogHelper.Log("URL:" + req.Url);
                        //LogHelper.Log(string.Join(",", req.Headers.Select(kv => kv.Key + ":" + kv.Value)));
                        //LogHelper.Log(string.Join(",", req.Payloads.Select(kv => kv.Key + ":" + kv.Value)));
                        if (req.Payloads.Count > 0)
                            LogHelper.Log(string.Join(",", req.Payloads.Select(kv => "{" + kv.Key + ":" + kv.Value + "}")));
                        else
                            LogHelper.Log("No Payloads");

                        if (req.Headers.ContainsKey("Authorization") && req.Headers["Authorization"].StartsWith("Bearer "))
                        {
                            if (req.Headers["Authorization"].Split(' ')[1] != Container.Get<LPRSetting>().JToken && Container.Get<LPRSetting>().JToken != "")
                            {
                                return new CompactResponse()
                                {
                                    StatusText = CompactResponse.HttpStatus.Http403
                                };
                            }
                        }                        

                        if (req.Url == "/setspace")
                        {
                            if (req.Method == "POST" && req.Payloads.ContainsKey("value") && int.TryParse(req.Payloads["value"], out var v))
                            {
                                return SetSpace(v);
                            }

                            return new CompactResponse()
                            {
                                StatusText = CompactResponse.HttpStatus.Http500
                            };
                        }
                        else if (req.Url == "/opendoor")
                        {
                            if (req.Method == "POST" && req.Payloads.ContainsKey("id") && int.TryParse(req.Payloads["id"], out var id))
                            {
                                return OpenDoor(id);
                            }

                            return new CompactResponse()
                            {
                                StatusText = CompactResponse.HttpStatus.Http500
                            };
                        }
                        else if (req.Url == "/getspace")
                        {
                            if (req.Method == "GET")
                            {
                                return GetSpace();
                            }
                            return new CompactResponse()
                            {
                                StatusText = CompactResponse.HttpStatus.Http500
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("Error:" + ex.Message);
                    }
                    return new CompactResponse()
                    {
                        StatusText = CompactResponse.HttpStatus.Http404
                    };

                });
        }

        private CompactResponse GetSpace()
        {
            string res = "";
            try
            {
                var space = SpaceService.GetSpaceCount();

                res = "{\"result\":true,\"value\":" + space + "}";
            }
            catch (Exception ex)
            {
                res = "{\"result\":false,\"msg\":\"" + ex.Message + "\"}";
            }
            return new CompactResponse()
            {
                ContentType = "application/json",
                Data = Encoding.UTF8.GetBytes(res),
                StatusText = CompactResponse.HttpStatus.Http200
            };
        }

        private CompactResponse SetSpace(int count)
        {
            string res = "";
            try
            {
                SpaceService.SetSpaceCount(count);

                // 入口顯示剩餘車位
                foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                {
                    if (channelDataModel.Enabled)
                    {
                        if (channelDataModel.EntryMode == EntryMode.In)
                        {
                            if (channelDataModel.Led2Ip != "")
                            {
                                foreach (var ip in channelDataModel.Led2Ip.Split(';'))
                                {
                                    if (string.IsNullOrWhiteSpace(ip))
                                        continue;

                                    LEDService.SendNumber(ip, channelDataModel.Led2Port, count);
                                }
                            }
                        }
                    }
                }

                res = "{\"result\":true}";
            }
            catch (Exception ex)
            {
                res = "{\"result\":false,\"msg\":\"" + ex.Message + "\"}";
            }
            return new CompactResponse()
            {
                ContentType = "application/json",
                Data = Encoding.UTF8.GetBytes(res),
                StatusText = CompactResponse.HttpStatus.Http200
            };
        }

        private CompactResponse OpenDoor(int channelId)
        {
            string res = "";
            try
            {
                ChannelViewerViewModel channelViewerViewModel = Container.Get<ChannelViewerViewModel>(channelId);
                if (channelViewerViewModel == null)
                {
                    throw new Exception("can not find object with id:" + channelId);
                }
                channelViewerViewModel.CameraViewModel.OpenDoor();
                res = "{\"result\":true}";
            }
            catch (Exception ex)
            {
                res = "{\"result\":false,\"msg\":\"" + ex.Message + "\"}";
            }
            return new CompactResponse()
            {
                ContentType = "application/json",
                Data = Encoding.UTF8.GetBytes(res),
                StatusText = CompactResponse.HttpStatus.Http200
            };
        }

        public void Stop()
        {
            microHttpServer.Stop();
        }
    }
}
