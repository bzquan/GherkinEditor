using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class RegexDocument : NotifyPropertyChangedBase
    {
        static Lazy<string> s_RegexNote = new Lazy<string>(() => LoadRegexNote());

        private bool m_IsShowingRegexHelp;
        private string m_RegexNote;

        public ICommand ShowRegexHelpCmd => new DelegateCommandNoArg(OnShowRegexHelp);
        public ICommand CloseRegexHelpCmd => new DelegateCommandNoArg(OnCloseRegexHelp);

        public RegexDocument()
        {
            m_RegexNote = LoadRegexNote();
        }

        private void OnShowRegexHelp() => IsShowingRegexHelp = true;
        private void OnCloseRegexHelp() => IsShowingRegexHelp = false;

        public bool IsShowingRegexHelp
        {
            get { return m_IsShowingRegexHelp; }
            set
            {
                m_IsShowingRegexHelp = value;
                base.OnPropertyChanged();
            }
        }

        public string RegexNote
        {
            get { return s_RegexNote.Value; }
        }

        private static string LoadRegexNote()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.ViewModel.ReleaseNote.RegexNote.html";

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
