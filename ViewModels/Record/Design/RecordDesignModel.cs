namespace Illumine.LPR
{
    public class RecordDesignModel : RecordViewModel
    {
        public static RecordDesignModel Instance => new RecordDesignModel();

        public RecordDesignModel()
        {
            DataInitializer.SetupDesignData();
            this.Id = 1;
            this.CarPlateViewModel = new CarPlateViewModel()
            {
                PlateNumber = "ABC8888",
                ParkingMode = ParkingMode.SmartPay
            };
        }
    }
}
