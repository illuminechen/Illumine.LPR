using System;

namespace Illumine.LPR
{
    public class MsgData : IIndexData
    {
        public int Id { get; set; }

        public int ChannelId { get; set; }

        public DateTime TimeStamp { get; set; }

        public string PlateNumber { get; set; }

        public int PlateX { get; set; }

        public int PlateY { get; set; }

        public int PlateWidth { get; set; }

        public int PlateHeight { get; set; }

        public string ImagePath { get; set; }

        public ParkingMode ParkingMode { get; set; }

        public string Tag { get; set; }
    }
}
