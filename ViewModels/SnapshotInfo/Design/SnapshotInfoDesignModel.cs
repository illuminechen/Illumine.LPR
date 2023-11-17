using System;

namespace Illumine.LPR
{
    public class SnapshotInfoDesignModel : SnapshotInfoViewModel
    {
        public static SnapshotInfoDesignModel Instance => new SnapshotInfoDesignModel();

        public SnapshotInfoDesignModel()
        {
            DataInitializer.SetupDesignData();
            this.RecordViewModel = new RecordViewModel(new MsgData());
            this.RecordViewModel.Id = 1;
            this.Title = "智慧支付離場";
            this.PlateNumber = new KeyValuePairViewModel()
            {
                KeyText = "入場車牌",
                ValueText = "ABC8888"
            };
            this.CardNumber = new KeyValuePairViewModel()
            {
                KeyText = "卡片內碼",
                ValueText = "1234567890"
            };
            this.ParkingMode = new KeyValuePairViewModel()
            {
                KeyText = "月票種類",
                ValueText = "所在里雙月月票"
            };
            this.TimeStamp = new KeyValuePairViewModel()
            {
                KeyText = "入場時間",
                ValueText = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            };
        }
    }
}
