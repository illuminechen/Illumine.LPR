namespace Illumine.LPR
{
    public class RecordPageDesignModel : RecordPageViewModel
    {
        public static RecordPageDesignModel Instance => new RecordPageDesignModel();

        public RecordPageDesignModel() => DataInitializer.SetupDesignData();
    }
}
