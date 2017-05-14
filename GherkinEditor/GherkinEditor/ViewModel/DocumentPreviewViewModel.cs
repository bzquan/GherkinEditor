using Gherkin.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using C4F.DevKit.PreviewHandler.PreviewHandlerHost;
using System.Diagnostics;
using System.Windows;

namespace Gherkin.ViewModel
{
    public class DocumentPreviewViewModel : NotifyPropertyChangedBase
    {
        private string m_FilePath;

        public ICommand OpenPreviewDocumentCmd => new DelegateCommandNoArg(OnPreviewDocument);

        public PreviewHandlerHostControl PreviewHandlerHostControl { get; set; }

        public string PreviewTitle
        {
            get
            {
                if (string.IsNullOrEmpty(m_FilePath))
                    return Properties.Resources.Preview_Title;
                else
                {
                    var fileName = System.IO.Path.GetFileName(m_FilePath);
                    return Properties.Resources.Preview_Title + " - (" + fileName + ")";
                }
            }
        }

        public string FilePath
        {
            get { return m_FilePath; }
            set
            {
                try
                {
                    m_FilePath = value;
                    base.OnPropertyChanged(nameof(PreviewTitle));
                    base.OnPropertyChanged();

                    PreviewHandlerHostControl.FilePath = value;
                }
                catch(Exception ex)
                {
                    string message = ex.Message + "\n Previewing by native apllication";
                    MessageBox.Show(message, "Fail to preview document", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowPreviewByNativeApplication(m_FilePath);
                }
            }
        }

        private void OnPreviewDocument()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.FilterIndex = 1;
            dlg.Filter = "PDF File(*.pdf)|*.pdf|XPS File(*.xps)|*.xps|RTF File(*.rtf)|*.rtf|Word File(*.doc;docx)|*.doc;*.docx|All Files (*.*)|*.*";
            bool? result = dlg.ShowDialog();
            if ((result == true) && (dlg.FileNames.Length > 0))
            {
                FilePath = dlg.FileName;
            }
        }

        private void ShowPreviewByNativeApplication(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
