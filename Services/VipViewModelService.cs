using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Illumine.LPR
{
    public static class VipViewModelService
    {
        public static ObservableCollection<VipViewModel> GetViewModels() => new ObservableCollection<VipViewModel>(Container.Get<List<VipData>>()?.Select(x => new VipViewModel(x)));

        public static void ResetId(IList<VipViewModel> vmList)
        {
            for (int index = 0; index < vmList.Count; ++index)
                vmList[index].Id = index + 1;
        }
    }
}
