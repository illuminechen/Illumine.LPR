namespace Illumine.LPR
{
    public class InfoService
    {
        public static SnapshotInfoViewModel GetViewModel(RecordViewModel recordVM)
        {
            SnapshotInfoViewModel snapshotVM = null;
            if (recordVM != null)
            {
                string str1;
                switch (recordVM.CarPlateViewModel.ParkingMode)
                {
                    case ParkingMode.Temporary:
                        str1 = "臨時停車";
                        break;
                    case ParkingMode.Vip:
                        str1 = "月租停車";
                        break;
                    case ParkingMode.NotVip:
                        str1 = "非月租停車";
                        break;
                    case ParkingMode.NotOnPeriod:
                        str1 = "錯誤時段";
                        break;
                    case ParkingMode.Expired:
                        str1 = "時效過期";
                        break;
                    case ParkingMode.Incorrect:
                        str1 = "進出錯誤";
                        break;
                    case ParkingMode.SmartPay:
                        str1 = "智慧支付";
                        break;
                    case ParkingMode.NoSpace:
                        str1 = "車位不足";
                        break;
                    case ParkingMode.NoPay:
                        str1 = "請至繳費機繳費";
                        break;
                    case ParkingMode.NotCoherence:
                        str1 = "ETag車牌不一致";
                        break;
                    default:
                        str1 = "未知模式";
                        break;
                }
                string str2;
                switch (recordVM.ChannelViewModel?.EntryMode)
                {
                    case EntryMode.In:
                        str2 = "進場";
                        break;
                    case EntryMode.Out:
                        str2 = "離場";
                        break;
                    default:
                        str2 = "未知方向";
                        break;
                }
                string str3 = recordVM.IsVip ? "白名單-" : "";
                snapshotVM = new SnapshotInfoViewModel(recordVM);
                snapshotVM.Title = str3 + str1 + "[" + str2 + "]";
                // snapshotVM.RecordId = recordVM.Id;

                snapshotVM.PlateNumber = new KeyValuePairViewModel()
                {
                    KeyText = "車牌",
                    ValueText = recordVM.CarPlateViewModel.PlateNumber
                };

                snapshotVM.CardNumber = new KeyValuePairViewModel()
                {
                    Visible = Container.Get<LPRSetting>().ETagMode != ETagMode.No,
                    KeyText = "卡片內碼",
                    ValueText = recordVM.eTagNumber
                };

                snapshotVM.ParkingMode = new KeyValuePairViewModel()
                {
                    Visible = false,
                    KeyText = "月票種類",
                    ValueText = "所在里雙月月票"
                };
                snapshotVM.TimeStamp = new KeyValuePairViewModel()
                {
                    KeyText = str2 + "時間",
                    ValueText = recordVM.TimeStamp.ToString("yyyy/MM/dd HH:mm:ss")
                };
            }
            return snapshotVM;
        }
    }
}
