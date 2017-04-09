using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class SaveAsDocumentRequestedArg : EventArgs
    {
        public SaveAsDocumentRequestedArg(EditorTabContentViewModel tabContentViewModel)
        {
            SourceTabContentViewModel = tabContentViewModel;
        }

        public EditorTabContentViewModel SourceTabContentViewModel { get; private set; }
    }
}
