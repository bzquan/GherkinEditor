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
using Gherkin.Model;
using Gherkin.ViewModel;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for GrepDialog.xaml
    /// </summary>
    public partial class GrepDialog : Window
    {
        public GrepDialog(GrepViewModel viewModel)
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            this.DataContext = viewModel;

            // WPF and initial focus
            // I just need to add the following code to my Window's constructor
        }

        /// <summary>
        /// WPF and initial focus : Need to add the following code to Window's constructor
        /// Note:
        /// To dig through Reflector to see where the Focusable property is used, and
        /// this will automatically select the first control in the tab order. 
        /// So it's a general solution that should be able to be dropped into any window and just Work.
        /// </summary>
        private void SetDefaultFocus()
        {
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
