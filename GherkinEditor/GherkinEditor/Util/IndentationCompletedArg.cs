using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class IndentationCompletedArg : EventArgs
    {
        public IndentationCompletedArg(int origLine, int origColumn, string origText)
        {
            Line = origLine;
            Column = origColumn;
            LineText = origText;
        }

        public int Line { get; private set; }
        public int Column { get; private set; }
        public string LineText { get; private set; }
    }
}
