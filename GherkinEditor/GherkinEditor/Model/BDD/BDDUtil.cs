using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CucumberCpp
{
    static class BDDUtil
    {
        static private TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;
        internal const string INDENT = "    ";
        internal const string INDENT_DOUBLE = INDENT + INDENT;
        internal const string INDENT_DOUBLE_PLUS = INDENT + INDENT + "  ";
        internal const string INDENT_TRIPLE = INDENT_DOUBLE + INDENT;
        internal const string INDENT_TABLE_HEADER = "//      ";
        internal const string ParameterClassName = "GherkinRow";

        internal const string IntRegexSymbol = "(_INT_)";
        internal const string IntRegex = @"([-+]?\d[\d,]*)";
        internal const string FloatRegexSymbol = "(_DOUBLE_)";
        internal const string FloatRegex = @"([-+]?\d[\d,]*\.?\d*)";
        internal const string StringRegexSymbol = "(_STR_)";
        internal const string StringRegex = @"(""[^""]*"")";
        internal const string RowParamRegex = @"(<[^>]+>)";
        internal const string MockAttrSymbol = "[[mock]]";
        internal const string MockAttrRegex = @"\[\[mock\]\]";

        const string NEW_LINE = "\n";
        internal const string TEST_METHOD_BODY =
                NEW_LINE +
                INDENT + "// Given" + NEW_LINE +
                NEW_LINE +
                INDENT + "// When" + NEW_LINE +
                NEW_LINE +
                INDENT + "// Then" + NEW_LINE +
                NEW_LINE +
                INDENT + "FAIL();" + NEW_LINE +
                "}" + NEW_LINE;

        static public bool SupportUnicode { get; set; } = false;

        static public string ReplaceWhiteSpaceWithUnderBar(string str)
        {
            return Regex.Replace(str, @"\s+", "_");
        }

        static public string RemoveAllWhiteSpaces(string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }

        static public string MakeIdentifier(string str)
        {
            str = Regex.Replace(str, @"[\s+]", "_");
            return Regex.Replace(str, @"\W", "");
        }

        static public string ToTitleCase(string str)
        {
            string titleCase = TitleCase.ToTitleCase(str);

            return Regex.Replace(titleCase, @"\s+", ""); // delete all white spaces
        }

        static public string ToLowerCaseWithUnderBar(string str)
        {
            string lowerCase = str.ToLower();
            return ReplaceWhiteSpaceWithUnderBar(lowerCase);
        }

        static public string ToCPPwstringLiteral(string str)
        {
            StringBuilder text = new StringBuilder();
            if (!str.StartsWith("L\"", System.StringComparison.InvariantCulture))
            {
                if (str.StartsWith("\"", System.StringComparison.InvariantCulture))
                {
                    text.Append("L");
                }
                else
                {
                    text.Append("L\"");
                }
            }
            text.Append(str);

            if (!str.EndsWith("\"", System.StringComparison.InvariantCulture))
            {
                text.Append("\"");
            }

            return text.ToString();
        }

        static public string to_ident(string name)
        {
            if (SupportUnicode) return name;

            // change wide character to unicode if unicode is not supported
            StringBuilder ident = new StringBuilder();
            foreach (char ch in name)
            {
                ident.Append(to_string(ch));
            }

            return ident.ToString();
        }

        static string to_string(char ch)
        {
            StringBuilder sb = new StringBuilder();
            if (ch < 128)
            {
                sb.Append(ch);
            }
            else
            {
                sb.Append('u')
                  .Append((int)ch)
                  .Append("uu");
            }

            return sb.ToString();
        }
    }
}
