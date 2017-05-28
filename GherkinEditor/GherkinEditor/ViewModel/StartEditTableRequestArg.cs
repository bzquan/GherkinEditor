using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Gherkin.ViewModel
{
    public class StartEditTableRequestArg : ExchangeTableWithGridTableArg
    {
        public StartEditTableRequestArg(TextEditor editor) : base(editor)
        {
        }
    }
}
