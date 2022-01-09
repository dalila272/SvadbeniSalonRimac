using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SvadbeniSalon.WinUI.Commands
{
    public class CustomCommand : ICommand
    {
        private Action<object> execute;
        private Predicate<object> canExecute;

        public event EventHandler CanExecuteChanged;

        public CustomCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);

        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

    }
}
