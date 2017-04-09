using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class OpenNewGherkinEditorRequestedArg : EventArgs
    {
        public OpenNewGherkinEditorRequestedArg(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; private set; }
    }
}
