using System.Collections.Generic;

namespace Illumine.LPR
{
    public class MainWindowViewModel : BaseViewModel
    {
        public bool IsVipEnabled => Container.Get<LPRSetting>().IsVipEnabed;

        //public List<ChannelViewerViewModel> ChannelViewerModelList => ChannelViewerService.GetList();

        //public ChannelViewerViewModel ChannelViewerModel1 { get; set; }

        //public ChannelViewerViewModel ChannelViewerModel2 { get; set; }

        //public ChannelViewerViewModel ChannelViewerModel3 { get; set; }

        //public ChannelViewerViewModel ChannelViewerModel4 { get; set; }

        public RecordPageViewModel RecordPageViewModel { get; set; }

        public VipListPageViewModel VipListPageViewModel { get; set; }

        public PagingGirdItemsControlViewModel PagingGirdItemsControlViewModel { get; set; }

        public MainWindowViewModel()
        {
            //this.ChannelViewerModel1 = Container.Get<ChannelViewerViewModel>(1);
            //this.ChannelViewerModel2 = Container.Get<ChannelViewerViewModel>(2);
            //this.ChannelViewerModel3 = Container.Get<ChannelViewerViewModel>(3);
            //this.ChannelViewerModel4 = Container.Get<ChannelViewerViewModel>(4);
            this.PagingGirdItemsControlViewModel = Container.Get<PagingGirdItemsControlViewModel>();
            this.RecordPageViewModel = Container.Get<RecordPageViewModel>();
            this.VipListPageViewModel = Container.Get<VipListPageViewModel>();
        }
    }
}
