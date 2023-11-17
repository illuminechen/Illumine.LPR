using System;
using System.IO;
using System.Text;

namespace Illumine.LPR
{
    public class LogHelper
    {
        private const string LogFileName = "./log.txt";

        public static bool CanLog => File.Exists("./EnabledLog.flg");

        public static void Log(string tag, string line) => LogHelper.Log("[" + tag + "]" + line);

        public static void Log(Exception ex) => LogHelper.Log(ex.ToString());

        public static void Log(string tag, Exception ex) => LogHelper.Log("[" + tag + "]" + ex.ToString());

        public static void Log(string line)
        {
            try
            {
                LogHelper.WriteLine(line);
            }
            catch
            {
            }
        }

        private static void WriteLine(string line)
        {
            if (!LogHelper.CanLog)
                return;
            using (StreamWriter streamWriter = new StreamWriter("./log.txt", true, Container.Get<Encoding>()))
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + line);
        }
    }
}
