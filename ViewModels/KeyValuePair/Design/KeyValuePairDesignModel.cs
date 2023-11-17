namespace Illumine.LPR
{
    public class KeyValuePairDesignModel : KeyValuePairViewModel
    {
        public static KeyValuePairDesignModel Instance => new KeyValuePairDesignModel();

        public KeyValuePairDesignModel()
        {
            this.KeyText = "Key";
            this.ValueText = "Value";
        }
    }
}
