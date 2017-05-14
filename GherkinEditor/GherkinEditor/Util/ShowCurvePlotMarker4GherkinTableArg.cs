using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class ShowCurvePlotMarker4GherkinTableArg : EventArgs
    {
        public ShowCurvePlotMarker4GherkinTableArg(bool showMarker)
        {
            ShowMarker = showMarker;
        }

        public bool ShowMarker { get; private set; }
    }
}
