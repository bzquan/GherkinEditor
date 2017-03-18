using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public class GherkinLanguage
    {
        public string key;
        public string name;
        public string native;
        public string[] feature;
        public string[] background;
        public string[] scenario;
        public string[] scenarioOutline;
        public string[] examples;
        public string[] given;
        public string[] when;
        public string[] then;
        public string[] and;
        public string[] but;
    }

    public class GherkinDialectProviderExtention : GherkinDialectProvider
    {
        static Dictionary<string, GherkinLanguageSetting> s_LanguageDictionary;

        Dictionary<string, GherkinLanguageSetting> LanguageDictionary
        {
            get
            {
                if (s_LanguageDictionary == null)
                    s_LanguageDictionary = base.LoadLanguageSettings();

                return s_LanguageDictionary;
            }
        }

        public bool IsSupported(string language)
        {
            GherkinLanguageSetting setting;
            return LanguageDictionary.TryGetValue(language, out setting);
        }

        public GherkinLanguage[] GherkinLanguages
        {
            get
            {
                List<GherkinLanguage> languages = new List<GherkinLanguage>();
                foreach (var v in LanguageDictionary)
                {
                    GherkinLanguageSetting setting = v.Value;
                    GherkinLanguage language = new GherkinLanguage();

                    language.key = v.Key;
                    language.name = setting.name;
                    language.native = setting.native;
                    language.feature = setting.feature;
                    language.background = setting.background;
                    language.scenario = setting.scenario;
                    language.scenarioOutline = setting.scenarioOutline;
                    language.examples = setting.examples;
                    language.given = setting.given;
                    language.when = setting.when;
                    language.then = setting.then;
                    language.and = setting.and;
                    language.but = setting.but;

                    languages.Add(language);
                }

                return languages.ToArray();
            }
        }

        public GherkinDialect GetCurrentDialect(string language)
        {
            var location = new Ast.Location();
            try
            {
                return base.GetDialect(language, location);
            }
            catch
            {
                return base.DefaultDialect;
            }
        }
    }
}
