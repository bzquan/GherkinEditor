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

namespace Gherkin.ViewModel
{
    public class GherkinViewModel : NotifyPropertyChangedBase, IOpenNewEditor
    {
        // Example: (12:0): detailed error message 
        private Regex m_ErrorMsgRegex = new Regex(@"\s*\((\d+):(\d+)\):.*");

        private string m_Status = "Ready";
        private int m_SelectedTabIndex = 0;
        private EditorView m_CurrentEditor;
        private IAppSettings m_AppSettings;
        private TabControl m_EditorTabControl;
        private MultiFileOpener m_MultiFilesOpener;

        public ObservableCollection<EditorTab> TabPanels { get; private set; } = new ObservableCollection<EditorTab>();

        public ICommand SaveAllCmd => new DelegateCommandNoArg(OnSaveAll);
        public ICommand OpenFileInNewWindowCmd => new DelegateCommandNoArg(OnOpenFileInNewWindow);
        public ICommand OpenRecentFileCmd => new DelegateCommand<string>(OnOpenRecentFile);
        public ICommand OpenCurrentFolderCmd => new DelegateCommandNoArg(OnOpenFolder);
        public ICommand OpenAllFoldingsCmd => new DelegateCommandNoArg(OnOpenAllFoldings);
        public ICommand SetGherkinHighlightingCmd => new DelegateCommandNoArg(OnSetGherkinHighlighting);
        public ICommand GenCPPTestCodeCmd => new DelegateCommandNoArg(OnGenCPPTestCode);

        public FontViewModel FontViewModel { get; private set; }
        public GherkinSettingViewModel GherkinSettings { get; private set; }
        public AboutViewModel AboutViewModel { get; private set; }

        public GherkinViewModel(IAppSettings appSettings,
                                MultiFileOpener multiFilesOpener,
                                FontViewModel fontViewModel,
                                GherkinSettingViewModel gherkinSettings,
                                AboutViewModel aboutViewModel)
        {
            m_AppSettings = appSettings;
            GherkinFormatUtil.AppSettings = appSettings;
            m_MultiFilesOpener = multiFilesOpener;
            multiFilesOpener.Config(TabPanels, this);

            FontViewModel = fontViewModel;
            GherkinSettings = gherkinSettings;
            GherkinSettings.TabPanels = TabPanels;
            AboutViewModel = aboutViewModel;

            GherkinSettings.ChangeFoldingTextColorOfAvalonEdit();
            EventAggregator<DeleteEditorTabRequested>.Instance.Event += OnDeleteEditorTab;
            EventAggregator<AdjustMaxWidthOfEditorTabArg>.Instance.Event += OnAdjustMaxWidthOfEditorTab;
            EventAggregator<StatusChangedArg>.Instance.Event += OnStatusChanged;
            EventAggregator<IndentationCompletedArg>.Instance.Event += OnIndentationCompleted;
            EventAggregator<FailedToFindEditorControlArg>.Instance.Event += OnFailedToFindEditorControl;
            EventAggregator<HideScenarioIndexArg>.Instance.Event += OnHideScenarioIndex;
            EventAggregator<FileSavedAsArg>.Instance.Event += OnFileSavedAs;
        }

        public TabControl EditorTabControl
        {
            get { return m_EditorTabControl; }
            set
            {
                m_EditorTabControl = value;
                CreateNewTab(filePath: null); // Create initial editor
                SelectedTabIndex = 0;
            }
        }

        private void OnAdjustMaxWidthOfEditorTab(object sender, AdjustMaxWidthOfEditorTabArg arg)
        {
            AdjustMaxWidthOfTabs();
        }

        private void AdjustMaxWidthOfTabs()
        {
            double tab_control_width = EditorTabControl.ActualWidth;
            if (tab_control_width <= 0.00001) return;

            const double STATUS_IMAGE_WIDTH = 16.0;
            const double MIN = 15.0;
            double max_tab_width = Math.Max(tab_control_width / TabPanels.Count - STATUS_IMAGE_WIDTH, MIN);

            foreach (EditorTab tab in TabPanels)
            {
                tab.SetMaxWidth(max_tab_width);
            }
        }

        public void AddTabItem(EditorTab item)
        {
            TabPanels.Add(item);
            base.OnPropertyChanged(nameof(TabPanels));
        }

        public int SelectedTabIndex
        {
            get { return m_SelectedTabIndex; }
            set
            {
                if ((value < 0) || (value >= TabPanels.Count)) return;

                m_SelectedTabIndex = value;
                CurrentEditor = TabPanels[value].EditorView;
                base.OnPropertyChanged();
            }
        }

        public bool HasEditorLoaded => (MainTextEditor != null);
        private TextEditor MainTextEditor => CurrentEditor?.MainEditor;

        public void FindAndReplace()
        {
            if (HasEditorLoaded)
            {
                View.FindReplaceDialog.ShowForReplace(MainTextEditor);
            }
        }

        private EditorView CurrentEditor
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
        }

        public string CurrentFilePath => CurrentEditor?.CurrentFilePath ?? "";

        private void OnFileSavedAs(object sender, FileSavedAsArg arg)
        {
            base.OnPropertyChanged(nameof(CurrentFilePath));
        }

        public FontFamily CurrentFontFamily
        {
            get
            {
                if (CurrentEditor?.FontFamily != null)
                    return CurrentEditor.FontFamily;
                else
                    return new FontFamily(m_AppSettings.FontFamilyName); }
            set
            {
                string name = value.ToString();
                if (m_AppSettings.FontFamilyName != name)
                {
                    m_AppSettings.FontFamilyName = name;
                    if (HasEditorLoaded)
                        CurrentEditor.FontFamily = new FontFamily(name);
                }
                base.OnPropertyChanged();
            }
        }

