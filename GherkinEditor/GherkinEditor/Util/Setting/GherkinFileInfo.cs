using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class GherkinFileInfo
    {
        public string FilePath = "";
        public string FontFamilyName = "KaiTi";
        public string FontSize = "11";
        public int CursorLine = 1;
        public int CursorColumn = 1;
        public int CodePage = 0;    // undefined: use automatic detection
    }
}
