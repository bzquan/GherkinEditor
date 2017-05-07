using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gherkin.ViewModel;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for GherkinKeywords.xaml
    /// </summary>
    public partial class GherkinKeywords : Window
    {
        private GherkinKeywordsViewModel m_ViewModel;

        public GherkinKeywords(GherkinKeywordsViewModel viewModel)
        {
            InitializeComponent();

            m_ViewModel = viewModel;
            this.Owner = Application.Current.MainWindow;
            this.DataContext = viewModel;
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.gherkinLanguagesList.ScrollIntoView(m_ViewModel.SelectedItem);
            Keyboard.Focus(gherkinLanguagesList);
        }
    }
}
