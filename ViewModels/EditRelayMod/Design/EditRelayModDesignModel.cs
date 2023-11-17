namespace Illumine.LPR
{
    public class EditRelayModDesignModel : EditRelayModViewModel
    {
        public static EditRelayModDesignModel Instance => new EditRelayModDesignModel();

        public EditRelayModDesignModel()
        {
            DataInitializer.SetupDesignData();
        }
    }
}
