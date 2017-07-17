using System;
using System.IO;
using Gherkin.Util;

namespace Gherkin.ViewModel
{
    public class HelpViewModel : NotifyPropertyChangedBase
    {
        static Lazy<string> s_HelpHTML = new Lazy<string>(() => LoadHelpHTML());
        static Lazy<string> s_TableGridFormulaHTML = new Lazy<string>(() => LoadTableGridFormulaHTML());
        static Lazy<string> s_GraphvizHTML = new Lazy<string>(() => LoadGraphvizHTML());

        private static IAppSettings s_AppSettings;
        private HelpKind m_HelpKind;

        public enum HelpKind { BasicHelp, TableGridFormulaHelp, GraphvizHelp }

        public HelpViewModel(IAppSettings appSettings, HelpKind helpKind)
        {
            s_AppSettings = appSettings;
            m_HelpKind = helpKind;
        }

        public string HelpTitle
        {
            get
            {
                switch (m_HelpKind)
                {
                    case HelpKind.BasicHelp:
                        return Properties.Resources.MenuHelp_GherkinHelp;
                    case HelpKind.TableGridFormulaHelp:
                        return Properties.Resources.MenuHelp_TableGridFormula;
                    case HelpKind.GraphvizHelp:
                        return Properties.Resources.MenuHelp_Graphviz;
                    default:
                        return Properties.Resources.MenuHelp_GherkinHelp;
                }
            }
        }

        public string HelpHtml
        {
            get
            {
                switch (m_HelpKind)
                {
                    case HelpKind.BasicHelp:
                        return s_HelpHTML.Value;
                    case HelpKind.TableGridFormulaHelp:
                        return s_TableGridFormulaHTML.Value;
                    case HelpKind.GraphvizHelp:
                        return s_GraphvizHTML.Value;
                    default:
                        return s_HelpHTML.Value;
                }
            }
        }

        private static string LoadHelpHTML() => LoadHTML(GetHelpHTMLFileName());
        private static string LoadTableGridFormulaHTML() => LoadHTML(GetTableGridFormulaHTMLFileName());
        private static string LoadGraphvizHTML() => LoadHTML(GetGraphvizHTMLFileName());

        private static string LoadHTML(string filename)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.ViewModel.ReleaseNote." + filename;

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetHelpHTMLFileName() => "Help" + LanguageSuffix() + ".html";
        private static string GetTableGridFormulaHTMLFileName() => "TableGridFormula" + LanguageSuffix() + ".html";
        private static string GetGraphvizHTMLFileName() => "Graphviz" + LanguageSuffix() + ".html";

        private static string LanguageSuffix()
        {
            switch (s_AppSettings.Language)
            {
                case Languages.Chinese:
                    return "_zh-CN";
                case Languages.Japanese:
                    return "_ja";
                default:
                    return "";
            }
        }
    }
}
