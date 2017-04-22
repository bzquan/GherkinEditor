using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Gherkin.Model
{
    public static class GherkinUtil
    {
        public const string DEFAULT_LANGUAGE = "en";
        public const string GherkinHighlightingBaseName = "GherkinHighlighting";
        public const string FEATURE_EXTENSION = ".feature";

        static GherkinDialectProviderExtention s_GherkinDialectcs = new GherkinDialectProviderExtention();
        static List<string> s_LoadedHighlitings = new List<string>();
        private static TokenMatcher s_TokenMatcher = new TokenMatcher();

        public static bool IsSupported(string language)
        {
            return s_GherkinDialectcs.IsSupported(language);
        }

        public static bool IsGherkinHighlighting(string highlighingName)
        {
            return (highlighingName != null) &&
                    highlighingName.StartsWith(GherkinHighlightingBaseName, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Get current Gherkin language setting by scanning first 5 lines
        /// </summary>
        /// <param name="document"></param>
        /// <returns>language defined in tag or English as default language</returns>
        public static string CurrentLanguage(ICSharpCode.AvalonEdit.Document.TextDocument document)
        {
            var location = new Ast.Location();
            ICSharpCode.AvalonEdit.Document.DocumentLine line = document.GetLineByNumber(1);
            while (line != null)
            {
                string line_text = document.GetText(line.Offset, line.TotalLength);
                Token token = new Token(new GherkinLine(line_text, line.LineNumber), location);
                try
                {
                    if (s_TokenMatcher.Match_Language(token) && IsSupported(token.MatchedText))
                    {
                        return token.MatchedText;
                    }
                    if (line.LineNumber > 5) return DEFAULT_LANGUAGE;
                    line = line.NextLine;
                }
                catch
                {
                    return DEFAULT_LANGUAGE;
                }
            }
            return DEFAULT_LANGUAGE;
        }

        public static void RegisterGherkinHighlighting(string language = "en")
        {
            // Load Gherkin highlighting definition
            if (!IsSupported(language)) language = "en";

            if (!IsGherkinHighlightingRegistered(language))
            {
                IHighlightingDefinition gherkinHighlighting;
                string xshd_file = "Gherkin.View.GherkinHighlighting." + GherkinHighlightingName(language) + ".xshd";
                using (Stream s = typeof(View.MainWindow).Assembly.GetManifestResourceStream(xshd_file))
                {
                    if (s == null)
                        throw new InvalidOperationException("Could not find embedded resource");
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        gherkinHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
                s_LoadedHighlitings.Add(language);

                // and register it in the HighlightingManager
                HighlightingManager.Instance.RegisterHighlighting(GherkinHighlightingName(language), new string[] { FEATURE_EXTENSION }, gherkinHighlighting);
            }
        }

        public static string GherkinHighlightingName(string language)
            => GherkinHighlightingBaseName + "_" + language;

        private static bool IsGherkinHighlightingRegistered(string language)
        {
            return s_LoadedHighlitings.Contains(language);
        }

        public static bool IsFeatureFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return HasExtension(filePath, FEATURE_EXTENSION);
        }

        public static bool HasExtension(string filePath, string ext) =>
            filePath.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase);
    }
}
