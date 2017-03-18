using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.ViewModel;

namespace Gherkin.Util
{
    public class HideScenarioIndexArg : EventArgs
    {
        public HideScenarioIndexArg(EditorView editorView)
        {
            EditorView = editorView;
        }

        public EditorView EditorView { get; private set; }
    }
}
