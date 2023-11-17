using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Illumine.LPR
{
    public class RecordService
    {
        public static RecordViewModel GetViewModel(int RecordId)
        {
            MsgData msg = Container.GetFrom<MsgData>(RecordId);
            return msg != null ? new RecordViewModel(msg) : null;
        }

        public static ObservableCollection<RecordViewModel> GetViewModels() => new ObservableCollection<RecordViewModel>(Container.Get<List<MsgData>>().Select<MsgData, RecordViewModel>((Func<MsgData, RecordViewModel>)(x => new RecordViewModel(x))));
    }
}
