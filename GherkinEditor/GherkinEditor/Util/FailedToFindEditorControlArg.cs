using System;
using Gherkin.ViewModel;

namespace Gherkin.Util
{
    public class FailedToFindEditorControlArg : EventArgs
    {
        public FailedToFindEditorControlArg(EditorView editorView)
        {
            EditorView = editorView;
        }

        public EditorView EditorView { get; private set; }
    }
}
