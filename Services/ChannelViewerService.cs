using System.Collections.Generic;

namespace Illumine.LPR
{
    public class ChannelViewerService
    {
        public static List<ChannelViewerViewModel> GetList()
        {
            var list = Container.Get<List<ChannelViewerViewModel>>();
            if (list == null)
            {
                list = new List<ChannelViewerViewModel>();
                foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                {
                    var cvvm = Container.Get<ChannelViewerViewModel>(channelDataModel.Id);
                    if (cvvm != null)
                        list.Add(cvvm);
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
                Container.Put<List<ChannelViewerViewModel>>(list);
            }

            return list;
        }

        public static ChannelViewerViewModel Get(int ChannelId)
        {
            return Container.Get<ChannelViewerViewModel>(ChannelId);
        }
    }
}
