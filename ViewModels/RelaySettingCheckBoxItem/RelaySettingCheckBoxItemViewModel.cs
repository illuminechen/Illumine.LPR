using System.ComponentModel;

namespace Illumine.LPR
{
    public class RelaySettingCheckBoxItemViewModel : BaseViewModel
    {
        public int Index { get; set; }

        public string Label { get; set; }

        public bool IsChecked { get; set; }

        public RelaySettingCheckBoxItemViewModel() => this.PropertyChanged += new PropertyChangedEventHandler(this.CheckBoxItemViewModel_PropertyChanged);

        private void CheckBoxItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsChecked"))
                return;
            Container.Get<RelaySetting>().TriggerRelay[this.Index] = this.IsChecked;
        }
    }
}
