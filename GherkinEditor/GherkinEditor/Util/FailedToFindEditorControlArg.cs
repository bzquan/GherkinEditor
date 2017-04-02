using System;
using Gherkin.ViewModel;

namespace Gherkin.Util
{
    public class FailedToFindEditorControlArg : EventArgs
    {
        public FailedToFindEditorControlArg(EditorTabContentViewModel editorViewModel)
        {
            EditorViewModel = editorViewModel;
        }

        public EditorTabContentViewModel EditorViewModel { get; private set; }
    }
}
