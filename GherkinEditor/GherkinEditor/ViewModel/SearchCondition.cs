using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    /// <summary>
    /// Search text condition
    /// </summary>
    public class SearchCondition
    {
        public string TextToFind { get; set; }
        public bool IsCaseSensitive { get; set; }
        public bool IsMatchWholeWord { get; set; }
        public bool IsUseRegex { get; set; }
        public bool IsUseWildcards { get; set; }
        public bool IsSearchUp { get; set; }

        public bool IsValidTextToFind()
        {
            if (string.IsNullOrEmpty(TextToFind)) return false;

            if (IsUseRegex)
            {
                try
                {
                    Regex.Match("", TextToFind);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Create simple regex withoud considering serch condisitons
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Regex GetSimpleRegEx(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            string pattern = Regex.Escape(text);
            return new Regex(pattern);
        }

        /// <summary>
        /// Create regex to searching under the search condition
        /// </summary>
        /// <param name="isForHighlighting">if true then RegexOptions.RightToLeft is not used</param>
        /// <param name="leftToRight">use RegexOptions.RightToLeft for non highlighting</param>
        /// <returns></returns>
        public Regex GetRegEx(bool isForHighlighting, bool leftToRight = false)
        {
            RegexOptions options = RegexOptions.None;
            if (IsSearchUp && !leftToRight && !isForHighlighting)
                options |= RegexOptions.RightToLeft;
            if (!IsCaseSensitive)
                options |= RegexOptions.IgnoreCase;

            if (IsUseRegex)
            {
                return new Regex(TextToFind, options);
            }
            else
            {
                string pattern = Regex.Escape(TextToFind);
                if (IsUseWildcards)
                    pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
                if (IsMatchWholeWord)
                    pattern = "\\b" + pattern + "\\b";
                return new Regex(pattern, options);
            }
        }
    }
}
