namespace Illumine.LPR
{
    public class ChannelDesignModel : ChannelViewModel
    {
        public static ChannelDesignModel Instance => new ChannelDesignModel();

        public ChannelDesignModel() => DataInitializer.SetupDesignData();
    }
}
