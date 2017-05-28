using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;

namespace Gherkin.ViewModel
{
    public class HelpViewModel : NotifyPropertyChangedBase
    {
        static Lazy<string> s_HelpHTML = new Lazy<string>(() => LoadHelpHTML());
        static Lazy<string> s_TableGridFormulaHTML = new Lazy<string>(() => LoadTableGridFormulaHTML());
        private static IAppSettings s_AppSettings;
        private HelpKind m_HelpKind;

        public enum HelpKind { BasicHelp, TableGridFormulaHelp }

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
                    default:
                        return s_HelpHTML.Value;
                }
            }
        }

        private static string LoadHelpHTML()
        {
            return LoadHTML(GetHelpHTMLFileName());
        }

        private static string LoadTableGridFormulaHTML()
        {
            return LoadHTML(GetTableGridFormulaHTMLFileName());
        }

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

        private static string GetHelpHTMLFileName()
        {
            switch (s_AppSettings.Language)
            {
                case Languages.Chinese:
                    return "Help_zh-CN.html";
                case Languages.Japanese:
                    return "Help_ja.html";
                default:
                    return "Help.html";
            }
        }

        private static string GetTableGridFormulaHTMLFileName()
        {
            switch (s_AppSettings.Language)
            {
                case Languages.Chinese:
                    return "TableGridFormula_zh-CN.html";
                case Languages.Japanese:
                    return "TableGridFormula_ja.html";
                default:
                    return "TableGridFormula.html";
            }
        }
    }
}
