using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Model;
using System.IO;

namespace Gherkin.Util
{
    public class GherkinKeywordGenerator
    {
        static Lazy<GherkinDialectProviderExtention> s_GherkinDialectcs = new Lazy<GherkinDialectProviderExtention>(() => new GherkinDialectProviderExtention());

        public static void GenerateKeywords()
        {
            GherkinLanguage[] languages = s_GherkinDialectcs.Value.GherkinLanguages;

            Array.Sort(languages, delegate(GherkinLanguage lang1, GherkinLanguage lang2) {
                return string.Compare(lang1.name, lang2.name, StringComparison.Ordinal);
            });

            StringBuilder sb = new StringBuilder();
            foreach (var language in languages)
            {
                GenerateKeywords(sb, language);
            }
            WriteTextFile(sb.ToString());

            string msg = string.Format("Gherkin languages{0} specific keywords are saved in {1}", languages.Length, OutputFilePath());
            EventAggregator<StatusChangedArg>.Instance.Publish(null, new StatusChangedArg(msg));
        }

        public static string GenerateKeywordsToolTip(string key)
        {
            GherkinDialect dialect = s_GherkinDialectcs.Value.GetCurrentDialect(key);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Language: " + key)
              .AppendLine(GenerateKeywords("Feature", dialect.FeatureKeywords))
              .AppendLine(GenerateKeywords("Background", dialect.BackgroundKeywords))
              .AppendLine(GenerateKeywords("Scenario", dialect.ScenarioKeywords))
              .AppendLine(GenerateKeywords("Scenario Outline", dialect.ScenarioOutlineKeywords))
              .AppendLine(GenerateKeywords("Examples", dialect.ExamplesKeywords))
              .AppendLine(GenerateKeywords("Given", dialect.GivenStepKeywords))
              .AppendLine(GenerateKeywords("When", dialect.WhenStepKeywords))
              .AppendLine(GenerateKeywords("Then", dialect.ThenStepKeywords))
              .AppendLine(GenerateKeywords("And", dialect.AndStepKeywords))
              .Append(GenerateKeywords("But", dialect.ButStepKeywords));

            return sb.ToString();
        }

        private static void GenerateKeywords(StringBuilder sb, GherkinLanguage language)
        {
            sb.AppendLine("======================================")
              .Append("#language: ")
              .AppendLine(language.key)
              .Append("Language: ")
              .AppendLine(language.name + ", " + language.native)
              .AppendLine("======================================")
              .AppendLine(GenerateKeywords("Feature", language.feature))
              .AppendLine(GenerateKeywords("Background", language.background))
              .AppendLine(GenerateKeywords("Scenario", language.scenario))
              .AppendLine(GenerateKeywords("Scenario Outline", language.scenarioOutline))
              .AppendLine(GenerateKeywords("Examples", language.examples))
              .AppendLine(GenerateKeywords("Given", language.given))
              .AppendLine(GenerateKeywords("When", language.when))
              .AppendLine(GenerateKeywords("Then", language.then))
              .AppendLine(GenerateKeywords("And", language.and))
              .AppendLine(GenerateKeywords("But", language.but))
              .AppendLine();
        }

        private static string GenerateKeywords(string english_keyword, string[] natives)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(english_keyword).Append(": ");

            foreach (string word in natives)
            {
                if (word != "* ")
                {
                    sb.Append(word.Trim())
                      .Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);    // remove last redundant ", "

            return sb.ToString();
        }

        private static void WriteTextFile(string content)
        {
            File.WriteAllText(OutputFilePath(), content, Encoding.UTF8);
        }

        private static string OutputFilePath()
        {
            string appDirectory = Util.StartupFolder();
            return Path.Combine(appDirectory, "GherkinKeyword.txt");
        }
    }
}
