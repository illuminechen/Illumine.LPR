using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Illumine.LPR
{
    public static class GroupViewModelService
    {
        public static ObservableCollection<GroupViewModel> GetViewModels() => new ObservableCollection<GroupViewModel>(Container.Get<List<GroupData>>()?.Select(x => new GroupViewModel(x)));

        public static void ResetId(IList<GroupViewModel> vmList, IList<VipViewModel> vipList)
        {
            for (int index = 0; index < vmList.Count; ++index)
            {
                vipList.ToList().FindAll(x => x.Group == vmList[index].Id).ForEach(x => x.Group = index + 1);
                vmList[index].Id = index + 1;
            }
        }
    }
}
