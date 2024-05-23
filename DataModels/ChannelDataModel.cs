using System.ComponentModel;

namespace Illumine.LPR
{
    public enum CameraType { Model1, Ice, Ice_eng }

    [DisplayName("Channel")]
    public class ChannelDataModel
    {
        public int Id { get; set; }

        public CameraType CameraType { get; set; } = CameraType.Model1;

        public CameraType BackCameraType { get; set; } = CameraType.Model1;

        public string BackCameraIp { get; set; } = "";

        public int InitialTimeout { get; set; } = 3000;

        public string Ip { get; set; } = "127.0.0.1";

        public string SensorIp { get; set; } = "";

        public string Led1Ip { get; set; } = "";

        public int Led1Port { get; set; } = 5200;

        public string Led2Ip { get; set; } = "";

        public int Led2Port { get; set; } = 5200;

        public string EtagReaderIp { get; set; } = "";

        public string Line1Normal { get; set; } = "";

        public string Line2Normal { get; set; } = "";

        public string Line1NoVip { get; set; } = "";

        public string Line2NoVip { get; set; } = "";

        public string Line1Fail { get; set; } = "";

        public string Line2Fail { get; set; } = "";

        public string Line1Active { get; set; } = "";

        public string Line2Active { get; set; } = "";

        public string Line1CantPass { get; set; } = "";

        public string Line2CantPass { get; set; } = "";

        public string Watch { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool ExtOutput { get; set; }

        public string ExtSubFolder { get; set; } = "";

        public EntryMode EntryMode { get; set; }

        public VehicleMode VehicleMode { get; set; }

        public PresentMode PresentMode { get; set; }
    }
}
