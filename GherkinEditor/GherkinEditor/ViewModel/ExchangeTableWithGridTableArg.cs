using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Gherkin.ViewModel
{
    public class ExchangeTableWithGridTableArg : EventArgs
    {
        public ExchangeTableWithGridTableArg(TextEditor editor)
        {
            TextEditor = editor;
        }

        public TextEditor TextEditor { get; private set; }
    }
}
