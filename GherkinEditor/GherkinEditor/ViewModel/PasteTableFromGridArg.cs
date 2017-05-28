using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Gherkin.ViewModel
{
    public class PasteTableFromGridArg : ExchangeTableWithGridTableArg
    {
        public PasteTableFromGridArg(TextEditor editor) : base(editor)
        {
        }
    }
}
