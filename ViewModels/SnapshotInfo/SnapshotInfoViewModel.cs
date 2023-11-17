using System;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class SnapshotInfoViewModel : BaseViewModel
    {
        public RecordViewModel RecordViewModel { get; set; }

        public string Title { get; set; }

        public string NewPlateNumber { get; set; }

        public KeyValuePairViewModel PlateNumber { get; set; }

        public KeyValuePairViewModel CardNumber { get; set; }

        public KeyValuePairViewModel ParkingMode { get; set; }

        public KeyValuePairViewModel TimeStamp { get; set; }

        public ICommand CorrectCommand { get; set; }

        public SnapshotInfoViewModel()
        {
            this.CorrectCommand = (ICommand)new RelayCommand(new Action(this.Correct));
        }

        public SnapshotInfoViewModel(RecordViewModel RecordViewModel)
        {
            this.RecordViewModel = RecordViewModel;
            this.CorrectCommand = (ICommand)new RelayCommand(new Action(this.Correct));
        }

        private void Correct()
        {
            if (RecordViewModel.Id == -1)
                return;
            MsgData from = Container.GetFrom<MsgData>(RecordViewModel.Id);
            from.PlateNumber = this.NewPlateNumber.ToUpper();
            RecordPageViewModel recordPageViewModel = Container.Get<RecordPageViewModel>();
            if (recordPageViewModel.SelectedRecord != null)
                recordPageViewModel.SelectedRecord.CarPlateViewModel.PlateNumber = this.NewPlateNumber.ToUpper();
            else
                recordPageViewModel.OnPropertyChanged("RecordList");
            this.PlateNumber.ValueText = this.NewPlateNumber.ToUpper();
            RepositoryService.Update(from);
        }
    }
}
