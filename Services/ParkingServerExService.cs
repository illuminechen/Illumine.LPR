using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Security.Policy;
using Illumine.LPR.Helpers;

namespace Illumine.LPR
{
    class ParkingServerExService
    {
        private static Dictionary<string, string> GetHeader(int id)
        {
            string epoch = TimeHelper.GetEpochTime().ToString();
            string uid = (string.IsNullOrWhiteSpace(Container.Get<LPRSetting>().ProjectName)) ? id.ToString() : Container.Get<LPRSetting>().ProjectName + "-" + id.ToString();
            string source = Container.Get<LPRSetting>().ParkingServerToken + ':' + Container.Get<LPRSetting>().ProjectName + "-" + id.ToString() + ':' + epoch;

            string hash = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
                byte[] hashBytes = sha1.ComputeHash(sourceBytes);
                hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            }

            return new Dictionary<string, string>
                    {
                        { "X-ASMS-ID",  uid},
                        { "X-ASMS-TIME", epoch},
                        { "X-ASMS-TOKEN", hash},
                    };
        }

        public static async void HeartBeat(string time, int channelId)
        {
            if (!Container.Get<LPRSetting>().UseParkingServerEx)
                return;
            try
            {
                var payload = new Dictionary<string, object>()
                {
                    {"type","state" },
                    {"time",time },
                };

                var res = await HttpClientHelper.PostMultiPartAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/event_m1",
                    GetHeader(channelId),
                    payload
                );
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }


        public static async Task<bool> CarPass(int channelId, string time, string plateNo, string imageFilePath)
        {
            if (!Container.Get<LPRSetting>().UseParkingServerEx)
                return false;
            try
            {
                LogHelper.Log("Enter(" + time + ")<" + plateNo + ">");

                //byte[] bytes = new byte[0];
                //try
                //{
                //    bytes = FileHelper.ReadFileBytes(imageFilePath);
                //}
                //catch (Exception ex)
                //{
                //    LogHelper.Log(ex.ToString());
                //}
                var payload = new Dictionary<string, object>()
                {
                    {"type", "pass" },
                    {"plate_no", plateNo },
                    {"time", time },
                    {"image", imageFilePath },
                    {"sense", 1 },
                };

                var res = await HttpClientHelper.PostMultiPartAsync(Container.Get<LPRSetting>().ParkingServerHostUrl + "/event_m1",
                    GetHeader(channelId),
                    payload
                );
                LogHelper.Log("Enter(" + time + ")<" + plateNo + ">:" + res);

                var json = JsonSerializer.Deserialize<result>(res);

                return json.pass;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }

            return false;
        }
        class result
        {
            public long time { get; set; }
            public int rest_spaces { get; set; }
            public int code { get; set; }
            public bool pass { get; set; }
            public string message { get; set; }
        }
    }
}
