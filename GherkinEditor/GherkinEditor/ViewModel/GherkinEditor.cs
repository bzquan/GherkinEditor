﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Gherkin.Model;
using Gherkin.Util;
using ICSharpCode.AvalonEdit.Indentation.CSharp;

namespace Gherkin.ViewModel
{
    public class GherkinEditor
    {
        static readonly Brush s_LineBackground = new SolidColorBrush(Color.FromArgb(22, 20, 220, 224));
        static readonly Pen s_CurrentLineBorderPen = new Pen(Brushes.LightGray, thickness: 1);
        static readonly Pen s_CurrentLineBorderTransparentPen = new Pen(Brushes.Transparent, thickness: 1);

        private IAppSettings m_AppSettings;
        private ICSharpCode.AvalonEdit.Search.SearchPanel m_SearchPanel;
        private GherkinCodeCompletion m_GherkinCodeCompletion;
        private bool m_ShowCurrentLineBorder = true;
        private bool m_HighlightCurrentLine = true;

        private ICommand EditTableByTableEditorCmd => new DelegateCommandNoArg(OnEditTableByTableEditor, CanEditTableByTableEditor);
        private ICommand ReplaceTableFromGridCmd => new DelegateCommandNoArg(OnReplaceTableFromGrid, CanReplaceTableFromGrid);
        private ICommand PasteTableFromGridCmd => new DelegateCommandNoArg(OnPasteTableFromGrid, CanPasteTableFromGrid);

        public GherkinEditor(TextEditor editor, TextEditor subEditor, TextDocument document, IAppSettings appSettings, FontFamily fontFamily, string fontSize, bool installElementGenerators)
        {
            TextEditor = editor;
            SubTextEditor = subEditor;
            m_AppSettings = appSettings;

            ConfigTextEditor(editor, document, fontFamily, fontSize);
            if (installElementGenerators)
            {
                InstallElementGenerators();
            }

            m_GherkinCodeCompletion = new GherkinCodeCompletion(editor, appSettings);

            editor.Document.FileNameChanged += OnFileNameChanged;
            EventAggregator<IndentationCompletedArg>.Instance.Event += OnIndentationCompleted;
        }

        private void OnFileNameChanged(object sender, EventArgs e)
        {
            var indentationStrategy = TextEditor.TextArea.IndentationStrategy;
            if (GherkinUtil.IsFeatureFile(Document.FileName))
            {
                if (!(indentationStrategy is GherkinIndentationStrategy))
                {
                    TextEditor.TextArea.IndentationStrategy = new GherkinIndentationStrategy(TextEditor, SubTextEditor);
                }
                TextEditor.TextArea.Options.EditTableByTableEditorCmd = EditTableByTableEditorCmd;
                TextEditor.TextArea.Options.ReplaceTableFromGridCmd = ReplaceTableFromGridCmd;
                TextEditor.TextArea.Options.PasteTableFromGridCmd = PasteTableFromGridCmd;
            }
            else
            {
                if (GherkinUtil.IsCSharpFile(Document.FileName))
                {
                    if (!(indentationStrategy is CSharpIndentationStrategy))
                    {
                        TextEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(TextEditor.Options);
                    }
                }

                TextEditor.TextArea.Options.EditTableByTableEditorCmd = null;
                TextEditor.TextArea.Options.ReplaceTableFromGridCmd = null;
                TextEditor.TextArea.Options.PasteTableFromGridCmd = null;
            }
        }

        private void ConfigTextEditor(TextEditor editor, TextDocument document, FontFamily fontFamily, string fontSize)
        {
            editor.Document = document;
            editor.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            editor.ShowLineNumbers = true;
            editor.LineNumbersForeground = Brushes.DarkGray;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.Options.EnableRectangularSelection = true;
            editor.Options.HighlightCurrentLine = m_AppSettings.EditorSettings.HighlightCurrentLine;
            editor.Options.ConvertTabsToSpaces = true;
            editor.Options.AllowToggleOverstrikeMode = true;
            editor.FontFamily = fontFamily;
            editor.FontSize = Util.Util.ToFontSizeByPoint(fontSize);
            HighlightCurrentLine = m_AppSettings.EditorSettings.HighlightCurrentLine;
            ShowCurrentLineBorder = m_AppSettings.EditorSettings.ShowCurrentLineBorder;

            UpdateColumnRuler();
            UpdateRequireControlModifierForHyperlinkClick();
            UpdateConvertTabsToSpaces();
            UpdateIndentationSize();
            UpdateShowEndOfLine();
            UpdateShowSpaces();
            UpdateShowTabs();
            UpdateWordWrap();
        }

