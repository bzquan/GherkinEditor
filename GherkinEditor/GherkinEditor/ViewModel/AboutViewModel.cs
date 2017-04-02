using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class AboutViewModel : NotifyPropertyChangedBase
    {
        public ICommand AboutGherkinCmd => new DelegateCommandNoArg(OnAboutGherkin);

        /// <summary>
        /// https://aboutbox.codeplex.com/
        /// </summary>
        private void OnAboutGherkin()
        {
            View.AboutControlView about = new View.AboutControlView();
            AboutControlViewModel vm = (AboutControlViewModel)about.FindResource("ViewModel");

            vm.ApplicationLogo = Util.Util.DrawingImageFromResource("Gherkin.png");
            vm.Title = Properties.Resources.Message_AboutGherkinTitle;
            vm.Description = Properties.Resources.Message_AboutGherkinDescription;
            vm.PublisherLogo = Util.Util.DrawingImageFromResource("Feature.png");
            vm.ReleaseNote = LoadReleaseNote();
            vm.Version = ExtractVersionNoFromReleaseNote(vm.ReleaseNote);
            vm.HyperlinkText = "https://github.com/bzquan/GherkinEditor";
            vm.Window.Content = about;
            vm.Window.Show();
        }

        private string LoadReleaseNote()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.ViewModel.ReleaseNote.ReleaseNote.html";

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private string ExtractVersionNoFromReleaseNote(string releaseNote)
        {
            // Example: <h5>Version 1.0.2 - 2017.03.18</h5>
            Regex versionRegex = new Regex(@"\s*<h5>\s*Version\s*(\w+\.\w+\.\w+).*</h5>");
            Match m = versionRegex.Match(releaseNote);
            if (m.Success)
            {
                return m.Groups[1].ToString();
            }
            return "Unknown Version";
        }
    }
}
