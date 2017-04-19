using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Microsoft.Win32;

using ICSharpCode.AvalonEdit;
using Gherkin.Util;
using Gherkin.Model;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Gherkin.View;

namespace Gherkin.ViewModel
{
    public class GherkinViewModel : NotifyPropertyChangedBase
    {
        // Example: (12:0): detailed error message 
        private Regex m_ErrorMsgRegex = new Regex(@"\s*\((\d+):(\d+)\):.*");

        private string m_Status = "Ready";
        private int m_SelectedTabIndex;
        private EditorTabContentViewModel m_CurrentEditor;
        private IAppSettings m_AppSettings;
        private TabControl m_EditorTabControl;
        private MultiFileOpener m_MultiFilesOpener;

        public ObservableCollection<EditorTabItem> TabPanels { get; private set; } = new ObservableCollection<EditorTabItem>();

        public ICommand SaveAllCmd => new DelegateCommandNoArg(OnSaveAll);
        public ICommand OpenFileInNewWindowCmd => new DelegateCommandNoArg(OnOpenFileInNewWindow);
        public ICommand OpenRecentFileCmd => new DelegateCommand<string>(OnOpenRecentFile);
        public ICommand OpenCurrentFolderCmd => new DelegateCommandNoArg(OnOpenFolder);
        public ICommand OpenAllFoldingsCmd => new DelegateCommandNoArg(OnOpenAllFoldings);

        public ICommand SetGherkinHighlightingCmd => new DelegateCommandNoArg(OnSetGherkinHighlighting);
        public ICommand GenCPPTestCodeCmd => new DelegateCommandNoArg(OnGenCPPTestCode);
        public ICommand GrepCmd => new DelegateCommandNoArg(OnGrep);
        public ICommand ShowCodePageListCmd => new DelegateCommandNoArg(OnShowCodePageList);
        public FontViewModel FontViewModel { get; private set; }
        public GherkinSettingViewModel GherkinSettings { get; private set; }
        public AboutViewModel AboutViewModel { get; private set; }
        public CodePageListPopupViewModel CodePageListPopupViewModel { get; private set; }
        public GherkinViewModel(IAppSettings appSettings,
                                MultiFileOpener multiFilesOpener,
                                FontViewModel fontViewModel,
                                GherkinSettingViewModel gherkinSettings,
                                AboutViewModel aboutViewModel,
                                CodePageListPopupViewModel codePageListPopupViewModel)
        {
            m_AppSettings = appSettings;
            GherkinFormatUtil.AppSettings = appSettings;
            m_MultiFilesOpener = multiFilesOpener;
            m_MultiFilesOpener.OpeningTabEvent += OnOpeningTab;
            m_MultiFilesOpener.LoadFilesCompletedEvent += OnLoadFilesCompleted;

            FontViewModel = fontViewModel;
            GherkinSettings = gherkinSettings;
            GherkinSettings.TabPanels = TabPanels;
            AboutViewModel = aboutViewModel;
            CodePageListPopupViewModel = codePageListPopupViewModel;
            CodePageListPopupViewModel.CodePageChangedEvent += delegate { base.OnPropertyChanged(nameof(Codepage)); }; 

            GherkinSettings.ChangeFoldingTextColorOfAvalonEdit();
            EventAggregator<DeleteEditorTabRequestedArg>.Instance.Event += OnDeleteEditorTab;
            EventAggregator<DeleteAllEditorTabsRequestedArg>.Instance.Event += OnDeleteAllEditorTabsRequested;
            EventAggregator<RenameDocumentRequestedArg>.Instance.Event += OnRenameDocumentRequested;
            EventAggregator<SaveAsDocumentRequestedArg>.Instance.Event += OnSaveAsDocumentRequested;
            EventAggregator<StatusChangedArg>.Instance.Event += OnStatusChanged;
            EventAggregator<IndentationCompletedArg>.Instance.Event += OnIndentationCompleted;
            EventAggregator<FailedToFindEditorControlArg>.Instance.Event += OnFailedToFindEditorControl;
            EventAggregator<HideScenarioIndexArg>.Instance.Event += OnHideScenarioIndex;
            EventAggregator<FileSavedAsArg>.Instance.Event += OnFileSavedAs;
            EventAggregator<OpenNewGherkinEditorRequestedArg>.Instance.Event += OnOpenNewGherkinEditorRequested;
            EventAggregator<EditorLoadedArg>.Instance.Event += OnEditorLoaded;
            EventAggregator<FileLoadedArg>.Instance.Event += OnFileLoaded; ;
        }

