using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Document;
using System.IO;
using System.Xml;
using Gherkin.Util;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;

namespace Gherkin.ViewModel
{
    public class EditorView : System.Windows.Controls.ContentControl, INotifyPropertyChanged
    {
        const string editorDataTemplate = "editorDataTemplate";

        private ObservableCollection<ScenarioIndex> m_ScenarioIndexList = new ObservableCollection<ScenarioIndex>();

        private bool m_IsCloseTablesFolding;
        private bool m_IsCloseScenarioFolding;
        private string m_FontSize;
        private FontFamily m_FontFamily;
        private bool m_IsScenarioIndexHidden;
        private bool m_IsSplitViewHidden;
        private int m_SelectedScenarioIndex;

        private bool IsEditorViewLoaded { get; set; } = false;
        private GherkinEditor MainGherkinEditor { get; set; }
        private GherkinEditor SubGherkinEditor { get; set; }
        private IAppSettings m_AppSettings;
        private GherkinFoldingStrategy FoldingStrategy { get; set; }

        public delegate void FileNameChangedHandler(string filePath);
        public event FileNameChangedHandler FileNameChangedEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        public TextEditor MainEditor { get; private set; }
        public TextEditor SubEditor => SubGherkinEditor?.TextEditor;
        public TextDocument Document => MainGherkinEditor?.Document;
        public GridSplitter HSplitter { get; set; }
        public string CurrentFilePath { get; set; }

        public EditorView(System.Windows.FrameworkElement parent, IAppSettings appSettings)
        {
            this.ContentTemplate = parent.FindResource(editorDataTemplate) as DataTemplate;
            this.DataContext = this;

            m_AppSettings = appSettings;
            m_IsCloseTablesFolding = m_AppSettings.IsCloseTablesFoldingByDefault;
            m_IsCloseScenarioFolding = m_AppSettings.IsCloseScenarioFoldingByDefault;
            m_IsScenarioIndexHidden = !m_AppSettings.ShowScenarioIndexByDefault;
            m_IsSplitViewHidden = !m_AppSettings.ShowSplitViewByDefault;
            m_FontFamily = new FontFamily(m_AppSettings.FontFamilyName);
            m_FontSize = m_AppSettings.FontSize;
            HideScenarioIndex = !m_AppSettings.ShowScenarioIndexByDefault;
            HideSplitView = !m_AppSettings.ShowSplitViewByDefault;

            FoldingStrategy = new GherkinFoldingStrategy();

            base.Loaded += OnEditorViewLoaded;
        }

        public ObservableCollection<ScenarioIndex> ScenarioIndexes => m_ScenarioIndexList;
        public ICommand HideScenarioIndexCmd => new DelegateCommandNoArg(OnHideScenarioIndex);

        public void OnEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsEditorViewLoaded)
            {
                IsEditorViewLoaded = true;
                InitializeEditorView();
            }

