using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class GraphvizFontnameChangedArg : EventArgs
    {
        public GraphvizFontnameChangedArg(string fontname4unicode)
        {
            Fontname4Unicode = fontname4unicode;
        }

        public string Fontname4Unicode { get; private set; }
    }
}
