using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class EditorSettings
    {
        public bool ShowColumnRuler { get; set; } = true;
        public int ColumnRulerPositon { get; set; } = 100;
        public bool SupportUnicode { get; set; }
        public bool IsCloseTablesFoldingByDefault { get; set; }
        public bool IsCloseScenarioFoldingByDefault { get; set; }
        public bool ShowScenarioIndexByDefault { get; set; }
        public bool ShowSplitHorizontalViewByDefault { get; set; }
        public bool ShowSplitVerticalViewByDefault { get; set; }
        public bool HighlightCurrentLine { get; set; } = true;
        public bool ShowCurrentLineBorder { get; set; } = true;

        public bool ConvertTabsToSpaces { get; set; } = true;
        public int IndentationSize { get; set; } = 4;
        public bool ShowEndOfLine { get; set; }
        public bool ShowSpaces { get; set; }
        public bool ShowTabs { get; set; }
        public bool WordWrap { get; set; }
        public bool RequireControlModifierForHyperlinkClick { get; set; }
    }
}
