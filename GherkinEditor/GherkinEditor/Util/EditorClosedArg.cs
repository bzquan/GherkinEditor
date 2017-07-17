using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class EditorClosedArg : EventArgs
    {
        public EditorClosedArg(string filename)
        {
            FileName = filename;
        }

        public string FileName { get; private set; }
    }
}