        private void InstallElementGenerators()
        {
            TextEditor.TextArea.TextView.ElementGenerators.Add(new ImageElementGenerator(TextEditor));
            TextEditor.TextArea.TextView.ElementGenerators.Add(new MathElementGenerator(TextEditor));
            TextEditor.TextArea.TextView.ElementGenerators.Add(new CurveElementGenerator(TextEditor));
            TextEditor.TextArea.TextView.ElementGenerators.Add(new GraphvizElementGenerator(TextEditor));
        }

        public TextEditor TextEditor { get; set; }
        public TextEditor SubTextEditor { get; set; }
        public FoldingManager FoldingManager { get; set; }

        public bool HasFocus
        {
            get { return TextEditor.TextArea.IsKeyboardFocused; }
        }

        public void UpdateColumnRuler()
        {
            TextEditor.Options.ColumnRulerPosition = m_AppSettings.EditorSettings.ColumnRulerPositon;
            TextEditor.Options.ShowColumnRuler = m_AppSettings.EditorSettings.ShowColumnRuler;
        }

        public void UpdateRequireControlModifierForHyperlinkClick()
        {
            TextEditor.Options.RequireControlModifierForHyperlinkClick = m_AppSettings.EditorSettings.RequireControlModifierForHyperlinkClick;
        }

        public void UpdateConvertTabsToSpaces()
        {
            TextEditor.Options.ConvertTabsToSpaces = m_AppSettings.EditorSettings.ConvertTabsToSpaces;
        }

        public void UpdateIndentationSize()
        {
            TextEditor.Options.IndentationSize = m_AppSettings.EditorSettings.IndentationSize;
        }

        public void UpdateShowEndOfLine()
        {
            TextEditor.Options.ShowEndOfLine = m_AppSettings.EditorSettings.ShowEndOfLine;
        }

        public void UpdateShowSpaces()
        {
            TextEditor.Options.ShowSpaces = m_AppSettings.EditorSettings.ShowSpaces;
        }

        public void UpdateShowTabs()
        {
            TextEditor.Options.ShowTabs = m_AppSettings.EditorSettings.ShowTabs;
        }

        public void UpdateWordWrap()
        {
            TextEditor.WordWrap = m_AppSettings.EditorSettings.WordWrap;
        }

        public void ShowSearchPanel()
        {
            if (m_SearchPanel == null)
            {
                m_SearchPanel = ICSharpCode.AvalonEdit.Search.SearchPanel.Install(TextEditor);
                m_SearchPanel.Localization = SearchPanelLocalization.Instance.Value;
                m_SearchPanel.Closed += OnSearchPanelClosed;
                SetDefaults();
            }
            SetInitialSearchText();
            m_SearchPanel.Open();
        }

        private void OnSearchPanelClosed()
        {
            m_AppSettings.LastStatus.LastSearchedText = m_SearchPanel.SearchPattern;
            m_AppSettings.LastStatus.IsCaseSensitiveInFind = m_SearchPanel.MatchCase;
            m_AppSettings.LastStatus.IsMatchWholeWordInFind = m_SearchPanel.WholeWords;
            m_AppSettings.LastStatus.IsUseWildcardsInFind = m_SearchPanel.UseWildCards;
            m_AppSettings.LastStatus.IsUseRegexInFind = m_SearchPanel.UseRegex;
        }

        private void SetDefaults()
        {
            m_SearchPanel.MatchCase = m_AppSettings.LastStatus.IsCaseSensitiveInFind;
            m_SearchPanel.WholeWords = m_AppSettings.LastStatus.IsMatchWholeWordInFind;
            m_SearchPanel.UseWildCards = m_AppSettings.LastStatus.IsUseWildcardsInFind;
            m_SearchPanel.UseRegex = m_AppSettings.LastStatus.IsUseRegexInFind;
        }

