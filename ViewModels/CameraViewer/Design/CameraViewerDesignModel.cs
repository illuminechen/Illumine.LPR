using Illumine.LPR;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class CameraViewerDesignModel : CameraViewerViewModel
    {
        public static CameraViewerDesignModel Instance => new CameraViewerDesignModel();

        public CameraViewerDesignModel()
        {
            DataInitializer.SetupDesignData();
            this.IsPlaying = false;
        }
    }
}
