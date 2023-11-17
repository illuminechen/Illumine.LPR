namespace Illumine.LPR
{
    public class PagingGirdItemsControlDesignModel : PagingGirdItemsControlViewModel
    {
        public static PagingGirdItemsControlViewModel Instance => new PagingGirdItemsControlViewModel();

        public PagingGirdItemsControlDesignModel() => DataInitializer.SetupDesignData();
    }
}