        private void SetInitialSearchText()
        {
            m_SearchPanel.RecentSearchPatterns = m_AppSettings.LastStatus.LastSearchedTexts;
            if (TextEditor.TextArea.Selection.IsMultiline == false)
            {
                var text = TextEditor.TextArea.Selection.GetText();
                if (!string.IsNullOrEmpty(text))
                {
                    m_SearchPanel.SearchPattern = text;
                }
            }

            if (string.IsNullOrEmpty(m_SearchPanel.SearchPattern))
            {
                m_SearchPanel.SearchPattern = m_SearchPanel.RecentSearchPatterns.Count > 0 ? m_SearchPanel.RecentSearchPatterns[0] : "";
            }
        }

        public SearchHighlightingTransformer GetSearchHighlightingTransformer()
        {
            var transformer = LineTransformers.FirstOrDefault(x => x is SearchHighlightingTransformer);
            if (transformer == null)
            {
                var lineTransformers = new SearchHighlightingTransformer();
                LineTransformers.Add(lineTransformers);
                return lineTransformers;
            }
            else
            {
                return transformer as SearchHighlightingTransformer;
            }
        }

        public void ClearSearchHighlighting()
        {
            var transformers = LineTransformers;
            var itemsToRemove = transformers.Where(x => x is SearchHighlightingTransformer).ToList();
            foreach (var item in itemsToRemove)
            {
                transformers.Remove(item);
            }
        }

        public bool HasSearchHighlightingTransformer =>
                            LineTransformers.FirstOrDefault(x => x is SearchHighlightingTransformer) != null;
        private IList<IVisualLineTransformer> LineTransformers => TextEditor.TextArea.TextView.LineTransformers;

        public TextDocument Document
        {
            get { return TextEditor?.Document; }
        }

        public FontFamily FontFamily
        {
            get { return TextEditor.FontFamily; }
            set { TextEditor.FontFamily = value; }
        }

        public string FontSize
        {
            set { TextEditor.FontSize = Util.Util.ToFontSizeByPoint(value); }
        }

        public Encoding Encoding
        {
            get { return TextEditor.Encoding; }
        }

        public int CursorLine
        {
            get { return TextEditor.TextArea.Caret.Line; }
        }
        public int CursorColumn
        {
            get { return TextEditor.TextArea.Caret.Column; }
        }

        public void ChangeToEmptyFile()
        {
            Document.FileName = null;
            TextEditor.Clear();
            TextEditor.IsModified = false;
        }

        public bool HighlightCurrentLine
        {
            get { return m_HighlightCurrentLine; }
            set
            {
                m_HighlightCurrentLine = value;
                TextEditor.TextArea.TextView.CurrentLineBackground =
                            value ? s_LineBackground : Brushes.Transparent;

                UpdateHighlightCurrentLine();
            }
        }

        private void UpdateHighlightCurrentLine()
        {
            TextEditor.Options.HighlightCurrentLine = HighlightCurrentLine || ShowCurrentLineBorder;
        }

        public bool ShowCurrentLineBorder
        {
            get { return m_ShowCurrentLineBorder; }
            set
            {
                m_ShowCurrentLineBorder = value;
                TextEditor.TextArea.TextView.CurrentLineBorder =
                            value ? s_CurrentLineBorderPen : s_CurrentLineBorderTransparentPen;
                UpdateHighlightCurrentLine();
            }
        }

        public void ScrollCursorTo(int line, int column)
        {
            line = Math.Min(line, Document.LineCount);
            DocumentLine doc_line = Document.GetLineByNumber(line);
            int offset = Math.Min(doc_line.Offset + column - 1, Document.TextLength);
            ScrollCursorTo(offset);
        }

        public void ScrollCursorTo(int offset, bool focus = true)
        {
            TextEditor.TextArea.Caret.Offset = offset;
            TextEditor.TextArea.Caret.BringCaretToView();
            if (focus)
            {
                Keyboard.Focus(TextEditor);     // Display Cursor by focusing the editor
            }
        }

        public IHighlightingDefinition SyntaxHighlighting
        {
            get { return TextEditor.SyntaxHighlighting; }
            set
            {
                TextEditor.SyntaxHighlighting = value;
                TextEditor.TextArea.TextView.Redraw();
            }
        }

        public void InstallFoldingManager()
        {
            if (FoldingManager == null)
                FoldingManager = FoldingManager.Install(TextEditor.TextArea);
        }

        public void UnnstallFoldingManager()
        {
            if (FoldingManager != null)
            {
                FoldingManager.Uninstall(FoldingManager);
                FoldingManager = null;
            }
        }

