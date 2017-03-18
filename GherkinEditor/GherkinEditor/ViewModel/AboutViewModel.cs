using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            vm.ApplicationLogo = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("pack://application:,,,/View/Images/Gherkin.png"));
            vm.Title = Properties.Resources.Message_AboutGherkinTitle;
            vm.Description = Properties.Resources.Message_AboutGherkinDescription;
            vm.Version = "1.0.2";
            vm.PublisherLogo = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("pack://application:,,,/View/Images/Feature.png"));
            vm.ReleaseNote = LoadReleaseNote();
            vm.HyperlinkText = "https://github.com/bzquan";
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
    }
}
