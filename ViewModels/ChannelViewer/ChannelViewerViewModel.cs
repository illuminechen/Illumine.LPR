namespace Illumine.LPR
{
    public class ChannelViewerViewModel : BaseViewModel
    {
        //public int CurrentRecordId { get; set; }

        public int ChannelId { get; set; }

        public RecordViewModel RecordViewModel { get; set; } // RecordService.GetViewModel(this.CurrentRecordId);

        public CameraViewerViewModel CameraViewModel { get; set; }

        public PlateSnapshotViewModel PlateSnapshotViewModel { get; set; }

        public bool Streaming { get; set; } = true;
    }
}
