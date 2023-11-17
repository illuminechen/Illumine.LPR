using System.Windows.Input;

namespace Illumine.LPR
{
    public class ProgressDialogViewModel : BaseViewModel
    {
        public int Value { get; set; } = 0;

        public int Maximum { get; set; } = 0;

        public string Caption { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public bool? DialogResult { get; set; }

        public ProgressDialogViewModel()
        {

        }
    }
}
