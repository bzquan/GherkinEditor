using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Model;

namespace Gherkin.ViewModel
{
    public class GherkinKeywordsCurrent : NotifyPropertyChangedBase
    {
        private GherkinLanguage m_Language;
        private string m_Keywords;

        public GherkinKeywordsCurrent(GherkinLanguage lang)
        {
            m_Language = lang;
        }

        public bool IsSame(string twoLetterISOLanguageName)
        {
            return m_Language.key.StartsWith(twoLetterISOLanguageName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// return HTML of Gherkin keywords
        /// Set "UTF-8" to display Unicode in the WPF Webbrowser Control
        ///    <head>
        ///      <meta charset = "UTF-8" >
        ///    </head >
        /// Use div and nowrap to avoid wrapping contents
        /// </summary>
        public string KeywordsHtml
        {
            get
            {
                if (m_Keywords == null)
                {
                    BuildKeywords();
                }

                string html = @"<head><meta charset=""UTF-8""></head>
                                <body style='font-family=""Segoe UI""'>
                                 <div style='white-space:nowrap; display:inline;'>
                                   {0}
                                 </div>
                               </body>";
                return String.Format(html, m_Keywords);
            }
        }

        public string Language
        {
            get { return m_Language.native + "(" + m_Language.name + ")"; }
        }

        private void BuildKeywords()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<h3 style=""color:green"">")
              .Append("#language: " + m_Language.key)
              .Append("</h3>")
              .Append(@"<h3 style=""color:blue"">")
              .Append("Language: " + Language)
              .Append("</h3>")
              .Append(@"<span style=""font-size : 11pt"">")
              .Append(GenerateKeywords("Feature", m_Language.feature))
              .Append(GenerateKeywords("Background", m_Language.background))
              .Append(GenerateKeywords("Scenario", m_Language.scenario))
              .Append(GenerateKeywords("Scenario Outline", m_Language.scenarioOutline))
              .Append(GenerateKeywords("Examples", m_Language.examples))
              .Append(GenerateKeywords("Given", m_Language.given))
              .Append(GenerateKeywords("When", m_Language.when))
              .Append(GenerateKeywords("Then", m_Language.then))
              .Append(GenerateKeywords("And", m_Language.and))
              .Append(GenerateKeywords("But", m_Language.but))
              .Append("</span>");

            m_Keywords = sb.ToString();
        }

        private static string GenerateKeywords(string english_keyword, string[] natives)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(english_keyword)
              .Append(": ");

            foreach (string word in natives)
            {
                if (word != "* ")
                {
                    sb.Append(word.Trim())
                      .Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);    // remove last redundant ", "
            sb.Append("<br>");

            return sb.ToString();
        }
    }
}
