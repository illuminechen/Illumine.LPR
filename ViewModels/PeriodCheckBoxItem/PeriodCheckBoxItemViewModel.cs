using System.ComponentModel;

namespace Illumine.LPR
{
    public class PeriodCheckBoxItemViewModel : BaseViewModel
    {
        public PeriodCheckBoxItemViewModel()
        {

        }

        public PeriodCheckBoxItemViewModel(int VipId, int Index, string Label, bool IsChecked)
        {
            this.VipId = VipId;
            this.Index = Index;
            this.Label = Label;
            this.IsChecked = IsChecked;
            this.PropertyChanged += PeriodCheckBoxItemViewModel_PropertyChanged;
        }
        public int VipId { get; set; }

        public int Index { get; set; }

        public string Label { get; set; }

        public bool IsChecked { get; set; } = true;        

        private void PeriodCheckBoxItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsChecked"))
                return;
            VipDataService.SetPeriodValue(VipId, Index, IsChecked);
            RepositoryService.Update(VipDataService.GetData(VipId));
        }
    }
}
