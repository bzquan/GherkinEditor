using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class GherkinColors
    {
        public string ColorOfFoldingText { get; set; } = "Blue";
        public string ColorOfHighlightingKeyword { get; set; } = "Blue";
        public string ColorOfHighlightingStepWord { get; set; } = "DarkCyan";
        public string ColorOfHighlightingTag { get; set; } = "Purple";
        public string ColorOfHighlightingTable { get; set; } = "Peru";
        public string ColorOfHighlightingDocString { get; set; } = "Peru";
        // Digits, string, table cell
        public string ColorOfHighlightingConstants { get; set; } = "Peru";
        public string ColorOfHighlightingMockAttribute { get; set; } = "Green";
    }
}
