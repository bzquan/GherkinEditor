using Gherkin.ViewModel;
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

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for EditorSetting.xaml
    /// http://wpftoolkit.codeplex.com/wikipage?title=PropertyGrid&referringTitle=Home
    /// </summary>
    public partial class EditorSetting : Window
    {
        SettingPropertyGridViewModel m_ViewModel;

        public EditorSetting(SettingPropertyGridViewModel viewModel = null)
        {
            InitializeComponent();

            m_ViewModel = viewModel;
            this.Owner = Application.Current.MainWindow;
            this.DataContext = m_ViewModel;

            resetHighlightingColors.Command = m_ViewModel.GetResetHighlightingColorsCmd();
        }

        private void OnResetSelectedProperty(object sender, RoutedEventArgs e)
        {
            string property = editorPropertyGrid.SelectedProperty.ToString();
            m_ViewModel.ResetProperty(property);
        }
    }
 }
