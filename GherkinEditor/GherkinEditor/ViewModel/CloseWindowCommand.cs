using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class CloseWindowCommand : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            //we can only close Windows
            return (parameter is Window);
        }

#pragma warning disable 67  // Get rid of “[some event] never used” compiler warnings
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                ((Window)parameter).Close();
            }
        }

        #endregion

        private CloseWindowCommand()
        {
        }

        public static readonly ICommand Instance = new CloseWindowCommand();
    }
}
