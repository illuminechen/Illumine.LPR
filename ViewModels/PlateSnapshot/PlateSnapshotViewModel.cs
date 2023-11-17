using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Illumine.LPR
{
    public class PlateSnapshotViewModel : BaseViewModel
    {
        public int ChannelId { get; set; }

        public ChannelViewModel ChannelViewModel => Container.Get<ChannelViewModel>(this.ChannelId);

        public string LeftTopText { get; set; } = "上次入場";

        public string LeftBotText => "入場車牌:" + this.PlateNumber;

        public string SnapshotPath { get; set; }
        public BitmapImage SmallSnapshot { get; set; }
        public DateTime TimeStamp { get; set; }

        public string PlateNumber { get; set; }

        public PlateSnapshotViewModel()
        {
            
        }

        public PlateSnapshotViewModel(MsgData msg)
        {
            this.ChannelId = msg.ChannelId;
            this.TimeStamp = msg.TimeStamp;
            this.PlateNumber = msg.PlateNumber;
            this.SnapshotPath = msg.ImagePath;
            this.SmallSnapshot = Image.FromFile(msg.ImagePath).ToBitmapImage();
            this.LeftTopText = "";
        }
    }
}
