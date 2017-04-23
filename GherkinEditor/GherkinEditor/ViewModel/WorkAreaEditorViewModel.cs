using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Editing;
using System.IO;

namespace Gherkin.ViewModel
{
    public class WorkAreaEditorViewModel : NotifyPropertyChangedBase
    {
        private GherkinEditor m_WorkAreaEditor;
        private IAppSettings m_AppSettings;
        private MultiFileOpener m_MultiFilesOpener;

        public WorkAreaEditorViewModel(TextEditor editor, MultiFileOpener multiFilesOpener, IAppSettings appSettings)
        {
            m_WorkAreaEditor = new GherkinEditor(editor,
                                                 appSettings,
                                                 new FontFamily(appSettings.FontFamilyName),
                                                 appSettings.FontSize);
            editor.TextArea.CopyFileToHandler = CopyFileTo;
            editor.Options.RequireControlModifierForHyperlinkClick = false;     // Directly open web browser for work area instead of using Ctrl key
            m_MultiFilesOpener = multiFilesOpener;
            m_AppSettings = appSettings;

            m_WorkAreaEditor.TextEditor.Options.ContainOpenableFilePath = ContainOpenableFilePath;
            m_WorkAreaEditor.TextEditor.Options.FilePathClickedHandler = OnFilePathClickedHandler;
        }

        public ICommand ShowMessageWindowCmd => new DelegateCommandNoArg(OnShowMessageWindow);
        public ICommand HideMessageWindowCmd => new DelegateCommandNoArg(OnHideMessageWindow);

        private bool ContainOpenableFilePath(string text, out int start_offset, out int lenth)
        {
            return GreppedFileOpener.ContainOpenableFilePath(text, out start_offset, out lenth);
        }

        private void OnFilePathClickedHandler(string filePath)
        {
            GreppedFileOpener.TryOpenGreppedFile(m_MultiFilesOpener, filePath);
        }

        public bool HideMessageWindow
        {
            get { return !ShowMessageWindow; }
        }

        public bool ShowMessageWindow
        {
            get { return m_AppSettings.ShowMessageWindow; }
            set
            {
                if (m_AppSettings.ShowMessageWindow == value) return;
                m_AppSettings.ShowMessageWindow = value;
                base.OnPropertyChanged(nameof(MessageWindowIcon));
                base.OnPropertyChanged(nameof(HideMessageWindow));
                base.OnPropertyChanged(nameof(HideMessageWindow));
                base.OnPropertyChanged();
            }
        }

        private void OnShowMessageWindow()
        {
            ShowMessageWindow = true;
        }

        private void OnHideMessageWindow()
        {
            ShowMessageWindow = false;
        }

        public DrawingImage MessageWindowIcon
        {
            get
            {
                if (ShowMessageWindow)
                    return Util.Util.DrawingImageByOverlapping("MessageWindow.png", "Tick64.png");
                else
                    return Util.Util.DrawingImageFromResource("MessageWindow.png");
            }
        }

        public void SetSearchHighlightingTransformer(Regex regex)
        {
            var searchHighlightingTransformer = m_WorkAreaEditor.GetSearchHighlightingTransformer();
            searchHighlightingTransformer.SearchRegex = regex;
        }

        public string MessageOfGenCPPTestCode
        {
            set
            {
                Clear();
                ShowMessageWindow = true;
                m_WorkAreaEditor.AppendText(value);
            }
        }

        private void Clear()
        {
            m_WorkAreaEditor.ChangeToEmptyFile();
        }

        public void AppendText(string text)
        {
            m_WorkAreaEditor.AppendText(text);
        }

        public void OnGrepStarted()
        {
            Clear();
            ShowMessageWindow = true;
        }

        public void OnGrepFinished()
        {
            m_WorkAreaEditor.ScrollCursorTo(1, 1);
        }

        private void CopyFileTo(string filePath)
        {
            var dialog = new WPFFolderBrowser.WPFFolderBrowserDialog();
            dialog.FileName = m_AppSettings.LastFolderToCopyFile;

            if (dialog.ShowDialog() != true) return;

            try
            {
                m_AppSettings.LastFolderToCopyFile = dialog.FileName;
                string dstPath = Path.Combine(dialog.FileName, Path.GetFileName(filePath));
                File.Copy(filePath, dstPath, overwrite: true);

                string copyResultMsg = string.Format(Properties.Resources.Message_CopyFileToResult, dstPath);
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(copyResultMsg));
            }
            catch (Exception ex)
            {
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
            }
        }
    }
}
