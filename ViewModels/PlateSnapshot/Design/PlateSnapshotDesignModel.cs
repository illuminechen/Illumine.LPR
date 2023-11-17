namespace Illumine.LPR
{
  public class PlateSnapshotDesignModel : PlateSnapshotViewModel
  {
    public static PlateSnapshotDesignModel Instance => new PlateSnapshotDesignModel();

    public PlateSnapshotDesignModel()
    {
      DataInitializer.SetupDesignData();
      this.SnapshotPath = @"D:\code\Illumine.LPR\bin\Debug\Image\1\20210316195243830.jpg";
      this.PlateNumber = "TEST100";
    }
  }
}
