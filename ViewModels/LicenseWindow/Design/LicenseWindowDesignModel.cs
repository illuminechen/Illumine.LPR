namespace Illumine.LPR
{
    public class LicenseWindowDesignModel : LicenseWindowViewModel
    {
        public static LicenseWindowDesignModel Instance => new LicenseWindowDesignModel();

        public LicenseWindowDesignModel()
        {
            this.Mac = "AA-BB-CC-DD-EE-FF";
            this.Reason = "測試";
        }
    }
}
