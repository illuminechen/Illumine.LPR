using System.ComponentModel;

namespace Illumine.LPR
{
    [DisplayName("LPR")]
    public class LPRSetting
    {
        public string ProjectName { get; set; } = "";

        public string Ip { get; set; } = "127.0.0.1";

        public string HostIp { get; set; } = "127.0.0.1";

        public bool UseRemoteServer { get; set; } = false;

        public string JToken { get; set; } = "";

        public string WhiteList { get; set; } = "";

        public bool IsVipEnabed { get; set; }

        public bool IsCheckingInOut { get; set; } = true;

        public bool UseParkingServer { get; set; } = false;

        public bool UseParkingServerEx { get; set; } = false;

        public string ParkingServerHostUrl { get; set; } = "127.0.0.1";

        public string ParkingServerToken { get; set; } = "";

        public ETagMode ETagMode { get; set; } = ETagMode.No;

        public int ETagWaitingTime { get; set; } = 3000;

        public string ImageDirectory { get; set; } = "./Image/";

        public string MsgPath { get; set; } = "./Message.db";

        public string DataPath { get; set; } = "./Data.db";

        public bool FilteringNoPlate { get; set; } = false;

        public bool AmbiguousPlate { get; set; } = false;

        public bool UsePlateAddingDash { get; set; } = false;

        public int Port { get; set; } = 2019;

        public int HostPort { get; set; } = 2020;

        public bool ExtOutput { get; set; }

        public string ExtFolder { get; set; } = "./Ext/";

        public LPRMode LPRMode { get; set; } = LPRMode.UDPServer;

        public string NoPlate { get; set; } = "#######";

        public ImageQuality ImageQuality { get; set; }
    }
}
