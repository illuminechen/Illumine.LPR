namespace Illumine.LPR
{
    public class ProgressDialogDesignModel : ProgressDialogViewModel
    {
        public static ProgressDialogDesignModel Instance => new ProgressDialogDesignModel();

        public ProgressDialogDesignModel()
        {
            Value = 1;
            Maximum = 100;
            Caption = "測試中...";
        }
    }
}