            // Important Note: the main editor is focused whenever it is loaded.
            // Reason: FindControl of InitializeEditorView will fail
            // when creating loading next new editor if MainEditor is not focused.
            // 
            // Root cause: Microsoft bug?
            Keyboard.Focus(MainEditor);
        }

        private void InitializeEditorView()
        {
            try
            {
                MainEditor = VisualChildrenFinder.FindControl<EditorView, TextEditor>(this, "mainEditor");
                MainGherkinEditor = new GherkinEditor(MainEditor, m_AppSettings);

                TextEditor subEditor = VisualChildrenFinder.FindControl<EditorView, TextEditor>(this, "subEditor");
                SubGherkinEditor = new GherkinEditor(subEditor, m_AppSettings);
                SubEditor.Document = MainEditor.Document;

                Load(CurrentFilePath);
                EventAggregator<EditorViewInitializationCompleted>.Instance.Publish(this, new EditorViewInitializationCompleted());
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg("Ready"));
            }
            catch (Exception ex)
            {
                EventAggregator<FailedToFindEditorControlArg>.Instance.Publish(this, new FailedToFindEditorControlArg(this));
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
            }
        }

        public new FontFamily FontFamily
        {
            get { return m_FontFamily; }
            set
            {
                m_FontFamily = value;

                if (IsEditorViewLoaded)
                {
                    MainGherkinEditor.FontFamily = value;
                    SubGherkinEditor.FontFamily = value;
                }
            }
        }

        public new string FontSize
        {
            get { return m_FontSize; }
            set
            {
                m_FontSize = value;

                if (IsEditorViewLoaded)
                {
                    MainGherkinEditor.FontSize = value;
                    SubGherkinEditor.FontSize = value;
                }
            }
        }

        public bool HighlightCurrentLine
        {
            set
            {
                if (IsEditorViewLoaded)
                {
                    MainGherkinEditor.HighlightCurrentLine = value;
                    SubGherkinEditor.HighlightCurrentLine = value;
                }
            }
        }

        public bool ShowCurrentLineBorder
        {
            set
            {
                if (IsEditorViewLoaded)
                {
                    MainGherkinEditor.ShowCurrentLineBorder = value;
                    SubGherkinEditor.ShowCurrentLineBorder = value;
                }
            }
        }

        public void ChangeToEmptyFile()
        {
            MainGherkinEditor.ChangeToEmptyFile();
            SubGherkinEditor.ChangeToEmptyFile();

            CurrentFilePath = null;
            FileNameChangedEvent?.Invoke(CurrentFilePath);
        }

        public bool IsEmptyFile()
        {
            return string.IsNullOrEmpty(CurrentFilePath) && !MainEditor.IsModified;
        }

        public void Load(string filePath)
        {
            if ((MainEditor == null) || (filePath == null)) return;

            if (File.Exists(filePath))
            {
                MainEditor.Load(filePath);
                UpdateEditor(filePath);
                UpdateFoldingsByDefault();
            }
        }

        private void UpdateEditor(string filePath)
        {
            SetSyntaxHighlighting(filePath);
            InstallFoldingManager();
            bool isSavedAs = (CurrentFilePath != filePath);
            CurrentFilePath = filePath;
            FileNameChangedEvent?.Invoke(filePath);

            if (isSavedAs)
            {
                EventAggregator<FileSavedAsArg>.Instance.Publish(this, new FileSavedAsArg(CurrentFilePath));
            }
        }

        private void UpdateFoldingsByDefault()
        {
            m_IsCloseTablesFolding = m_AppSettings.IsCloseTablesFoldingByDefault;
            m_IsCloseScenarioFolding = m_AppSettings.IsCloseScenarioFoldingByDefault;
            UpdateFoldings(false);   // update folding by manually at the beginning
        }

        private void SetSyntaxHighlighting(string filePath)
        {
            IHighlightingDefinition highlighting;
            if (Path.GetExtension(filePath) == ".feature")
            {
                highlighting = HighlightingManager.Instance.GetDefinition(GherkinUtil.GherkinHighlightingName(CurrentLanguage));
            }
            else
            {
                highlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(filePath));
            }

            MainGherkinEditor.SyntaxHighlighting = highlighting;
            SubGherkinEditor.SyntaxHighlighting = highlighting;
        }

        private string CurrentLanguage
        {
            get
            {
                if (Document != null)
                    return GherkinUtil.CurrentLanguage(Document);
                else
                    return GherkinUtil.DEFAULT_LANGUAGE;
            }
        }

        public void UpdateGherkinHighlighing(bool installFolding = true)
        {
            if (string.IsNullOrEmpty(CurrentFilePath) || (MainGherkinEditor == null)) return;

            if (Path.GetExtension(CurrentFilePath) == ".feature")
            {
                GherkinUtil.RegisterGherkinHighlighting(CurrentLanguage);
                IHighlightingDefinition highlighting = GetHighlightingDefinition();

                MainGherkinEditor.SyntaxHighlighting = highlighting;
                SubGherkinEditor.SyntaxHighlighting = highlighting;

                if (installFolding)
                    InstallFoldingManager();
            }
        }

        private IHighlightingDefinition GetHighlightingDefinition()
        {
            IHighlightingDefinition highlighting = HighlightingManager.Instance.GetDefinition(GherkinUtil.GherkinHighlightingName(CurrentLanguage));
            string[] colorNames = { "Keyword", "StepWord", "Table", "Tag", "DocString", "Mock", "Constants" };
            foreach (string color_name in colorNames)
            {
                ConfigHighlightColor(highlighting, color_name);
            }

            return highlighting;
        }

        private void ConfigHighlightColor(IHighlightingDefinition highlighting, string name)
        {
            HighlightingColor highlightingColor;
            string color_name = null;
            switch (name)
            {
                case "Keyword":
                    color_name = m_AppSettings.ColorOfHighlightingKeyword;
                    break;
                case "StepWord":
                    color_name = m_AppSettings.ColorOfHighlightingStepWord;
                    break;
                case "Table":
                    color_name = m_AppSettings.ColorOfHighlightingTable;
                    break;
                case "Tag":
                    color_name = m_AppSettings.ColorOfHighlightingTag;
                    break;
                case "DocString":
                    color_name = m_AppSettings.ColorOfHighlightingDocString;
                    break;
                case "Constants":
                    color_name = m_AppSettings.ColorOfHighlightingConstants;
                    break;
                case "Mock":
                    color_name = m_AppSettings.ColorOfHighlightingMockAttribute;
                    break;
            }

            if (color_name == null) return;

            highlightingColor = highlighting.GetNamedColor(name);
            highlightingColor.Foreground = new SimpleHighlightingBrush(color_name.ToColor());
        }

        private void InstallFoldingManager()
        {
            string highlighingName = MainGherkinEditor.SyntaxHighlighting?.Name;
            if (GherkinUtil.IsGherkinHighlighting(highlighingName))
            {
                MainGherkinEditor.InstallFoldingManager();
                SubGherkinEditor.InstallFoldingManager();
            }
            else
            {
                MainGherkinEditor.UnnstallFoldingManager();
                SubGherkinEditor.UnnstallFoldingManager();
            }
        }

        public void OpenAllFoldings()
        {
            m_IsCloseTablesFolding = false;
            m_IsCloseScenarioFolding = false;
            UpdateFoldings(refresh: true);
        }

        public bool IsCloseTablesFolding
        {
            get { return m_IsCloseTablesFolding; }
            set
            {
                if (m_IsCloseTablesFolding == value) return;
                m_IsCloseTablesFolding = value;
                UpdateFoldings(refresh: true);
            }
        }

        public bool IsCloseScenarioFolding
        {
            get { return m_IsCloseScenarioFolding; }
            set
            {
                if (m_IsCloseScenarioFolding == value) return;
                m_IsCloseScenarioFolding = value;
                UpdateFoldings(refresh: true);
            }
        }

        public void UpdateFoldings(bool refresh)
        {
            if (MainGherkinEditor == null) return;

            UpdateGherkinHighlighing();
            List<FoldingManager> managers = new List<FoldingManager>();
            if (MainGherkinEditor.FoldingManager != null)
                managers.Add(MainGherkinEditor.FoldingManager);
            if (SubGherkinEditor.FoldingManager != null)
                managers.Add(SubGherkinEditor.FoldingManager);

            List<NewFolding> scenarioFoldings = FoldingStrategy.UpdateFoldings(managers, Document, IsCloseTablesFolding, IsCloseScenarioFolding, refresh);
            UpdateScenarioIndexes(scenarioFoldings);
        }

        private void UpdateScenarioIndexes(List<NewFolding> scenarioFoldings)
        {
            if (scenarioFoldings == null) return;

            AdjustScenarioIndexCount(scenarioFoldings);
            AdjustScenarioIndexContent(scenarioFoldings);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScenarioIndexes)));
        }

        private void AdjustScenarioIndexCount(List<NewFolding> scenarioFoldings)
        {
            int newScenarios = scenarioFoldings.Count - m_ScenarioIndexList.Count;
            bool isAddItems = newScenarios > 0;
            foreach (var i in Enumerable.Range(0, Math.Abs(newScenarios)))
            {
                if (isAddItems)
                    m_ScenarioIndexList.Add(new ScenarioIndex());
                else
                    m_ScenarioIndexList.RemoveAt(m_ScenarioIndexList.Count - 1);
            }
        }

        private void AdjustScenarioIndexContent(List<NewFolding> scenarioFoldings)
        {
            for (int i = 0; i < scenarioFoldings.Count; i++)
            {
                m_ScenarioIndexList[i].Offset = scenarioFoldings[i].StartOffset;
                m_ScenarioIndexList[i].Title = scenarioFoldings[i].Name;
            }
        }

        public int SelectedScenarioIndex
        {
            get { return m_SelectedScenarioIndex; }
            set
            {
                if (m_SelectedScenarioIndex == value) return;
                if (IsScenarioIndexWithinRange(value))
                {
                    m_SelectedScenarioIndex = value;
                    MainGherkinEditor.ScrollCursorTo(ScenarioIndexes[m_SelectedScenarioIndex].Offset);

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedScenarioIndex)));
                }
            }
        }

        private bool IsScenarioIndexWithinRange(int index)
        {
            if ((index < 0) || (index >= ScenarioIndexes.Count)) return false;

            int offset = ScenarioIndexes[index].Offset;
            var line = Document.GetLineByNumber(Document.LineCount);
            if (index >= line.Offset + line.Length) return false;

            return true;
        }

        private void OnHideScenarioIndex()
        {
            // Hide scenario index window by raising event in order to coordiate main window and this control
            // Note: HideScenarioIndex will be changed to Visibility.Hidden by main window
            EventAggregator<HideScenarioIndexArg>.Instance.Publish(this, new HideScenarioIndexArg(this));
        }

        public bool HideScenarioIndex
        {
            get { return m_IsScenarioIndexHidden; }
            set
            {
                m_IsScenarioIndexHidden = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideScenarioIndex)));
            }
        }

        public bool HideSplitView
        {
            get { return m_IsSplitViewHidden; }
            set
            {
                m_IsSplitViewHidden = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideSplitView)));
            }
        }

        public void ScrollCursorTo(int line, int column)
        {
            MainGherkinEditor.ScrollCursorTo(line, column);
        }

        public void SaveFile(bool saveAs)
        {
            string filePath2Save = CurrentFilePath;
            if (string.IsNullOrEmpty(CurrentFilePath) || saveAs)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".feature";
                dlg.FilterIndex = 1;
                dlg.Filter = "Gherkin feature(.feature)|*.feature|Text File(*.txt)|*.txt|All Files (*.*)|*.*";
                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    m_AppSettings.LastUsedFile = dlg.FileName;
                    filePath2Save = dlg.FileName;
                }
            }
            MainEditor.Save(filePath2Save);
            UpdateEditor(filePath2Save);
        }

        public MessageBoxResult SaveCurrentFileWithRequesting()
        {
            if (MainEditor.IsModified)
            {
                string message = String.Format(Properties.Resources.Message_ConfirmSave, CurrentFilePath);
                MessageBoxResult result = MessageBox.Show(message,
                                                          Properties.Resources.Message_ConfirmSaveTile,
                                                          MessageBoxButton.YesNoCancel,
                                                          MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile(saveAs: false);
                }
                return result;
            }

            return MessageBoxResult.Yes;
        }
    }
}
