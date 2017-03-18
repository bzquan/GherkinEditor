using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class ShowCodeGenMessageWindowArg : EventArgs
    {
        public ShowCodeGenMessageWindowArg(bool showMessageWindow, string message = "")
        {
            ShowMessageWindow = showMessageWindow;
            Message = message;
        }

        public bool ShowMessageWindow { get; private set; }
        public string Message { get; set; }
    }
}
