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
    /// Interaction logic for DocumentPreviewWindow.xaml
    /// To use the WindowsFormsHost and controls from WinForms, you need to add a reference to the following assemblies in your application:
    /// WindowsFormsIntegration
    /// System.Windows.Forms
    /// </summary>
    public partial class DocumentPreviewWindow : Window
    {
        public DocumentPreviewWindow(DocumentPreviewViewModel viewModel)
        {
            InitializeComponent();
            Width = 1024;
            Height = 768;

            viewModel.PreviewHandlerHostControl = previewHandlerHostControlInstance;
            this.DataContext = viewModel;
            this.Owner = Application.Current.MainWindow;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            previewHandlerHostControlInstance.CloseFileStream();
            this.Owner = null;
        }
    }
}
