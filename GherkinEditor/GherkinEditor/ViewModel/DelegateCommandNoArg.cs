using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class DelegateCommandNoArg : ICommand
    {
        readonly Action _execute;
        readonly Func<bool> _canExecute;
        public DelegateCommandNoArg(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException();
            _execute = execute;
            _canExecute = canExecute;
        }
        public DelegateCommandNoArg(Action execute)
        : this(execute, null)
        {
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();

        /// <summary>
        ///  we have to override the default behavior of the event and register to
        ///  the CommandManager.RequerySuggested. This is very important
        ///  because by doing so the CanExecute method will be called when ever
        ///  there is focus changed, user input etc…
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
