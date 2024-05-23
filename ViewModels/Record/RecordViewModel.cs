using System;
using System.Drawing;

namespace Illumine.LPR
{
    public class RecordViewModel : BaseViewModel
    {
        public MsgData Msg { get; set; }

        public int Id { get; set; }

        public int ChannelId { get; set; }

        public DateTime TimeStamp { get; set; }

        public string ImagePath { get; set; }

        public int PlateX { get; set; }

        public int PlateY { get; set; }

        public bool IsVip { get; set; }

        public CarPlateViewModel CarPlateViewModel { get; set; }

        public Image Image => Image.FromFile(this.ImagePath);

        public ChannelViewModel ChannelViewModel => Container.Get<ChannelViewModel>(this.ChannelId);

        public string EntryDirection => this.ChannelViewModel.EntryMode != EntryMode.In ? "出口" : "入口";

        public string ChannelName => ChannelService.GetEntryName(this.ChannelId);

        public string eTagNumber { get; set; }

        public RecordViewModel()
        {
        }

        public RecordViewModel(MsgData msg)
        {
            this.Id = msg.Id;
            this.ChannelId = msg.ChannelId;
            this.TimeStamp = msg.TimeStamp;
            this.ImagePath = msg.ImagePath;
            this.PlateX = msg.PlateX;
            this.PlateY = msg.PlateY;
            this.CarPlateViewModel = new CarPlateViewModel()
            {
                ParkingMode = msg.ParkingMode,
                PlateNumber = msg.PlateNumber
            };
            if (Container.Get<LPRSetting>().IsVipEnabed && Container.Get<LPRSetting>().ETagMode != ETagMode.No)
            {
                string[] pairs = msg.Tag.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pair in pairs)
                {
                    string[] eles = pair.Split('=');

                    if (eles[0] == "eTagNumber")
                        eTagNumber = eles[1];
                }
            }
            this.Msg = msg;
        }
    }
}
