using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public class GherkinKeyword
    {
        public GherkinKeyword(string text, TokenType type)
        {
            Text = text;
            Type = type;
        }

        public string Text { get; private set; }
        public TokenType Type { get; private set; }

        /// <summary>
        /// Check if a text is a step keyword.
        /// We need trim the text when comparing because a step keyword may have white space at the end
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public static bool IsStepKeyword(string text, string[] keywords)
        {
            string word = text.Trim();
            foreach (string keyword in keywords)
            {
                if (word == keyword.Trim()) return true;
            }

            return false;
        }

        public bool IsLanguage => (Type == TokenType.Language);
        public bool IsFeature => (Type == TokenType.FeatureLine);
        public bool IsBackground => (Type == TokenType.BackgroundLine);
        public bool IsScenario => (Type == TokenType.ScenarioLine);
        public bool IsScenarioOutline => (Type == TokenType.ScenarioOutlineLine);
        public bool IsExample => (Type == TokenType.ExamplesLine);

        public bool IsKeyword
        {
            get
            {
                return IsFeature || IsBackground || IsScenario || IsScenarioOutline || IsExample;
            }
        }

        public bool IsStepKeyword() => (Type == TokenType.StepLine);
    }
}
