namespace Illumine.LPR
{
    public class VipDesignModel : VipViewModel
    {
        public static VipDesignModel Instance => new VipDesignModel();

        public VipDesignModel() => DataInitializer.SetupDesignData();
    }
}
