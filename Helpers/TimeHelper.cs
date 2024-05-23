using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class TimeHelper
    {
        public static long GetEpochTime()
        {
            // 获取当前时间
            DateTime now = DateTime.UtcNow;

            // Unix epoch starts on 1970-01-01 00:00:00
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 计算当前时间与Unix epoch之间的差值
            TimeSpan timestamp = now - epochStart;

            // 获取总秒数
            long secondsSinceEpoch = (long)timestamp.TotalSeconds;

            return secondsSinceEpoch;
        }

    }
}
