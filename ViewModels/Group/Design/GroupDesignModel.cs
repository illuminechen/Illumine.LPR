namespace Illumine.LPR
{
    public class GroupDesignModel : GroupViewModel
    {
        public static GroupDesignModel Instance => new GroupDesignModel();

        public GroupDesignModel() => DataInitializer.SetupDesignData();
    }
}
