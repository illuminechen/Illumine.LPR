namespace Illumine.LPR
{
  public class CarPlateDesignModel : CarPlateViewModel
  {
    public static ChannelDesignModel Instance => new ChannelDesignModel();

    public CarPlateDesignModel()
    {
      DataInitializer.SetupDesignData();
      this.PlateNumber = "ABC8888";
      this.ParkingMode = ParkingMode.SmartPay;
    }
  }
}
