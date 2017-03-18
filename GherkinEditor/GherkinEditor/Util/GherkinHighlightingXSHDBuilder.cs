using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Gherkin.Model;

namespace Gherkin.Util
{
    public static class GherkinHighlightingXSHDBuilder
    {
        static GherkinDialectProviderExtention s_GherkinDialectcs = new GherkinDialectProviderExtention();

        public static void CreateGherkinHighlightingFiles()
        {
            string xshd_template = LoadGherkinHighlightingTemplate();
            GherkinLanguage[] languages = s_GherkinDialectcs.GherkinLanguages;
            foreach (var language in languages)
            {
                string content = MakeGherkinHighlightingContent(xshd_template, language);
                WriteTextFile(language.key, content);
            }

            string msg = string.Format("{0} Gherkin highliting xshd files are saved in {1}", languages.Length, Util.StartupFolder());
            EventAggregator<StatusChangedArg>.Instance.Publish(null, new StatusChangedArg(msg));
        }

        private static string LoadGherkinHighlightingTemplate()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.View.GherkinHighlighting." + GherkinUtil.GherkinHighlightingBaseName + "_template.xshd";

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string MakeGherkinHighlightingContent(string xshd_template, GherkinLanguage language)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>")
              .AppendLine("<SyntaxDefinition name=\"" + GherkinUtil.GherkinHighlightingName(language.key) + "\" xmlns=\"http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008\">")
              .AppendLine(xshd_template)

              .AppendLine(MakeGherkinKeyword(language))
              .AppendLine()
              .AppendLine(MakeStepKeyword(language))

              .AppendLine("  </RuleSet>")
              .AppendLine("</SyntaxDefinition>");

            return sb.ToString();
        }

        private static string MakeGherkinKeyword(GherkinLanguage language)
        {
            // Sample
            // \s*(Feature|Background|Scenario|Scenarios|Examples|Scenario\s+Outline|Scenario\s+Template)\s*:

            StringBuilder sb = new StringBuilder();
            sb
              .AppendLine("    <Rule color=\"Keyword\">")
              .Append("      \\s*(")
              .Append(Keyword(language.feature))
              .Append(Keyword(language.background))
              .Append(Keyword(language.scenario))
              .Append(Keyword(language.scenarioOutline))
              .Append(Keyword(language.examples));
            sb.Remove(sb.Length - 1, 1); // remove last "|"
            sb.AppendLine(")\\s*:")
              .AppendLine("    </Rule>");

            return sb.ToString();
        }

        private static string MakeStepKeyword(GherkinLanguage language)
        {
            StringBuilder sb = new StringBuilder();
            sb
              .AppendLine("    <Keywords color=\"StepWord\">")
              .Append(StepKeyword(language.given))
              .Append(StepKeyword(language.when))
              .Append(StepKeyword(language.then))
              .Append(StepKeyword(language.and))
              .Append(StepKeyword(language.but))
              .AppendLine("    </Keywords>");

            return sb.ToString();
        }

        private static void WriteTextFile(string language_key, string content)
        {
            string appDirectory = Util.StartupFolder();
            string file = GherkinUtil.GherkinHighlightingName(language_key) + ".xshd";
            string outputFile = Path.Combine(appDirectory, file);
            File.WriteAllText(outputFile, content, Encoding.UTF8);
        }

        private static Regex MultiSpaceRegex = new Regex("[ ]{2,}", RegexOptions.None);

        private static string Keyword(string[] words)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string word in words)
            {
                if (word != "* ")
                {
                    string str = MultiSpaceRegex.Replace(word.Trim(), " "); // Replace multiple spaces with a single space
                    sb
                       .Append(str.Replace(" ", "\\s+"))
                       .Append("|");
                }
            }

            return sb.ToString();
        }

        private static string StepKeyword(string[] words)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string word in words)
            {
                if (word != "* ")
                {
                    sb.Append("      <Word>  ")
                      .Append(MultiSpaceRegex.Replace(word.Trim(), " "))  // Replace multiple spaces with a single space
                      .AppendLine(" </Word>");
                }
            }

            return sb.ToString();
        }
    }
}
