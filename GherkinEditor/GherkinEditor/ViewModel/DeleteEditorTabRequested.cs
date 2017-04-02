using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.View;

namespace Gherkin.ViewModel
{
    public class DeleteEditorTabRequested : EventArgs
    {
        public DeleteEditorTabRequested(EditorTabContentViewModel editorViewModel)
        {
            EditorViewModel = editorViewModel;
        }

        public EditorTabContentViewModel EditorViewModel { get; private set; }
    }
}
