using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class StatusChangedArg : EventArgs
    {
        public StatusChangedArg(string status_msg)
        {
            StatusMsg = status_msg;
        }

        public string StatusMsg { get; private set; }
    }
}
