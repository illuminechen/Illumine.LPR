namespace Illumine.LPR
{
    public class ChannelViewerDesignModel : ChannelViewerViewModel
    {
        public static ChannelViewerDesignModel Instance => new ChannelViewerDesignModel();

        public ChannelViewerDesignModel()
        {
            DataInitializer.SetupDesignData();
            this.ChannelId = 1;
            this.RecordViewModel = RecordService.GetViewModel(1);
        }
    }
}
