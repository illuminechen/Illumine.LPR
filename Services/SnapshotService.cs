using System;
using System.Collections.Generic;

namespace Illumine.LPR
{
    public class SnapshotService
    {
        public static PlateSnapshotViewModel GetViewModel(int RecordId) => new PlateSnapshotViewModel(Container.GetFrom<MsgData>(RecordId));
        public static PlateSnapshotViewModel GetViewModel(RecordViewModel recordViewModel) => new PlateSnapshotViewModel(recordViewModel.Msg);
        
        public static PlateSnapshotViewModel GetLastInViewModel(string PlateNumber)
        {
            MsgData lastOut = Container.Get<List<MsgData>>().Find((Predicate<MsgData>)(x => x.PlateNumber == PlateNumber && ChannelService.GetEntryMode(x.ChannelId) == EntryMode.Out));
            MsgData msg = Container.Get<List<MsgData>>().Find((Predicate<MsgData>)(x =>
           {
               if (!(x.PlateNumber == PlateNumber) || ChannelService.GetEntryMode(x.ChannelId) != EntryMode.In)
                   return false;
               return lastOut == null || x.TimeStamp > lastOut.TimeStamp;
           }));
            if (msg == null)
                return (PlateSnapshotViewModel)null;
            return new PlateSnapshotViewModel(msg)
            {
                LeftTopText = "上次入場:" + msg.TimeStamp.ToString("yyyy/MM/dd HH:mm:ss")
            };
        }
    }
}
