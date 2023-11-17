using Illumine.LPR.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class EditRelayModViewModel : BaseViewModel
    {
        private Dictionary<string, string> backup;

        public ObservableCollection<RelaySettingCheckBoxItemViewModel> RelayList
        {
            get => EditRelayService.GetCheckBoxItem(Container.Get<RelaySetting>().TriggerRelay);
        }

        public string Comport { get; set; } = Container.Get<RelaySetting>().KeyName;

        public int ActionTime { get; set; } = Container.Get<RelaySetting>().OpenSeconds;

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public bool? DialogResult { get; set; }

        public EditRelayModViewModel()
        {
            this.PropertyChanged += new PropertyChangedEventHandler(this.EditRelayModViewModel_PropertyChanged);
            this.backup = ItemConverter<RelaySetting>.GetDict(Container.Get<RelaySetting>());
            this.OkCommand = new RelayCommand(new Action(this.Ok));
            this.CancelCommand = new RelayCommand(new Action(this.Cancel));
        }

        private void Cancel()
        {
            Container.Put(ItemConverter<RelaySetting>.GetData(this.backup));
            this.DialogResult = new bool?(false);
        }

        private void Ok()
        {
            LPRSettingService.Save();
            this.DialogResult = new bool?(true);
        }

        private void EditRelayModViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if (!(propertyName == "Comport"))
            {
                if (!(propertyName == "ActionTime"))
                    return;
                Container.Get<RelaySetting>().OpenSeconds = this.ActionTime;
            }
            else
                Container.Get<RelaySetting>().KeyName = this.Comport;
        }
    }
}
