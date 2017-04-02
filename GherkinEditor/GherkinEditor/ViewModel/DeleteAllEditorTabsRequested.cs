using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class DeleteAllEditorTabsRequested : EventArgs
    {
        public DeleteAllEditorTabsRequested(EditorTabContentViewModel excludedEditorViewModel)
        {
            ExcludedEditorViewModel = excludedEditorViewModel;
        }

        public EditorTabContentViewModel ExcludedEditorViewModel { get; private set; }
    }
}
