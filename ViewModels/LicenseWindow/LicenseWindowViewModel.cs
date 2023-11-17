using System;
using System.Windows;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class LicenseWindowViewModel : BaseViewModel
    {
        public string Mac { get; set; }

        public bool IsCopied { get; set; }

        public string Reason { get; set; }

        public ICommand CopyCommand { get; set; }

        public ICommand FinishCommand { get; set; }

        public string Path => Environment.CurrentDirectory;

        public LicenseWindowViewModel()
        {
            this.Mac = NicHelper.GetMac();
            this.CopyCommand = (ICommand)new RelayCommand(new Action(this.Copy));
            this.FinishCommand = (ICommand)new RelayCommand<LicenseWindow>(new Action<LicenseWindow>(this.Finish));
        }

        private void Copy()
        {
            Clipboard.SetText(this.Mac);
            this.IsCopied = true;
        }

        private void Finish(LicenseWindow licenseWindow) => licenseWindow.Close();
    }
}
