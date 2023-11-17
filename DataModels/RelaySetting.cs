using System;
using System.ComponentModel;
using System.Linq;

namespace Illumine.LPR
{
    [DisplayName("Relay")]
    public class RelaySetting
    {
        public string PortName => SerialPortHelper.GetPortName(this.KeyName);

        public string KeyName { get; set; } = "USB to UART";

        public bool[] TriggerRelay { get; set; } = Enumerable.Range(0, 16).Select<int, bool>((Func<int, bool>)(x => x == 0)).ToArray<bool>();

        public int OpenSeconds { get; set; } = 3;
    }
}
