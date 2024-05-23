using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Illumine.LPR
{
    public static class VipDataService
    {
        public static VipData GetData(int index) => Container.GetFrom<VipData>(index);

        public static int PutData(VipViewModel vm)
        {
            VipData data = new VipData();
            data.Name = vm.Name;
            data.Description = vm.Description;
            data.PlateNumber = vm.PlateNumber;
            data.ExpireTime = vm.ExpireTime;
            data.Periods = string.Join("", Enumerable.Repeat("1", 24));
            int num = Container.PutInto(data);
            data.Id = num;
            RepositoryService.Insert(data);
            return num;
        }

        public static ObservableCollection<PeriodCheckBoxItemViewModel> GetPeriods(int id)
        {
            var data = GetData(id);
            var ary = (bool[])TypeDescriptor.GetConverter(typeof(bool[])).ConvertFromString(data.Periods);
            ObservableCollection<PeriodCheckBoxItemViewModel> checkBoxItem = new ObservableCollection<PeriodCheckBoxItemViewModel>();
            for (int i = 0; i < 24; ++i)
            {
                checkBoxItem.Add(new PeriodCheckBoxItemViewModel(id, i, $"{i}", (i >= ary.Length) ? true : ary[i]));
            }
            return checkBoxItem;
        }

        public static bool GetPeriodValue(int VipId, int Index)
        {
            var data = GetData(VipId);
            return data.Periods[Index] == '1';
        }

        public static void SetPeriodValue(int VipId, int Index, bool Checked)
        {
            var data = GetData(VipId);
            var charAry = data.Periods.ToCharArray();
            charAry[Index] = Checked ? '1' : '0';
            data.Periods = new string(charAry);
        }

        public static void RemoveAtIndex(int index)
        {
            List<VipData> vipDataList = Container.Get<List<VipData>>();
            VipData data = vipDataList.Find(x => x.Id == index);
            RepositoryService.Delete(data);
            vipDataList.Remove(data);
        }

        public static void SetData(VipData data) => Container.PutInto(data);

        public static void ResetId()
        {
            List<VipData> vipDataList = Container.Get<List<VipData>>();
            for (int index = 0; index < vipDataList.Count; ++index)
            {
                int newId = index + 1;
                RepositoryService.ResetId(vipDataList[index], newId);
                vipDataList[index].Id = newId;
            }
        }
        public static bool IsValidPeriod(int vipId)
        {
            var ts = (DateTime.Now - DateTime.Now.Date);
            return GetPeriodValue(vipId, (int)ts.TotalHours);
        }

        public static bool CheckCoherence(string plateNumber, string eTagNumber)
        {
            List<VipData> vipDatas = Container.Get<List<VipData>>().FindAll(x => x?.ETagNumber?.Equals(eTagNumber) == true && x?.PlateNumber?.Replace("-", "").Equals(plateNumber.Replace("-", "")) == true);
            return vipDatas.Count >= 1;
        }

        public static string GetDisplayText(ParkingMode parkingMode)
        {
            switch (parkingMode)
            {
                case ParkingMode.Temporary:
                    return "臨時停車";
                case ParkingMode.Vip:
                    return "月租停車";
                case ParkingMode.NotVip:
                    return "非月租停車";
                case ParkingMode.NotOnPeriod:
                    return "錯誤時段";
                case ParkingMode.Expired:
                    return "時效過期";
                case ParkingMode.Incorrect:
                    return "進出錯誤";
                case ParkingMode.SmartPay:
                    return "智慧支付";
                case ParkingMode.NoSpace:
                    return "車位已滿";
                case ParkingMode.NoPay:
                    return "請至繳費機繳費";
                case ParkingMode.CantPass:
                    return "無法通行";
                case ParkingMode.NotCoherence:
                    return "ETag車牌不一致";
                default:
                    return "未知模式";
            }
        }

        public static ParkingMode CheckETagValid(string number, out VipData vip)
        {
            List<VipData> vipDatas = Container.Get<List<VipData>>().FindAll(x => x?.ETagNumber?.Equals(number) == true);
            vip = vipDatas.FirstOrDefault();
            return CheckOther(vipDatas);
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static ParkingMode CheckPlateValid(string number, out VipData vip)
        {
            var pureplate = number.Replace("-", "");
            List<VipData> vipDatas = Container.Get<List<VipData>>().FindAll(x => x?.PlateNumber?.Replace("-", "") == pureplate);
            if (Container.Get<LPRSetting>().AmbiguousPlate && vipDatas.Count == 0)
            {
                vipDatas = Container.Get<List<VipData>>().FindAll(x => Compute(x.PlateNumber, pureplate) < 2);
            }
            vip = vipDatas.FirstOrDefault();
            return CheckOther(vipDatas);
        }

        public static ParkingMode CheckOther(List<VipData> vipDatas)
        {
            if (vipDatas.Count == 0)
                return ParkingMode.NotVip;

            if (!vipDatas.Exists(x => x.ExpireTime.Date.AddDays(1.0) > DateTime.Now))
                return ParkingMode.Expired;

            if (!vipDatas.Exists(x => IsValidPeriod(x.Id)))
                return ParkingMode.NotOnPeriod;

            return ParkingMode.Vip;
        }

        public static ParkingMode CheckSpace(EntryMode mode, VipData vip)
        {
            if (!vip.Group.HasValue)
                return ParkingMode.Vip;
            // space
            var group = GroupDataService.GetData(vip.Group.Value);
            if (group != null)
            {
                if (mode == EntryMode.In && group.CurrentCount == 0)
                    return ParkingMode.NoSpace;
            }
            return ParkingMode.Vip;
        }

        public static void UpdateGroup(EntryMode mode, VipData vip)
        {
            if (!vip.Group.HasValue)
                return;
            var group = GroupDataService.GetData(vip.Group.Value);
            if (group != null)
            {
                group.CurrentCount += (mode == EntryMode.In) ? -1 : 1;
                group.CurrentCount = Math.Max(0, Math.Min(group.CurrentCount, group.TotalCount));
                Container.Get<VipListPageViewModel>().GroupList.First(x => x.Id == group.Id).CurrentCount = group.CurrentCount;

                RepositoryService.Update(group);
            }
        }
    }
}
