using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Document;
using System.IO;
using Gherkin.Util;
using static Gherkin.Util.Util;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;

namespace Gherkin.ViewModel
{
    public class EditorTabContentViewModel : NotifyPropertyChangedBase
    {
        const string editorDataTemplate = "editorDataTemplate";

        public delegate void FileNameChangedHandler(string filePath);
        public delegate void TextEditorLoadedHandler();
        public delegate void DocumentSavedHandler();

        public event FileNameChangedHandler FileNameChangedEvent;
        public event TextEditorLoadedHandler TextEditorLoadedEvent;
        public event DocumentSavedHandler DocumentSavedEvent;

        private ObservableCollection<ScenarioIndex> m_ScenarioIndexList = new ObservableCollection<ScenarioIndex>();

        private bool m_IsCloseTablesFolding;
        private bool m_IsCloseScenarioFolding;
        private string m_FontSize;
        private FontFamily m_FontFamily;
        private bool m_IsScenarioIndexHidden;
        private bool m_IsSplitViewHidden;
        private int m_SelectedScenarioIndex;

        private GherkinEditor MainGherkinEditor { get; set; }
        private GherkinEditor SubGherkinEditor { get; set; }
        private IAppSettings m_AppSettings;
        private GherkinFoldingStrategy FoldingStrategy { get; set; }

        public TextEditor MainEditor { get; private set; }
        public TextEditor SubEditor => SubGherkinEditor?.TextEditor;
        public TextDocument Document => MainGherkinEditor?.Document;
        public string CurrentFilePath { get; set; }

        public EditorTabContentViewModel(string filePath, IAppSettings appSettings)
        {
            CurrentFilePath = filePath;
            m_AppSettings = appSettings;
            m_IsCloseTablesFolding = m_AppSettings.IsCloseTablesFoldingByDefault;
            m_IsCloseScenarioFolding = m_AppSettings.IsCloseScenarioFoldingByDefault;
            m_IsScenarioIndexHidden = !m_AppSettings.ShowScenarioIndexByDefault;
            m_IsSplitViewHidden = !m_AppSettings.ShowSplitViewByDefault;
            LoadFileInfo(filePath);
            HideScenarioIndex = !m_AppSettings.ShowScenarioIndexByDefault;
            HideSplitView = !m_AppSettings.ShowSplitViewByDefault;

            FoldingStrategy = new GherkinFoldingStrategy();
        }

        public ObservableCollection<ScenarioIndex> ScenarioIndexes => m_ScenarioIndexList;
        public ICommand HideScenarioIndexCmd => new DelegateCommandNoArg(OnHideScenarioIndex);

        private bool IsEditorViewLoaded => MainEditor != null;

        public void InitializeEditorView(TextEditor mainEditor, TextEditor subEditor)
        {
            MainEditor = mainEditor;
            MainGherkinEditor = new GherkinEditor(MainEditor, m_AppSettings, FontFamily, FontSize);

            SubGherkinEditor = new GherkinEditor(subEditor, m_AppSettings, FontFamily, FontSize);
            SubEditor.Document = MainEditor.Document;

            Load(CurrentFilePath);
            EventAggregator<EditorViewInitializationCompleted>.Instance.Publish(this, new EditorViewInitializationCompleted());
            EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg("Ready"));
            TextEditorLoadedEvent?.Invoke();
        }

        private void LoadFileInfo(string filePath)
        {
            GherkinFileInfo fileInfo = m_AppSettings.GetFileInfo(filePath);
            FontFamily = new FontFamily(fileInfo.FontFamilyName);
            FontSize = fileInfo.FontSize;
        }

        public FontFamily FontFamily
        {
            get { return m_FontFamily; }
            set
            {
                if (value == null) return;
                m_FontFamily = value;
                m_AppSettings.UpdateFontFamilyName(CurrentFilePath, FontFamily.ToString());

                if (IsEditorViewLoaded)
                {
                    MainGherkinEditor.FontFamily = value;
                    SubGherkinEditor.FontFamily = value;
                }
            }
        }

        public string FontSize
        {
            get { return m_FontSize; }
            set
            {
                if (value == null) return;
                m_FontSize = value;
                m_AppSettings.UpdateFontSize(CurrentFilePath, FontSize);

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
            return string.IsNullOrEmpty(CurrentFilePath) && !IsModified;
        }

        public void Load(string filePath)
        {
            LoadFileInfo(filePath);

            if (!IsEditorViewLoaded)
            {
                CurrentFilePath = filePath;
            }
            else if ((filePath != null) && File.Exists(filePath))
            {
                MainEditor.Load(filePath);
                GherkinFileInfo fileInfo = m_AppSettings.GetFileInfo(filePath);
                ScrollCursorTo(fileInfo.CursorLine, fileInfo.CursorColumn);

                UpdateEditor(filePath);
                UpdateFoldingsByDefault();
            }
        }

        private void UpdateEditor(string filePath)
        {
            SetSyntaxHighlighting(filePath);
            InstallFoldingManager();
            CurrentFilePath = filePath;
            Document.FileName = filePath;
            FileNameChangedEvent?.Invoke(filePath);

            if (CurrentFilePath != filePath)
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
            if (Path.GetExtension(filePath) == GherkinUtil.FEATURE_EXTENSION)
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

            if (Path.GetExtension(CurrentFilePath) == GherkinUtil.FEATURE_EXTENSION)
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

            color_name.IfNotNull(x =>
            {
                HighlightingColor highlightingColor;
                highlightingColor = highlighting.GetNamedColor(name);
                highlightingColor.Foreground = new SimpleHighlightingBrush(x.ToColor());
            }
            );
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

            base.OnPropertyChanged(nameof(ScenarioIndexes));
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

                    base.OnPropertyChanged();
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
                base.OnPropertyChanged();
            }
        }

        public bool HideSplitView
        {
            get { return m_IsSplitViewHidden; }
            set
            {
                m_IsSplitViewHidden = value;
                base.OnPropertyChanged();
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
                dlg.DefaultExt = GherkinUtil.FEATURE_EXTENSION;
                dlg.FilterIndex = 1;
                dlg.Filter = "Gherkin feature(.feature)|*.feature|Text File(*.txt)|*.txt|All Files (*.*)|*.*";
                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    filePath2Save = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            MainEditor.Save(filePath2Save);
            if (CurrentFilePath != filePath2Save)
            {
                UpdateLastUsedFile(filePath2Save);
            }
            UpdateEditor(filePath2Save);

            DocumentSavedEvent?.Invoke();
        }

        private void UpdateLastUsedFile(string filePath2Save)
        {
            m_AppSettings.LastUsedFile = filePath2Save;
            m_AppSettings.UpdateFontFamilyName(filePath2Save, FontFamily.ToString());
            m_AppSettings.UpdateFontSize(filePath2Save, FontSize);
        }

        public bool IsModified => MainEditor?.IsModified == true;

        public MessageBoxResult SaveCurrentFileWithRequesting()
        {
            m_AppSettings.UpdateCursorPos(CurrentFilePath, MainGherkinEditor.CursorLine, MainGherkinEditor.CursorColumn);
            if (IsModified)
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
