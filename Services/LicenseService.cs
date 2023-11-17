using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Illumine.LPR
{
    public static class LicenseService
    {
        public static bool CheckCamera(string secret, string mac)
        {
            string key = "JULIO";
            return Check(mac, secret, key);
        }

        public static (bool success, string messages) CheckLicenseFile(string filename)
        {
            if (!File.Exists(filename))
                return (false, "找不到License.dat檔案");
            string str1;
            string str2;
            string str3;
            string s;
            string str4;
            string str5;
            string str6;
            string code;
            using (StreamReader streamReader = new StreamReader(filename, Container.Get<Encoding>()))
            {
                str1 = streamReader.ReadLine();
                str2 = streamReader.ReadLine();
                str3 = streamReader.ReadLine();
                s = streamReader.ReadLine();
                str4 = streamReader.ReadLine();
                str5 = streamReader.ReadLine();
                str6 = streamReader.ReadLine();
                code = streamReader.ReadLine();
            }
            if (str2 != NicHelper.GetMac())
                return (false, "您的License檔案格式錯誤");
            if (!LicenseService.Check(str3, str6, "illumine.LPR"))
                return (false, "您的License檔案格式錯誤");
            if (!(str3 == "1"))
                return (false, "意外錯誤");
            if (!LicenseService.Check(str1, str4, str5))
                return (false, "您的License檔案格式錯誤");
            if (!LicenseService.Check(str2, str5, str6))
                return (false, "您的License檔案格式錯誤");
            if (!LicenseService.Check(s, code, str4))
                return (false, "您的License檔案格式錯誤");
            return DateTime.Parse(s) < DateTime.Now ? (false, "您的License已過期") : (true, "");
        }

        private static bool Check(string value, string code, string key)
        {
            UTF8Encoding utF8Encoding = new UTF8Encoding();
            byte[] bytes1 = utF8Encoding.GetBytes(key);
            byte[] bytes2 = utF8Encoding.GetBytes(value);
            using (HMACSHA512 hmacshA512 = new HMACSHA512(bytes1))
            {
                byte[] hash = hmacshA512.ComputeHash(bytes2);
                return code.ToUpper() == BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public static string GenerateCode(string value, string key)
        {
            UTF8Encoding utF8Encoding = new UTF8Encoding();
            byte[] bytes1 = utF8Encoding.GetBytes(key);
            byte[] bytes2 = utF8Encoding.GetBytes(value);
            using (HMACSHA512 hmacshA512 = new HMACSHA512(bytes1))
            {
                byte[] hash = hmacshA512.ComputeHash(bytes2);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }
    }
}
