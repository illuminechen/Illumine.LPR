namespace Illumine.LPR
{
    public class MainWindowDesignModel : MainWindowViewModel
    {
        public static MainWindowDesignModel Instance => new MainWindowDesignModel();

        public MainWindowDesignModel() => DataInitializer.SetupDesignData();
    }
}
