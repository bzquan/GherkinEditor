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
using System.Text;

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
        private bool m_IsHSplitViewHidden;
        private bool m_IsVSplitViewHidden;
        private int m_SelectedScenarioIndex;

        public GherkinEditor MainGherkinEditor { get; set; }
        private GherkinEditor SubGherkinEditor { get; set; }
        private GherkinEditor ViewerGherkinEditor { get; set; }
        private IAppSettings m_AppSettings;
        private FoldingExecutor FoldingExecutor { get; set; }

        public TextEditor MainEditor => MainGherkinEditor?.TextEditor;
        private TextEditor SubEditor => SubGherkinEditor?.TextEditor;
        public TextEditor ViewerEditor => ViewerGherkinEditor?.TextEditor;
        public TextDocument Document => MainGherkinEditor?.Document;
        public string CurrentFilePath { get; set; }
        public GherkinEditor LastUsedGherkinEditor { get; set; }
        public GrepViewModel GrepViewModel { get; set; }    // It will own the GrepViewModel if current editor is grep editor

        public EditorTabContentViewModel(string filePath, IAppSettings appSettings)
        {
            CurrentFilePath = filePath;
            m_AppSettings = appSettings;
            m_IsCloseTablesFolding = m_AppSettings.IsCloseTablesFoldingByDefault;
            m_IsCloseScenarioFolding = m_AppSettings.IsCloseScenarioFoldingByDefault;
            m_IsScenarioIndexHidden = !m_AppSettings.ShowScenarioIndexByDefault;
            LoadFileInfo(filePath);
            HideScenarioIndex = !m_AppSettings.ShowScenarioIndexByDefault;
            HideHSplitView = !NeedShowHSplitView(filePath);
            HideVSplitView = !NeedShowVSplitView(filePath);
        }

        public ObservableCollection<ScenarioIndex> ScenarioIndexes => m_ScenarioIndexList;
        public ICommand HideScenarioIndexCmd => new DelegateCommandNoArg(OnHideScenarioIndex);

        private bool IsEditorViewLoaded => MainEditor != null;
        private bool NeedShowHSplitView(string filePath) => m_AppSettings.ShowSplitHorizontalViewByDefault && GherkinUtil.IsFeatureFile(filePath);
        private bool NeedShowVSplitView(string filePath) => m_AppSettings.ShowSplitVerticalViewByDefault && GherkinUtil.IsFeatureFile(filePath);

        public void InitializeEditorView(TextEditor mainEditor, TextEditor subEditor, TextEditor viewerEditor)
        {
            TextDocument document = mainEditor.Document;
            MainGherkinEditor = new GherkinEditor(mainEditor, viewerEditor, document, m_AppSettings, FontFamily, FontSize, installElementGenerators: false);
            mainEditor.TextArea.IsKeyboardFocusedChanged += OnMainEditorKeyboardFocusedChanged;

            SubGherkinEditor = new GherkinEditor(subEditor, null/*viewerEditor*/, document, m_AppSettings, FontFamily, FontSize, installElementGenerators: false);
            subEditor.TextArea.IsKeyboardFocusedChanged += OnSubEditorKeyboardFocusedChanged;

            ViewerGherkinEditor = new GherkinEditor(viewerEditor, null/*viewerEditor*/, document, m_AppSettings, FontFamily, FontSize, installElementGenerators: true);
            viewerEditor.TextArea.IsKeyboardFocusedChanged += OnViewerEditorKeyboardFocusedChanged;

            FoldingExecutor = new FoldingExecutor(MainGherkinEditor, SubGherkinEditor, ViewerGherkinEditor);
            Load(CurrentFilePath);

            EventAggregator<EditorViewInitializationCompleted>.Instance.Publish(this, new EditorViewInitializationCompleted());
            TextEditorLoadedEvent?.Invoke();
            mainEditor.TextArea.Caret.PositionChanged += OnMainEditorCaretPositionChanged;
            viewerEditor.TextArea.Caret.PositionChanged += OnViewerEditorCaretPositionChanged;
        }

        private void OnMainEditorCaretPositionChanged(object sender, EventArgs e)
        {
            if (m_AppSettings.SynchronizeCursorPositions)
            {
                SubGherkinEditor.ScrollCursorTo(MainEditor.TextArea.Caret.Offset, focus: false);
                ViewerGherkinEditor.ScrollCursorTo(MainEditor.TextArea.Caret.Offset, focus: false);
            }
        }

        private void OnViewerEditorCaretPositionChanged(object sender, EventArgs e)
        {
            if (m_AppSettings.SynchronizeCursorPositions)
            {
                MainGherkinEditor.ScrollCursorTo(ViewerEditor.TextArea.Caret.Offset, focus: false);
                SubGherkinEditor.ScrollCursorTo(ViewerEditor.TextArea.Caret.Offset, focus: false);
            }
        }

        private void OnMainEditorKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainGherkinEditor?.HasFocus == true)
                LastUsedGherkinEditor = MainGherkinEditor;
        }

        private void OnSubEditorKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SubGherkinEditor?.HasFocus == true)
                LastUsedGherkinEditor = SubGherkinEditor;
        }
        private void OnViewerEditorKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewerGherkinEditor?.HasFocus == true)
                LastUsedGherkinEditor = ViewerGherkinEditor;
        }

        private void LoadFileInfo(string filePath)
        {
            GherkinFileInfo fileInfo = m_AppSettings.GetFileInfo(filePath);
            FontFamily = new FontFamily(fileInfo.FontFamilyName);
            FontSize = fileInfo.FontSize;
        }

        public bool ShowSearchPanel()
        {
            var editor = FocusedGherkinEditor;
            editor?.ShowSearchPanel();
            return (editor != null);
        }

        private GherkinEditor FocusedGherkinEditor
        {
            get
            {
                if (MainGherkinEditor?.HasFocus == true)
                    return MainGherkinEditor;
                else if (SubGherkinEditor?.HasFocus == true)
                    return SubGherkinEditor;
                else if (ViewerGherkinEditor?.HasFocus == true)
                    return ViewerGherkinEditor;
                else
                    return null;
            }
        }

        public void UpdateColumnRuler()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateColumnRuler));
        }

        public void UpdateRequireControlModifierForHyperlinkClick()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateRequireControlModifierForHyperlinkClick));
        }

        public void UpdateConvertTabsToSpaces()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateConvertTabsToSpaces));
        }

        public void UpdateIndentationSize()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateIndentationSize));
        }

        public void UpdateShowEndOfLine()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateShowEndOfLine));
        }

        public void UpdateShowSpaces()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateShowSpaces));
        }

        public void UpdateShowTabs()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateShowTabs));
        }

        public void UpdateWordWrap()
        {
            UpdateEditors(nameof(GherkinEditor.UpdateWordWrap));
        }

        public bool HasSearchHighlightingTransformer()
        {
            return MainGherkinEditor.HasSearchHighlightingTransformer ||
                   SubGherkinEditor.HasSearchHighlightingTransformer ||
                   ViewerGherkinEditor.HasSearchHighlightingTransformer;
        }

        public void ClearSearchHighlighting()
        {
            UpdateEditors(nameof(GherkinEditor.ClearSearchHighlighting));
        }

        public FontFamily FontFamily
        {
            get { return m_FontFamily; }
            set
            {
                if (value == null) return;
                m_FontFamily = value;
                m_AppSettings.UpdateFontFamilyName(CurrentFilePath, FontFamily.ToString());
                UpdateEditorProperty(nameof(GherkinEditor.FontFamily), value);
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
                UpdateEditorProperty(nameof(GherkinEditor.FontSize), value);
            }
        }

        public bool HighlightCurrentLine
        {
            set { UpdateEditorProperty(nameof(GherkinEditor.HighlightCurrentLine), value); }
        }

        public bool ShowCurrentLineBorder
        {
            set { UpdateEditorProperty(nameof(GherkinEditor.ShowCurrentLineBorder), value); }
        }

        public void ChangeToEmptyFile()
        {
            UpdateEditors(nameof(GherkinEditor.ChangeToEmptyFile));
            Keyboard.Focus(MainEditor);

            CurrentFilePath = null;
            HideScenarioIndex = true;
            FileNameChangedEvent?.Invoke(CurrentFilePath);
        }

        public bool IsEmptyFile()
        {
            return string.IsNullOrEmpty(CurrentFilePath) && !IsModified;
        }

        public void Load(string filePath, Encoding encoding = null)
        {
            LoadFileInfo(filePath);

            if (!IsEditorViewLoaded)
            {
                CurrentFilePath = filePath;
            }
            else if ((filePath != null) && File.Exists(filePath))
            {
                GherkinFileInfo fileInfo = m_AppSettings.GetFileInfo(filePath);
                MainEditor.Encoding = encoding ?? GetEncoding(fileInfo);
                MainEditor.Load(filePath);
                HideHSplitView = !NeedShowHSplitView(filePath);
                HideVSplitView = !NeedShowVSplitView(filePath);
                m_AppSettings.UpdateCodePage(filePath, MainEditor.Encoding.CodePage);
                ScrollCursorTo(fileInfo.CursorLine, fileInfo.CursorColumn);
                EventAggregator<FileLoadedArg>.Instance.Publish(this, new FileLoadedArg());
                UpdateEditor(filePath);
                UpdateFoldingsByDefault();
            }
        }

        private Encoding GetEncoding(GherkinFileInfo fileInfo)
        {
            var encodings = Encoding.GetEncodings();
            var encoding = encodings.FirstOrDefault(x => x.CodePage == fileInfo.CodePage);
            return encoding?.GetEncoding();
        }

        private void UpdateEditor(string filePath)
        {
            bool isOldFileFeatureFile = GherkinUtil.IsFeatureFile(Document.FileName);
            SetSyntaxHighlighting(filePath);
            FoldingExecutor?.InstallFoldingManager(filePath);
            CurrentFilePath = filePath;
            Document.FileName = filePath;
            FileNameChangedEvent?.Invoke(filePath);
            if (isOldFileFeatureFile != IsFeatureFile)
            {
                HideScenarioIndex = !IsFeatureFile;
            }

            if (CurrentFilePath != filePath)
            {
                EventAggregator<FileSavedAsArg>.Instance.Publish(this, new FileSavedAsArg(CurrentFilePath));
            }
        }

        public bool IsFeatureFile
        {
            get { return GherkinUtil.IsFeatureFile(CurrentFilePath); }
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
            if (GherkinUtil.IsFeatureFile(filePath))
            {
                highlighting = HighlightingManager.Instance.GetDefinition(GherkinUtil.GherkinHighlightingName(CurrentLanguage));
            }
            else
            {
                highlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(filePath));
            }

            UpdateEditorProperty(nameof(GherkinEditor.SyntaxHighlighting), highlighting);
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

        public Encoding Encoding
        {
            get { return MainGherkinEditor?.Encoding; }
        }

        public void UpdateGherkinHighlighing(bool installFolding = true)
        {
            if (string.IsNullOrEmpty(CurrentFilePath) || (MainGherkinEditor == null)) return;

            try
            {
                if (GherkinUtil.IsFeatureFile(CurrentFilePath))
                {
                    GherkinUtil.RegisterGherkinHighlighting(CurrentLanguage);
                    IHighlightingDefinition highlighting = GetHighlightingDefinition();
                    UpdateEditorProperty(nameof(GherkinEditor.SyntaxHighlighting), highlighting);

                    if (installFolding && (FoldingExecutor != null))
                        FoldingExecutor.InstallFoldingManager(CurrentFilePath);
                }
            }
            catch(Exception ex)
            {
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
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
            if (FoldingExecutor == null) return;

            if (FoldingExecutor.IsCurrentFoldingFeature)
            {
                UpdateGherkinHighlighing();
                List<NewFolding> scenarioFoldings = FoldingExecutor.UpdateFeatureFoldings(IsCloseTablesFolding, IsCloseScenarioFolding, refresh);
                UpdateScenarioIndexes(scenarioFoldings);
            }
            else if (FoldingExecutor.IsCurrentFoldingXML)
            {
                FoldingExecutor.UpdateXMLFoldings();
            }
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
                m_IsScenarioIndexHidden = value || !IsFeatureFile;
                base.OnPropertyChanged();
            }
        }

        public bool HideHSplitView
        {
            get { return m_IsHSplitViewHidden; }
            set
            {
                m_IsHSplitViewHidden = value;
                base.OnPropertyChanged();
            }
        }

        public bool HideVSplitView
        {
            get { return m_IsVSplitViewHidden; }
            set
            {
                m_IsVSplitViewHidden = value;
                base.OnPropertyChanged();
            }
        }

        public void ScrollCursorTo(int line, int column)
        {
            MainGherkinEditor.ScrollCursorTo(line, column);
        }

        /// <summary>
        /// Save current file
        /// </summary>
        /// <param name="saveAs">true: show save as diaglog</param>
        /// <param name="newEncoding">not null: use new encoding to save file</param>
        /// <returns>true : if file saved</returns>
        public bool SaveFile(bool saveAs, Encoding newEncoding = null)
        {
            string filePath2Save = CurrentFilePath;
            if (string.IsNullOrEmpty(CurrentFilePath) || saveAs)
            {
                filePath2Save = NewFilePath(CurrentFilePath);
                if (filePath2Save == null) return false;
            }

            if (newEncoding != null)
            {
                MainEditor.Encoding = newEncoding;
            }
            MainEditor.Save(filePath2Save);
            if (CurrentFilePath != filePath2Save)
            {
                UpdateLastUsedFile(filePath2Save);
            }

            UpdateEditor(filePath2Save);
            DocumentSavedEvent?.Invoke();
            return true;
        }

        private string NewFilePath(string defaultFilePath)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = string.IsNullOrEmpty(defaultFilePath) ? "Unknown" : Path.GetFileName(defaultFilePath);
            dlg.DefaultExt = GherkinUtil.FEATURE_EXTENSION;
            dlg.FilterIndex = 1;
            dlg.Filter = ConfigReader.GetValue<string>("file_open_filter", "All Files (*.*)|*.*");
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        public void Rename(string filePath)
        {
            string newFilePath = NewFilePath(filePath);
            if (newFilePath == null) return;

            try
            {
                File.Move(filePath, newFilePath);
                if (CurrentFilePath != newFilePath)
                {
                    UpdateLastUsedFile(newFilePath);
                }
                UpdateEditor(newFilePath);

                DocumentSavedEvent?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                Properties.Resources.Message_RenameFailedTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void UpdateLastUsedFile(string filePath2Save)
        {
            m_AppSettings.LastUsedFile = filePath2Save;
            m_AppSettings.UpdateFontFamilyName(filePath2Save, FontFamily.ToString());
            m_AppSettings.UpdateFontSize(filePath2Save, FontSize);
            if (MainEditor.Encoding != null)
            {
                m_AppSettings.UpdateCodePage(filePath2Save, MainEditor.Encoding.CodePage);
            }
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

        /// <summary>
        /// Call method by reflection
        /// </summary>
        /// <param name="methodName"></param>
        private void UpdateEditors(string methodName)
        {
            UpdateEditor(MainGherkinEditor, methodName);
            UpdateEditor(SubGherkinEditor, methodName);
            UpdateEditor(ViewerGherkinEditor, methodName);
        }

        /// <summary>
        /// Call method by reflection
        /// Implementation note:
        /// Type thisType = <your object>.GetType();
        /// MethodInfo theMethod = thisType.GetMethod(<The Method Name>); 
        /// theMethod.Invoke(this, <an object [] of parameters or null>); 
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="methodName"></param>
        private void UpdateEditor(GherkinEditor editor, string methodName)
        {
            if (editor == null) return;

            Type thisType = editor.GetType();
            System.Reflection.MethodInfo method = thisType.GetMethod(methodName);
            method.Invoke(editor, null);
        }

        /// <summary>
        /// Set property by reflection
        /// </summary>
        /// <param name="propertyName"></param>
        private void UpdateEditorProperty(string propertyName, object value)
        {
            UpdateEditorProperty(MainGherkinEditor, propertyName, value);
            UpdateEditorProperty(SubGherkinEditor, propertyName, value);
            UpdateEditorProperty(ViewerGherkinEditor, propertyName, value);
        }

        private void UpdateEditorProperty(GherkinEditor editor, string propertyName, object value)
        {
            if (editor == null) return;

            System.Reflection.PropertyInfo prop = editor.GetType().GetProperty(propertyName);
            prop.SetValue(editor, value);
        }
    }
}
