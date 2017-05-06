using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gherkin.ViewModel;
using Gherkin.Util;

namespace Gherkin.ViewModel
{
    public class CloseWindowCommand
    {
        public static readonly ICommand Instance = new CloseWindowCommand().CloseCmd;

        private CloseWindowCommand()
        {
        }

        private ICommand CloseCmd => new DelegateCommand<object>(Execute, CanExecute);

        private bool CanExecute(object parameter)
        {
            //we can only close Windows
            return (parameter is Window);
        }

        private void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                ((Window)parameter).Close();
            }
        }
    }
}
