using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;

namespace Illumine.LPR
{
    class ParkingServerService
    {
        public static async void CarEnter(string plateNo, string time, string imageFilePath)
        {
            if (!Container.Get<LPRSetting>().UseParkingServer)
                return;
            try
            {
                LogHelper.Log("Enter(" + time + ")<" + plateNo + ">");
                var res = await HttpClientHelper.PostMultiPartAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/enter",
                    new Dictionary<string, string>
                    {
                    { "Authorization", Container.Get<LPRSetting>().ParkingServerToken},
                    },
                    new Dictionary<string, object>()
                    {
                    {"plate_no",plateNo },
                    {"time",time },
                    {"file",imageFilePath },
                    }
                );
                LogHelper.Log("Enter(" + time + ")<" + plateNo + ">:" + res);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }

        public static async Task<bool> CarCheckAsync(string plateNo)
        {
            if (!Container.Get<LPRSetting>().UseParkingServer)
                return true;
            try
            {
                bool pass = true;
                string res = await HttpClientHelper.PostFormUrlEncodedAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/check",
                    new Dictionary<string, string>
                    {
                    { "Authorization", Container.Get<LPRSetting>().ParkingServerToken},
                    },
                    new Dictionary<string, string>()
                    {
                    {"plate_no",plateNo },
                    }
                );
                LogHelper.Log("Check<" + plateNo + ">:" + res);
                var json = JsonSerializer.Deserialize<result>(res);
                pass = (json.pass == 1);
                return pass;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
                return true;
            }
        }

        class result
        {
            public int code { get; set; }
            public int pass { get; set; }
            public string message { get; set; }
        }

        public static async Task<bool> CarLeave(string plateNo, string time, string imageFilePath)
        {
            if (!Container.Get<LPRSetting>().UseParkingServer)
                return true;
            try
            {
                var res = await HttpClientHelper.PostMultiPartAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/leave",
                    new Dictionary<string, string>
                    {
                    { "Authorization", Container.Get<LPRSetting>().ParkingServerToken},
                    },
                    new Dictionary<string, object>()
                    {
                    {"plate_no",plateNo },
                    {"time",time },
                    {"file",imageFilePath },
                    }
                );
                LogHelper.Log("Leave(" + time + ")<" + plateNo + ">:" + res);
                var json = JsonSerializer.Deserialize<result>(res);

                return json.message == "ok";
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
                return true;
            }
        }

        public static void SpaceCount(int count)
        {
            if (!Container.Get<LPRSetting>().UseParkingServer)
                return;
            try
            {
                HttpClientHelper.PostFormUrlEncodedAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/count",
                    new Dictionary<string, string>
                    {
                    { "Authorization", Container.Get<LPRSetting>().ParkingServerToken},
                    },
                    new Dictionary<string, string>()
                    {
                    {"amount",count.ToString() },
                    }
                ).ContinueWith(x => LogHelper.Log("Count<" + count + ">:" + x.Result));
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }
    }
}
