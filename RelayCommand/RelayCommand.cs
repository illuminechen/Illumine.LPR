using System;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute = () => true;


        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            this._execute = action;
            if (canExecute != null)
                this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => this._canExecute();

        public void Execute(object parameter) => this._execute();

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute = (t) => true;

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RelayCommand(Action<T> action, Func<T, bool> canExecute = null)
        {
            this._execute = action;
            if (canExecute != null)
                this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => this._canExecute(parameter == null ? default(T) : (T)parameter);

        public void Execute(object parameter) => this._execute(parameter == null ? default(T) : (T)parameter);

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

    }

}
