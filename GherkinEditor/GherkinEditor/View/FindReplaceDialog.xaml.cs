using System.Text.RegularExpressions;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System.Media;
using Gherkin.Util;
using Gherkin.Model;
using System;

namespace Gherkin.View
{
    /// <summary>
    /// Original version from
    /// https://www.codeproject.com/Tips/768408/A-Find-and-Replace-Tool-for-AvalonEdit
    /// </summary>
    public partial class FindReplaceDialog : Window
    {
        private static FindReplaceDialog theDialog = null;
        private static int defaultTabMainSelectedIndex = 0;

        public FindReplaceDialog(TextEditor editor, IAppSettings appSettings)
        {
            InitializeComponent();

            this.DataContext = new ViewModel.FindReplaceViewModel(editor, appSettings);
            this.Owner = Application.Current.MainWindow;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            defaultTabMainSelectedIndex = theDialog.tabMain.SelectedIndex;
            theDialog = null;
        }

        public static void ShowForReplace(TextEditor editor, IAppSettings appSettings)
        {
            if (theDialog == null)
            {
                theDialog = new FindReplaceDialog(editor, appSettings);
                theDialog.tabMain.SelectedIndex = defaultTabMainSelectedIndex;
                theDialog.Show();
                theDialog.Activate();
            }
            else
            {
                theDialog.tabMain.SelectedIndex = defaultTabMainSelectedIndex;
                theDialog.Activate();
            }

            if (!editor.TextArea.Selection.IsMultiline)
            {
                theDialog.txtFind.SelectAll();
                theDialog.txtFind2.SelectAll();
                theDialog.txtFind.Focus();
                theDialog.txtFind2.Focus();
            }
        }
    }
}
