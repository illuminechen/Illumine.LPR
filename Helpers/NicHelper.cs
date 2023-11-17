using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Illumine.LPR
{
    public static class NicHelper
    {
        private const int MIN_MAC_ADDR_LENGTH = 12;
        private const long maxspeed = -1;

        public static string GetMac()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = networkInterface.GetPhysicalAddress().ToString().ToUpper();
                if (!networkInterface.Description.Contains("Virtual") && networkInterface.Speed > -1L && !string.IsNullOrEmpty(tempMac) && tempMac.Length == 12)
                    return string.Join("-", Enumerable.Range(0, 6).Select<int, string>((Func<int, string>)(i => tempMac.Substring(i * 2, 2))));
            }
            return "";
        }

        public static uint IpToInt(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return uint.Parse(items[0]) << 24
                    | uint.Parse(items[1]) << 16
                    | uint.Parse(items[2]) << 8
                    | uint.Parse(items[3]);
        }

        public static string IntToIp(uint ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }

        public static uint Reverse_uint(uint uiNum)
        {
            return ((uiNum & 0x000000FF) << 24) |
                   ((uiNum & 0x0000FF00) << 8) |
                   ((uiNum & 0x00FF0000) >> 8) |
                   ((uiNum & 0xFF000000) >> 24);
        }
    }
}
