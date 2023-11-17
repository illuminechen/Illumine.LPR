using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class SerialPortHelper
    {
        public static string GetPortName(string keyword)
        {
            if (keyword.StartsWith("COM"))
                return keyword;
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
                return ((IEnumerable<string>)SerialPort.GetPortNames()).Join((IEnumerable<ManagementBaseObject>)managementObjectSearcher.Get().Cast<ManagementBaseObject>().ToList<ManagementBaseObject>(), (Func<string, string>)(n => n), (Func<ManagementBaseObject, string>)(p => p["DeviceID"].ToString()), (n, p) => new
                {
                    n = n,
                    p = p
                }).Where(_param1 => _param1.p["Caption"].ToString().IndexOf(keyword, 0, StringComparison.CurrentCultureIgnoreCase) != -1).Select(_param1 => _param1.n).ToList<string>().FirstOrDefault<string>();
        }

        public static void Send(string portName, byte[] message) => Task.Run((Action)(() =>
       {
           SerialPort serialPort = new SerialPort();
           serialPort.PortName = portName;
           serialPort.Parity = Parity.None;
           serialPort.BaudRate = 19200;
           serialPort.DataBits = 8;
           serialPort.StopBits = StopBits.One;
           if (!serialPort.IsOpen)
               serialPort.Open();
           if (!serialPort.IsOpen)
               return;
           serialPort.Write(message, 0, message.Length);
           Thread.Sleep(1000);
           serialPort.Close();
           serialPort.Dispose();
       }));
    }
}
