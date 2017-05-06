using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;

namespace Gherkin.ViewModel
{
    public class WorkAreaEditorViewModel : NotifyPropertyChangedBase
    {
        private GherkinEditor m_WorkAreaEditor;
        private IAppSettings m_AppSettings;
        private MultiFileOpener m_MultiFilesOpener;
        private bool m_ShowCppCodeGenResultUsagePopup;
        private bool m_ShowMessageWindow;

        public event Action MessageWindowHiddenEvent;

        public WorkAreaEditorViewModel(TextEditor editor, MultiFileOpener multiFilesOpener, IAppSettings appSettings)
        {
            m_WorkAreaEditor = new GherkinEditor(editor,
                                                 null, // subEditor,
                                                 editor.Document,
                                                 appSettings,
                                                 new FontFamily(appSettings.FontFamilyName),
                                                 appSettings.FontSize,
                                                 installElementGenerators: false);
            editor.Options.CopyFileToHandler = CopyFileTo;
            editor.Options.RequireControlModifierForHyperlinkClick = false;     // Directly open web browser for work area instead of using Ctrl key
            m_MultiFilesOpener = multiFilesOpener;
            m_AppSettings = appSettings;

            m_WorkAreaEditor.TextEditor.Options.ContainOpenableFilePath = ContainOpenableFilePath;
            m_WorkAreaEditor.TextEditor.Options.FilePathClickedHandler = OnFilePathClickedHandler;

            EventAggregator<WindowDeactivatedArg>.Instance.Event += OnWindowDeactivatedEvent;
        }

        public ICommand ShowMessageWindowCmd => new DelegateCommandNoArg(OnShowMessageWindow, CanShowMessageWindow);
        public ICommand HideMessageWindowCmd => new DelegateCommandNoArg(OnHideMessageWindow, CanHideMessageWindow);
        public ICommand CloseCPPCodeGenResultUagePopupCmd => new DelegateCommandNoArg(OnCloseCPPCodeGenResultUsagePopup);

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
            get { return m_ShowMessageWindow; }
            set
            {
                m_ShowMessageWindow = value;
                if (m_ShowMessageWindow)
                    m_WorkAreaEditor.TextEditor.Focus();
                else
                    MessageWindowHiddenEvent?.Invoke();

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
            ShowCPPCodeGenResultUsagePopup = false;
            ShowMessageWindow = false;
        }

        private bool CanShowMessageWindow() => HideMessageWindow;

        private bool CanHideMessageWindow() => ShowMessageWindow;

        public void ShowSearchPanel()
        {
            if (ShowMessageWindow)
            {
                m_WorkAreaEditor.ShowSearchPanel();
            }
        }

        public void SetSearchHighlightingTransformer(Regex regex)
        {
            var searchHighlightingTransformer = m_WorkAreaEditor.GetSearchHighlightingTransformer();
            searchHighlightingTransformer.SearchRegex = regex;
        }

        public void ShowMessageOfGenCPPTestCode(string message, bool IsCodeGenerationSuccess)
        {
            Clear();
            ShowMessageWindow = true;
            m_WorkAreaEditor.TextEditor.Options.RequireControlModifierForHyperlinkClick = true;
            m_WorkAreaEditor.AppendText(message);

            if (IsCodeGenerationSuccess)
            {
                ShowCPPCodeGenResultUsagePopup = true;
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

        public bool ShowCPPCodeGenResultUsagePopup
        {
            get { return m_ShowCppCodeGenResultUsagePopup; }
            set
            {
                m_ShowCppCodeGenResultUsagePopup = value;
                base.OnPropertyChanged();
            }
        }

        private void OnWindowDeactivatedEvent(object sender, WindowDeactivatedArg arg)
        {
            ShowCPPCodeGenResultUsagePopup = false;
        }

        private void OnCloseCPPCodeGenResultUsagePopup()
        {
            ShowCPPCodeGenResultUsagePopup = false;
        }

        public void OnGrepStarted()
        {
            Clear();
            ShowMessageWindow = true;
        }

        public void OnGrepFinished()
        {
            m_WorkAreaEditor.TextEditor.Options.RequireControlModifierForHyperlinkClick = false;
            m_WorkAreaEditor.ScrollCursorTo(1, 1);
        }

        private void CopyFileTo(string filePath)
        {
            try
            {
                string defaultDestPath = Path.Combine(m_AppSettings.LastFolderToCopyFile, Path.GetFileName(filePath));
                string destPath = GetSavingFilePath(defaultDestPath);
                if (string.IsNullOrWhiteSpace(destPath)) return;

                m_AppSettings.LastFolderToCopyFile = Path.GetDirectoryName(destPath);
                File.Copy(filePath, destPath, overwrite: true);

                string copyResultMsg = string.Format(Properties.Resources.Message_CopyFileToResult, destPath);
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(copyResultMsg));
            }
            catch (Exception ex)
            {
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
            }
        }

        private string GetSavingFilePath(string defaultFilePath)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = Properties.Resources.Message_DlgCopyFileToTitle;
            dlg.FileName = Path.GetFileName(defaultFilePath);
            dlg.DefaultExt = Path.GetExtension(defaultFilePath);
            dlg.Filter = "C++ File(*.cpp)|*.cpp|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            bool? result = dlg.ShowDialog();

            return (result == true) ? dlg.FileName : null;
        }
    }
}
