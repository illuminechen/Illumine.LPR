using System.Collections.Generic;

namespace Illumine.LPR
{
    public class ChannelService
    {
        public static List<ChannelViewModel> GetList()
        {
            var list = Container.Get<List<ChannelViewModel>>();
            if (list == null)
            {
                list = new List<ChannelViewModel>();
                foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                {
                    var cvm = Container.Get<ChannelViewModel>(channelDataModel.Id);
                    if (cvm != null)
                        list.Add(cvm);
                }
                //    ChannelViewerViewModel channelViewerViewModel1 = Container.Get<ChannelViewerViewModel>(1);
                //    ChannelViewerViewModel channelViewerViewModel2 = Container.Get<ChannelViewerViewModel>(2);
                //    ChannelViewerViewModel channelViewerViewModel3 = Container.Get<ChannelViewerViewModel>(3);
                //    ChannelViewerViewModel channelViewerViewModel4 = Container.Get<ChannelViewerViewModel>(4);
                //    if (channelViewerViewModel1 != null)
                //        list.Add(channelViewerViewModel1);
                //    if (channelViewerViewModel2 != null)
                //        list.Add(channelViewerViewModel2);
                //    if (channelViewerViewModel3 != null)
                //        list.Add(channelViewerViewModel3);
                //    if (channelViewerViewModel4 != null)
                //        list.Add(channelViewerViewModel4);
                Container.Put<List<ChannelViewModel>>(list);
            }

            return list;

        }
        public static string GetEntryName(int ChannelId)
        {
            ChannelViewModel channelViewModel = Container.Get<ChannelViewModel>(ChannelId);
            string str1 = "";
            string str2 = "";
            if (channelViewModel != null)
            {
                switch (channelViewModel.VehicleMode)
                {
                    case VehicleMode.Car:
                        str1 = "汽車";
                        break;
                    case VehicleMode.Motor:
                        str1 = "機車";
                        break;
                    case VehicleMode.Both:
                        str1 = "車道";
                        break;
                }
                switch (channelViewModel.EntryMode)
                {
                    case EntryMode.In:
                        str2 = "入口";
                        break;
                    case EntryMode.Out:
                        str2 = "出口";
                        break;
                }
            }
            return str1 + str2 + ChannelId;
        }

        public static EntryMode GetEntryMode(int ChannelId) => Container.Get<ChannelViewModel>(ChannelId).EntryMode;

        public static bool GetEnabled(int ChannelId) => Container.Get<ChannelViewModel>(ChannelId).Enabled;
    }
}
