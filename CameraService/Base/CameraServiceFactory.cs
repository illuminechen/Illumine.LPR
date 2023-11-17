namespace Illumine.LPR
{
    public static class CameraServiceFactory
    {
        static Model1CameraService model1Service;
        static ICECameraService iceService;
        static ICE_engCameraService ice_engService;

        public static ICameraService Create(CameraType type)
        {
            switch (type)
            {
                case CameraType.Model1:
                    if (model1Service == null)
                    {
                        model1Service = new Model1CameraService();
                    }
                    return model1Service;
                case CameraType.Ice:
                    if (iceService == null)
                    {
                        iceService = new ICECameraService();
                    }
                    return iceService;
                case CameraType.Ice_eng:
                    if (ice_engService == null)
                    {
                        ice_engService = new ICE_engCameraService();
                    }
                    return ice_engService;
                default:
                    return null;
            }
        }
    }
}