        public void CommentOutSelectedLines()
        {
            using (Document.RunUpdate())
            {
                int start, end;
                GetSelection(out start, out end);
                StringBuilder sb = new StringBuilder();

                for (int lineNo = start; lineNo <= end; lineNo++)
                {
                    DocumentLine line = Document.GetLineByNumber(lineNo);
                    string line_text = GherkinFormatUtil.GetText(Document, line);
                    sb.Append("#").Append(line_text);
                }
                var commented_lines = sb.ToString();

                DocumentLine startLine = Document.GetLineByNumber(start);
                DocumentLine endLine = Document.GetLineByNumber(end);
                int length = endLine.Offset - startLine.Offset + endLine.TotalLength;
                Document.Replace(startLine.Offset, length, commented_lines);
            }
        }
   
        public void UncommentSelectedLines()
        {
            using (Document.RunUpdate())
            {
                int start, end;
                GetSelection(out start, out end);
                StringBuilder sb = new StringBuilder();

                for (int lineNo = start; lineNo <= end; lineNo++)
                {
                    DocumentLine line = Document.GetLineByNumber(lineNo);
                    string line_text = GherkinFormatUtil.GetText(Document, line);
                    sb.Append(RemoveBeginningChar(line_text, '#'));
                }
                var uncommented_lines = sb.ToString();

                DocumentLine startLine = Document.GetLineByNumber(start);
                DocumentLine endLine = Document.GetLineByNumber(end);
                int length = endLine.Offset - startLine.Offset + endLine.TotalLength;
                Document.Replace(startLine.Offset, length, uncommented_lines);
            }
        }

        private string RemoveBeginningChar(string str, char ch)
        {
            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == ch)
                {
                    return str.Remove(i, 1);
                }
                else if (!Char.IsWhiteSpace(c))
                {
                    return str;
                }
            }

            return str;
        }

        private void GetSelection(out int start, out int end)
        {
            var textArea = TextEditor.TextArea;
            if (textArea.Selection.IsEmpty)
            {
                start = textArea.Caret.Line;
                end = start;
            }
            else
            {
                start = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.Offset).LineNumber;
                end = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.EndOffset).LineNumber;
            }
        }

        public void AppendText(string text)
        {
            TextEditor.AppendText(text);
        }

        /// <summary>
        /// Scroll cursor to original position if the document of this editor has been indented.
        /// </summary>
        /// <param name="sender">Indented document</param>
        /// <param name="arg"></param>
        private void OnIndentationCompleted(object sender, IndentationCompletedArg arg)
        {
            if (sender == TextEditor)
            {
                int lineNo = Math.Min(Document.LineCount, arg.Line);
                var line = Document.GetLineByNumber(lineNo);
                int column = Math.Min(arg.Column, line.Length);
                ScrollCursorTo(lineNo, column);
            }
        }

        private void OnEditTableByTableEditor()
        {
            EventAggregator<StartEditTableRequestArg>.Instance.Publish(this, new StartEditTableRequestArg(TextEditor));
        }

        private bool CanEditTableByTableEditor()
        {
            return GherkinUtil.IsFeatureFile(Document.FileName) && IsCurrentLineTableRow();
        }

        private bool IsCurrentLineTableRow()
        {
            int offset = TextEditor.TextArea.Caret.Offset;
            DocumentLine line = Document.GetLineByOffset(offset);
            GherkinSimpleParser parser = new GherkinSimpleParser(Document);
            Token token = parser.ParseToken(GherkinFormatUtil.GetText(Document, line));

            return (token.MatchedType == TokenType.TableRow);
        }

        private void OnReplaceTableFromGrid()
        {
            EventAggregator<ReplaceTableFromGridArg>.Instance.Publish(this, new ReplaceTableFromGridArg(TextEditor));
        }

        private bool CanReplaceTableFromGrid()
        {
            return GherkinUtil.IsFeatureFile(Document.FileName) && IsCurrentLineTableRow();
        }

        private void OnPasteTableFromGrid()
        {
            EventAggregator<PasteTableFromGridArg>.Instance.Publish(this, new PasteTableFromGridArg(TextEditor));
        }

        private bool CanPasteTableFromGrid()
        {
            return GherkinUtil.IsFeatureFile(Document.FileName);
        }
    }
}
