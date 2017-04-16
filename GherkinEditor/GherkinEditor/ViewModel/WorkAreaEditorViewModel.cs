using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Gherkin.View;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Gherkin.Model;

namespace Gherkin.ViewModel
{
    public class WorkAreaEditorViewModel : NotifyPropertyChangedBase
    {
        private GherkinEditor m_WorkAreaEditor;
        private IAppSettings m_AppSettings;
        private bool m_ShowGrepResultUsagePopup;
        private MultiFileOpener m_MultiFilesOpener;

        public WorkAreaEditorViewModel(TextEditor editor, MultiFileOpener multiFilesOpener, IAppSettings appSettings)
        {
            m_WorkAreaEditor = new GherkinEditor(editor,
                                                 appSettings,
                                                 new FontFamily(appSettings.FontFamilyName),
                                                 appSettings.FontSize);
            m_MultiFilesOpener = multiFilesOpener;
            m_AppSettings = appSettings;
            m_WorkAreaEditor.TextEditor.TextArea.MouseDoubleClick += OnGrepLineDoubleClick;
            EventAggregator<WindowDeactivatedArg>.Instance.Event += OnWindowDeactivatedEvent;
        }

        public ICommand CloseGrepResultUagePopupCmd => new DelegateCommandNoArg(OnCloseGrepResultUagePopup);

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
                ShowGrepResultUsagePopup = false;
                base.OnPropertyChanged(nameof(MessageWindowIcon));
                base.OnPropertyChanged(nameof(HideMessageWindow));
                base.OnPropertyChanged(nameof(HideMessageWindow));
                base.OnPropertyChanged();
            }
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
                ShowGrepResultUsagePopup = false;
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

        public bool ShowGrepResultUsagePopup
        {
            get { return m_ShowGrepResultUsagePopup; }
            set
            {
                m_ShowGrepResultUsagePopup = value;
                base.OnPropertyChanged();
            }
        }

        private void OnWindowDeactivatedEvent(object sender, WindowDeactivatedArg arg)
        {
            ShowGrepResultUsagePopup = false;
        }

        private void OnCloseGrepResultUagePopup()
        {
            ShowGrepResultUsagePopup = false;
        }

            /// <summary>
        /// Open an editor to display double clicked file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGrepLineDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int line_no = m_WorkAreaEditor.CursorLine;
            string current_line = GherkinFormatUtil.GetText(m_WorkAreaEditor.Document, m_WorkAreaEditor.Document.GetLineByNumber(line_no));
            string grep_line = current_line.Trim();
            GreppedFileOpener.TryOpenGreppedFile(m_MultiFilesOpener, grep_line);
        }

        public void OnGrepStarted()
        {
            Clear();
            ShowMessageWindow = true;
            ShowGrepResultUsagePopup = true;
        }

        public void OnGrepFinished()
        {
            m_WorkAreaEditor.ScrollCursorTo(1, 1);
        }
    }
}
