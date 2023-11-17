using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class BaseViewModel : ValidatorBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        protected async Task RunCommand(Expression<Func<bool>> updatingFlag, Func<Task> action)
        {
            if (updatingFlag.GetPropertyValue<bool>())
                return;
            updatingFlag.SetPropertyValue<bool>(true);
            try
            {
                await action();
            }
            finally
            {
                updatingFlag.SetPropertyValue<bool>(false);
            }
        }
    }
}