        public TabControl EditorTabControl
        {
            get { return m_EditorTabControl; }
            set
            {
                m_EditorTabControl = value;
                m_MultiFilesOpener.Initialize(TabPanels, value);
                CreateEmptyTab(); // Create initial editor
            }
        }

        public WorkAreaEditorViewModel WorkAreaEditor { get; set; }

        public void SetMessageTextEditor(TextEditor editor)
        {
            WorkAreaEditor = new WorkAreaEditorViewModel(editor, m_MultiFilesOpener, m_AppSettings);
        }

        public void SaveLastSelectedFile()
        {
            m_AppSettings.LastSelectedFile = CurrentEditor?.CurrentFilePath;
        }

        public void SaveLastOpenedFileInfo()
        {
            List<string> openedFiles = new List<string>();
            foreach (var tab in TabPanels)
            {
                if (File.Exists(tab.EditorTabContentViewModel.CurrentFilePath))
                {
                    openedFiles.Add(tab.EditorTabContentViewModel.CurrentFilePath);
                }
            }

            m_AppSettings.LastOpenedFiles = openedFiles;
        }

        public int SelectedTabIndex
        {
            get { return m_SelectedTabIndex; }
            set
            {
                if ((value < 0) || (value >= TabPanels.Count)) return;

                m_SelectedTabIndex = value;
                CurrentEditor = TabPanels[value].EditorTabContentViewModel;
                base.OnPropertyChanged();
            }
        }

        public bool HasEditorLoaded => (MainTextEditor != null);
        private TextEditor MainTextEditor => CurrentEditor?.MainEditor;

        public void ShowFindReplace()
        {
            CurrentEditor?.ShowSearchPanel();
        }

        private EditorTabContentViewModel CurrentEditor
        {
            get { return m_CurrentEditor; }
            set
            {
                m_CurrentEditor = value;
                UpdateViewByCurrentEditor();
            }
        }

        private void UpdateViewByCurrentEditor()
        {
            ShowScenarioIndex = CurrentEditor?.HideScenarioIndex == false;
            ShowSplitView = CurrentEditor?.HideSplitView == false;

            base.OnPropertyChanged(nameof(CurrentFilePath));
            base.OnPropertyChanged(nameof(IsCloseTablesFolding));
            base.OnPropertyChanged(nameof(IsCloseScenarioFolding));
            base.OnPropertyChanged(nameof(CurrentFontFamily));
            base.OnPropertyChanged(nameof(CurrentFontSize));
            base.OnPropertyChanged(nameof(Codepage));
        }

        public string CurrentFilePath => CurrentEditor?.CurrentFilePath ?? "";

        private void OnFileSavedAs(object sender, FileSavedAsArg arg)
        {
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        public FontFamily CurrentFontFamily
        {
            get { return CurrentEditor?.FontFamily ?? new FontFamily(m_AppSettings.FontFamilyName); }
            set
            {
                string name = value.ToString();
                if (HasEditorLoaded)
                {
                    CurrentEditor.FontFamily = new FontFamily(name);
                }
                base.OnPropertyChanged();
            }
        }

        public string CurrentFontSize
        {
            get { return CurrentEditor?.FontSize ?? m_AppSettings.FontSize; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.FontSize = value;
                }
                base.OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return m_Status; }
            set
            {
                m_Status = value;
                base.OnPropertyChanged();
            }
        }

        public string Codepage
        {
            get
            {
                CodePageListPopupViewModel.CurrentEditor = CurrentEditor;
                base.OnPropertyChanged(nameof(CanShowCodePageList));
                return CurrentEditor?.Encoding?.EncodingName;
            }
        }
        
        private void OnShowCodePageList()
        {
            CodePageListPopupViewModel.ShowCodePageList = true;
            CodePageListPopupViewModel.CurrentEditor = CurrentEditor;
            base.OnPropertyChanged(nameof(CanShowCodePageList));
        }
        public bool CanShowCodePageList =>
                    (CurrentEditor?.CurrentFilePath != null) &&
                    File.Exists(CurrentFilePath);

        private void OnFileLoaded(object sender, FileLoadedArg arg)
        {
            if (sender == CurrentEditor)
            {
                base.OnPropertyChanged(nameof(Codepage));
            }
        }

        private void OnStatusChanged(object sender, StatusChangedArg arg)
        {
            Status = arg.StatusMsg;
        }

        private void OnEditorLoaded(object sender, EditorLoadedArg arg)
        {
            base.OnPropertyChanged(nameof(ShowScenarioIndex));
            base.OnPropertyChanged(nameof(IsShowScenarioIndexEnabled));
            base.OnPropertyChanged(nameof(ScenarioIndexIcon));
        }

