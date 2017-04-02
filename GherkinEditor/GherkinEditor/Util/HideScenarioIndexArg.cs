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
        public HideScenarioIndexArg(EditorTabContentViewModel editorViewModel)
        {
            EditorViewModel = editorViewModel;
        }

        public EditorTabContentViewModel EditorViewModel { get; private set; }
    }
}