        public string CurrentFontSize
        {
            get
            {
                if (CurrentEditor?.FontSize != null)
                    return CurrentEditor.FontSize;
                else
                    return m_AppSettings.FontSize;
            }
            set
            {
                if (m_AppSettings.FontSize != value)
                {
                    m_AppSettings.FontSize = value;
                    if (HasEditorLoaded)
                        CurrentEditor.FontSize = value;
                    base.OnPropertyChanged();
                }
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

        private void OnStatusChanged(object sender, StatusChangedArg arg)
        {
            Status = arg.StatusMsg;
        }

        private void OnHideScenarioIndex(object sender, HideScenarioIndexArg arg)
        {
            if (CurrentEditor == arg.EditorView)
            {
                ShowScenarioIndex = false;
            }
        }

        public bool ShowScenarioIndex
        {
            get { return CurrentEditor?.HideScenarioIndex == false; }
            set
            {
                if (HasEditorLoaded)
                {
                    CurrentEditor.HideScenarioIndex = !value;
                }
                base.OnPropertyChanged(nameof(ScenarioIndexIcon));
                base.OnPropertyChanged();
            }
        }
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
            string filePath = (path != null) ? path : m_AppSettings.LastUsedFile;
            LoadFile(filePath);
            StartFoldingUpdateTimer();
        }

        private void LoadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                m_AppSettings.LastUsedFile = filePath;
                CurrentEditor?.Load(filePath);
                base.OnPropertyChanged(nameof(CurrentFilePath));
            }
        }

        public void OnOpenFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.FilterIndex = 1;
            dlg.Filter = ConfigReader.GetValue<string>("file_open_filter", "All Files (*.*)|*.*");
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                OpenNewEditorTab(dlg.FileName);
            }
        }

        public void OpenFiles(string[] files)
        {
            m_MultiFilesOpener.OpenFiles(files);
        }

        public void OnNewFile()
        {
            EditorTabControl.SelectedItem = CreateNewTab(filePath: null);
        }

        /// <summary>
        /// Open new editor tab.
        /// Empty EditView will be reused when opening the last file.
        /// </summary>
        /// <param name="filePath">file to open. It should be new file when loading multi files.</param>
        public void OpenNewEditorTab(string filePath)
        {
            EditorTab tab;
            if (m_MultiFilesOpener.HaveMoreFilesToLoad)
            {
                tab = CreateNewTab(filePath);
            }
            else
            {
                tab = TabPanels.FirstOrDefault(x => x.EditorView.CurrentFilePath == filePath);
                if (tab == null)
                {
                    tab = TabPanels.FirstOrDefault(x => x.EditorView.IsEmptyFile());
                    if (tab != null)
                        tab.EditorView.Load(filePath);
                    else
                        tab = CreateNewTab(filePath);
                }
            }

            EditorTabControl.SelectedItem = tab;
            m_AppSettings.LastUsedFile = filePath;
        }

        private EditorTab CreateNewTab(string filePath)
        {
            EditorTab tab = new EditorTab();
            tab.HeaderTemplate = EditorTabControl.FindResource("TabHeader") as DataTemplate;
            tab.SetContent(new EditorView(EditorTabControl, m_AppSettings) { CurrentFilePath = filePath });
            AddTabItem(tab);
            base.OnPropertyChanged(nameof(CurrentFilePath));

            return tab;
        }

        private void OnFailedToFindEditorControl(object sender, FailedToFindEditorControlArg arg)
        {
            if (TabPanels.Count > 1)
            {
                EditorTab tab = TabPanels.FirstOrDefault(x => x.EditorView == arg.EditorView);
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
            OpenNewEditorTab(path);
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
            foreach (EditorTab tab in TabPanels)
            {
                tab.EditorView.SaveFile(saveAs: false);
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

        private void OnDeleteEditorTab(object sender, DeleteEditorTabRequested arg)
        {
            EditorView editor = arg.EditorTab.EditorView;
            MessageBoxResult result = GherkinSettings.SaveCurrentFileWithRequesting(editor);
            if (result == MessageBoxResult.Cancel) return;

            if (TabPanels.Count == 1)
            {
                CurrentEditor?.ChangeToEmptyFile();
                base.OnPropertyChanged(nameof(CurrentFilePath));
            }
            else
            {
                TabPanels.Remove(arg.EditorTab);
                SelectedTabIndex = TabPanels.Count - 1;
                base.OnPropertyChanged(nameof(TabPanels));
            }
            AdjustMaxWidthOfTabs();
        }

        public MessageBoxResult SaveAllFilesWithRequesting()
        {
            return GherkinSettings.SaveAllFilesWithRequesting();
        }

        private void SaveFile(EditorView editor, bool saveAs)
        {
            editor.SaveFile(saveAs);
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

        public bool ShowMessageWindow
        {
            get { return m_AppSettings.ShowMessageWindow; }
            set
            {
                if (m_AppSettings.ShowMessageWindow == value) return;
                m_AppSettings.ShowMessageWindow = value;
                base.OnPropertyChanged(nameof(MessageWindowIcon));
                base.OnPropertyChanged();

                EventAggregator<ShowCodeGenMessageWindowArg>
                    .Instance
                    .Publish(this, new ShowCodeGenMessageWindowArg(value));
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
                    ShowGenCPPTestCodeMessage(stringBuilder.ToString());
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
            ShowGenCPPTestCodeMessage(stringBuilder.ToString());
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

        private void ShowGenCPPTestCodeMessage(string message)
        {
            ShowMessageWindow = true;
            ShowCodeGenMessageWindowArg eventArg =
                        new ShowCodeGenMessageWindowArg(true, message);
            EventAggregator<ShowCodeGenMessageWindowArg>
                .Instance
                .Publish(this, eventArg);
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
