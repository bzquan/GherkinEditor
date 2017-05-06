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

        private static IAppSettings s_AppSettings;

        public HelpViewModel(IAppSettings appSettings)
        {
            s_AppSettings = appSettings;
        }

        public string HelpHtml
        {
            get { return s_HelpHTML.Value; }
        }

        private static string LoadHelpHTML()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.ViewModel.ReleaseNote." + GetHTMLFileName();

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetHTMLFileName()
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
    }
}
