namespace Illumine.LPR
{
    public class ConfigPageDesignModel : ConfigPageViewModel
    {
        public static ConfigPageDesignModel Instance => new ConfigPageDesignModel();

        public ConfigPageDesignModel() => DataInitializer.SetupDesignData();
    }
}
