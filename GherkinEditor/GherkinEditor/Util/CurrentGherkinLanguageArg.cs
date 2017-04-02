using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class CurrentGherkinLanguageArg : EventArgs
    {
        public CurrentGherkinLanguageArg(string language_key)
        {
            LanguageKey = language_key.Trim();
        }

        public string LanguageKey { get; private set; }
    }
}