        private void OnHideScenarioIndex(object sender, HideScenarioIndexArg arg)
        {
            if (CurrentEditor == arg.EditorViewModel)
            {
                ShowScenarioIndex = false;
            }
        }

        public bool ShowScenarioIndex
        {
            get { return (CurrentEditor != null) && !CurrentEditor.HideScenarioIndex; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.HideScenarioIndex = !value;
                }
                base.OnPropertyChanged(nameof(IsShowScenarioIndexEnabled));
                base.OnPropertyChanged(nameof(ScenarioIndexIcon));
                base.OnPropertyChanged();
            }
        }

        public bool IsShowScenarioIndexEnabled => (CurrentEditor?.IsFeatureFile == true);

        public DrawingImage ScenarioIndexIcon
        {
            get
            {
                if (ShowScenarioIndex)
                    return Util.Util.DrawingImageByOverlapping("Index.png", "Tick64.png");
                else
                    return Util.Util.DrawingImageFromResource("Index.png");
            }
        }

        public bool ShowSplitView
        {
            get { return CurrentEditor?.HideSplitView == false; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.HideSplitView = !value;
                }
                base.OnPropertyChanged(nameof(HorizontalSplitIcon));
                base.OnPropertyChanged();
            }
        }
        public DrawingImage HorizontalSplitIcon
        {
            get
            {
                if (ShowSplitView)
                    return Util.Util.DrawingImageByOverlapping("HSplit.png", "Tick64.png");
                else
                    return Util.Util.DrawingImageFromResource("HSplit.png");
            }
        }

        public Brush CurrentLineBackground
        {
            get { return Brushes.Red; }
        }

        public void StartupFile(string path)
        {
            StartFoldingUpdateTimer();

            List<string> lastOpenedFiles = null;
            if (path != null)
            {
                lastOpenedFiles = new List<string>() { path };
            }
            else
            {
                lastOpenedFiles = m_AppSettings.LastOpenedFiles;
            }

            OpenFiles(lastOpenedFiles.ToArray());
        }

        public void OnOpenFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = true;
            dlg.FilterIndex = 1;
            dlg.Filter = ConfigReader.GetValue<string>("file_open_filter", "All Files (*.*)|*.*");
            bool? result = dlg.ShowDialog();
            if ((result == true) && (dlg.FileNames.Length > 0))
            {
                OpenFiles(dlg.FileNames);
            }
        }

        public void OpenFiles(params string[] files) => m_MultiFilesOpener.OpenFiles(files);

        public void OnNewFile()
        {
            CreateEmptyTab();
        }

        private void OnOpeningTab(EditorTabItem tab)
        {
            SelectTab(tab);
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        /// <summary>
        /// Select last selected file which was selected when closing the application.
        /// This method will be called only once at starting up by
        /// removing the event handler after this method called.
        /// </summary>
        private void OnLoadFilesCompleted()
        {
            SelectLastSelectedFile();
            base.OnPropertyChanged(nameof(CurrentFilePath));

            m_MultiFilesOpener.LoadFilesCompletedEvent -= OnLoadFilesCompleted;
        }

        private void SelectLastSelectedFile()
        {
            string lastSelectedFile = m_AppSettings.LastSelectedFile;
            if (!string.IsNullOrEmpty(lastSelectedFile))
            {
                var tab = TabPanels.FirstOrDefault(x => x.FilePath == lastSelectedFile);
                SelectTab(tab);
            }
        }

        private void SelectTab(EditorTabItem tab)
        {
            int index = TabPanels.IndexOf(tab);
            this.SelectedTabIndex = Math.Max(0, index);
        }

        private void OnGrep()
        {
            string default_grep_text = "";
            if (CurrentEditor?.MainEditor.TextArea.Selection.IsMultiline == false)
            {
                default_grep_text = CurrentEditor.MainEditor.TextArea.Selection.GetText();
            }

            GrepViewModel vm = new GrepViewModel(WorkAreaEditor, m_MultiFilesOpener, m_AppSettings, default_grep_text);
            var dialog = new GrepDialog(vm);
            vm.GrepDiaglog = dialog;
            dialog.ShowDialog();
        }

        private EditorTabItem CreateEmptyTab()
        {
            var tab = m_MultiFilesOpener.CreateEmtyTab();
            SelectTab(tab);
            base.OnPropertyChanged(nameof(CurrentFilePath));
            return tab;
        }

        private void OnFailedToFindEditorControl(object sender, FailedToFindEditorControlArg arg)
        {
            if (TabPanels.Count > 1)
            {
                EditorTabItem tab = TabPanels.FirstOrDefault(x => x.EditorTabContentViewModel == arg.EditorViewModel);
                TabPanels.Remove(tab);
                base.OnPropertyChanged(nameof(TabPanels));
            }
        }

        private void OnOpenFileInNewWindow()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.FilterIndex = 1;
            dlg.Filter = ConfigReader.GetValue<string>("file_open_filter", "All Files (*.*)|*.*");
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                OpenNewGherkinEditorApp(dlg.FileName);

                m_AppSettings.LastUsedFile = dlg.FileName;
            }
        }
        
        private void OnOpenRecentFile(string path)
        {
            OpenFiles(path);
        }

        public void OnSaveFile()
        {
            SaveFile(CurrentEditor, saveAs: false);
        }

        public void OnSaveAsFile()
        {
            SaveFile(CurrentEditor, saveAs: true);
        }

        private void OnSaveAll()
        {
            foreach (EditorTabItem tab in TabPanels)
            {
                tab.EditorTabContentViewModel.SaveFile(saveAs: false);
            }
        }

        public void OnPrint()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Printing.PrintDialog(CurrentEditor.MainEditor, CurrentFilePath);
        }

        public void OnPrintPreview()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Printing.PrintPreviewDialog(CurrentEditor.MainEditor, CurrentFilePath);
        }

        private void OnDeleteEditorTab(object sender, DeleteEditorTabRequestedArg arg)
        {
            EditorTabContentViewModel editorViewModel = arg.EditorViewModel;
            MessageBoxResult result = GherkinSettings.SaveCurrentFileWithRequesting(editorViewModel);
            if (result == MessageBoxResult.Cancel) return;

            if (TabPanels.Count == 1)
            {
                CurrentEditor?.ChangeToEmptyFile();
                base.OnPropertyChanged(nameof(CurrentFilePath));
                base.OnPropertyChanged(nameof(IsShowScenarioIndexEnabled));
            }
            else
            {
                EditorTabItem editorTab = TabPanels.FirstOrDefault(x => x.EditorTabContentViewModel == editorViewModel);
                int index = TabPanels.IndexOf(editorTab);
                bool isDeletingCurrentTab = (index == SelectedTabIndex);
                if (isDeletingCurrentTab)
                {
                    // Force select a tab before removing tab.
                    // Note: property changed notification would not be raised if index is same as SelectedTabIndex.
                    // Therefore we select another tab at first and then select agian the indexed tab if index == SelectedTabIndex
                    int other_index = (index + 1) % TabPanels.Count;
                    SelectedTabIndex = other_index;
                }

                TabPanels.Remove(editorTab);

                if (isDeletingCurrentTab)
                {
                    SelectedTabIndex = Math.Min(index, TabPanels.Count - 1);
                }
            }
        }

        private void OnDeleteAllEditorTabsRequested(object sender, DeleteAllEditorTabsRequestedArg arg)
        {
            MessageBoxResult result = GherkinSettings.SaveAllFilesWithRequesting(arg.ExcludedEditorViewModel);
            if (result == MessageBoxResult.Cancel) return;

            SelectedTabIndex = TabPanels.Count - 1; // Note: Select last tab to force to notify SelectedTabIndex property changed event 

            if (arg.ExcludedEditorViewModel == null)
            {
                for (int i = TabPanels.Count - 1; i > 0; i--)
                {
                    TabPanels.RemoveAt(i);
                }
                TabPanels[0].EditorTabContentViewModel.ChangeToEmptyFile();
            }
            else
            {
                for (int i = TabPanels.Count - 1; i >= 0; i--)
                {
                    if (TabPanels[i].EditorTabContentViewModel != arg.ExcludedEditorViewModel)
                    {
                        TabPanels.RemoveAt(i);
                    }
                }
            }
            SelectedTabIndex = 0;
        }

        public MessageBoxResult SaveAllFilesWithRequesting()
        {
            return GherkinSettings.SaveAllFilesWithRequesting(excluded: null);
        }

        private void SaveFile(EditorTabContentViewModel editor, bool saveAs)
        {
            editor.SaveFile(saveAs);
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        private void OnRenameDocumentRequested(object sender, RenameDocumentRequestedArg arg)
        {
            arg.SourceTabContentViewModel.Rename(arg.SourceTabContentViewModel.CurrentFilePath);
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        private void OnSaveAsDocumentRequested(object sender, SaveAsDocumentRequestedArg arg)
        {
            arg.SourceTabContentViewModel.SaveFile(saveAs: true);
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        public bool IsAllowRunningMultiApps
        {
            set
            {
                m_AppSettings.IsAllowRunningMultiApps = value;
                m_AppSettings.Save();
            }
        }

        private void OnOpenNewGherkinEditorRequested(object sender, OpenNewGherkinEditorRequestedArg arg)
        {
            OpenNewGherkinEditorApp(arg.FilePath);
        }

        private void OpenNewGherkinEditorApp(string filePath)
        {
            IsAllowRunningMultiApps = true;

            Process gherkinEditor = new Process();
            gherkinEditor.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
            gherkinEditor.StartInfo.Arguments = filePath;
            gherkinEditor.Start();
        }

        private void OnOpenFolder()
        {
            string path;
            if (string.IsNullOrEmpty(CurrentFilePath))
                path = Directory.GetCurrentDirectory();
            else
                path = Path.GetDirectoryName(CurrentFilePath);

            Process.Start(path);
        }

        private void StartFoldingUpdateTimer()
        {
            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += delegate { CurrentEditor?.UpdateFoldings(refresh:false); };
            foldingUpdateTimer.Start();
        }

        public bool IsCloseTablesFolding
        {
            get { return CurrentEditor?.IsCloseTablesFolding ?? false; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.IsCloseTablesFolding = value;
                }

                base.OnPropertyChanged();
            }
        }

        public bool IsCloseScenarioFolding
        {
            get { return CurrentEditor?.IsCloseScenarioFolding ?? false; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.IsCloseScenarioFolding = value;
                }
                base.OnPropertyChanged();
            }
        }

        private void OnOpenAllFoldings()
        {
            CurrentEditor?.OpenAllFoldings();
            base.OnPropertyChanged(nameof(IsCloseTablesFolding));
            base.OnPropertyChanged(nameof(IsCloseScenarioFolding));
        }

        private void OnGenCPPTestCode()
        {
            if (!ConfirmSaveFeatureFile()) return;

            try
            {
                GherkinSimpleParser parser = new GherkinSimpleParser(MainTextEditor.Document);
                parser.AppendMissingScenarioGUID();

                string feature_text = MainTextEditor.Document.Text;
                using (TextReader reader = new StringReader(feature_text))
                {
                    CucumberCpp.BDDUtil.SupportUnicode = m_AppSettings.SupportUnicode;
                    CucumberCpp.BDDCucumber gen = new CucumberCpp.BDDCucumber();
                    string generated_file_names = gen.GenCucumberTestCode(reader, Path.GetDirectoryName(CurrentFilePath));

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder
                        .AppendLine(Properties.Resources.Message_CppTestCodeGeneration)
                        .Append(generated_file_names);

                    WorkAreaEditor.MessageOfGenCPPTestCode = stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex.Message, ex.StackTrace);
            }
        }

        private void DisplayErrorMessage(string errorMsg, string stackTrace)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine(Properties.Resources.Message_HasErrorInCodeGeneration)
                .AppendLine(errorMsg);
            if (Util.ConfigReader.GetValue("showExceptionTrace", false))
            {
                stringBuilder.AppendLine(stackTrace);
            }

            WorkAreaEditor.MessageOfGenCPPTestCode = stringBuilder.ToString();
            MoveCursorToErrorLine(errorMsg);
        }

        private bool ConfirmSaveFeatureFile()
        {
            if (CurrentFilePath == "")
            {
                if (GherkinSettings.SaveCurrentFileWithRequesting(CurrentEditor) == MessageBoxResult.Cancel) return false;
            }

            if (CurrentFilePath == "")
            {
                MessageBox.Show(Properties.Resources.Message_SaveFileBeforeGenCode,
                                Properties.Resources.Message_SaveFileBeforeGenCodeTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void MoveCursorToErrorLine(string exceptionMsg)
        {
            Match m = m_ErrorMsgRegex.Match(exceptionMsg);
            if (m.Success)
            {
                string line = m.Groups[1].ToString();
                string column = m.Groups[2].ToString();
                CurrentEditor?.ScrollCursorTo(int.Parse(line), int.Parse(column));
            }
        }

        private void OnIndentationCompleted(object sender, IndentationCompletedArg arg)
        {
            Status = Properties.Resources.Message_PrettyFormatCompleted;
        }

        private void OnSetGherkinHighlighting()
        {
            this.GherkinSettings.ShowGherkinHighlightSettingWindow();
        }
    }
}
