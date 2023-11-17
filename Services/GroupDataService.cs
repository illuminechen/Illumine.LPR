using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Illumine.LPR
{
    public static class GroupDataService
    {
        public static GroupData GetData(int index) => Container.GetFrom<GroupData>(index);

        public static int PutData(GroupViewModel vm)
        {
            GroupData data = new GroupData();
            data.GroupName = vm.GroupName;
            data.TotalCount = vm.TotalCount;
            data.CurrentCount = vm.CurrentCount;
            int num = Container.PutInto(data);
            data.Id = num;
            RepositoryService.Insert(data);
            return num;
        }

        public static void RemoveAtIndex(int index)
        {
            List<GroupData> groupDataList = Container.Get<List<GroupData>>();
            GroupData data = groupDataList.Find(x => x.Id == index);
            RepositoryService.Delete(data);
            groupDataList.Remove(data);
        }

        public static void SetData(GroupData data) => Container.PutInto(data);

        public static void ResetId()
        {
            List<GroupData> groupDataList = Container.Get<List<GroupData>>();
            for (int index = 0; index < groupDataList.Count; ++index)
            {
                int newId = index + 1;
                RepositoryService.ResetId(groupDataList[index], newId);
                groupDataList[index].Id = newId;
            }
        }
 
    }
}
