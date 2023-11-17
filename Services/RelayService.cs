using System;
using System.Collections;
using System.Linq;

namespace Illumine.LPR
{
    public static class RelayService
    {
        public static void OpenRelay(string comport, bool[] trigger, int time = 3)
        {
            if (string.IsNullOrWhiteSpace(comport))
                return;
            bool[] flagArray = new bool[16];
            int length = Math.Min(16, trigger.Length);
            Array.Copy(trigger, flagArray, length);
            BitArray bitArray = new BitArray(flagArray);
            byte[] numArray1 = new byte[2];
            byte[] numArray2 = numArray1;
            bitArray.CopyTo(numArray2, 0);
            byte[] numArray3 = new byte[11];
            numArray3[0] = 2;
            numArray3[1] = 36;
            numArray3[2] = numArray1[0];
            numArray3[3] = numArray1[1];
            numArray3[8] = (byte)time;
            numArray3[10] = numArray3.Cast<byte>().ToList().GetRange(0, numArray3.Length - 1).Aggregate((check, x) => check ^= x);
            SerialPortHelper.Send(comport, numArray3);
        }

        public static void OpenRelay(string comport, int id = 1, int time = 3)
        {
            BitArray bitArray = new BitArray(16);
            if (id < 1 || id > 16)
                return;
            bitArray[id - 1] = true;
            byte[] numArray1 = new byte[2];
            bitArray.CopyTo(numArray1, 0);
            byte[] numArray2 = new byte[11];
            numArray2[0] = 2;
            numArray2[1] = 36;
            numArray2[2] = numArray1[0];
            numArray2[3] = numArray1[1];
            numArray2[8] = (byte)time;
            numArray2[10] = numArray2.Cast<byte>().ToList().GetRange(0, numArray2.Length - 1).Aggregate((check, x) => check ^= x);
            SerialPortHelper.Send(comport, numArray2);
        }
    }
}
