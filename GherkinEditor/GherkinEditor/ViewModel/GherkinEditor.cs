using System;
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

namespace Gherkin.ViewModel
{
    public class GherkinEditor
    {
        static readonly Brush s_LineBackground = new SolidColorBrush(Color.FromArgb(22, 20, 220, 224));
        static readonly Pen s_CurrentLineBorderPen = new Pen(Brushes.LightGray, thickness: 1);
        static readonly Pen s_CurrentLineBorderTransparentPen = new Pen(Brushes.Transparent, thickness: 1);
        private static FontSizeConverter s_FontSizeConv = new FontSizeConverter();

        private IAppSettings m_AppSettings;
        private ICSharpCode.AvalonEdit.Search.SearchPanel m_SearchPanel;
        private GherkinCodeCompletion m_GherkinCodeCompletion;
        private bool m_ShowCurrentLineBorder = true;
        private bool m_HighlightCurrentLine = true;

        public GherkinEditor(TextEditor editor, IAppSettings appSettings, FontFamily fontFamily, string fontSize)
        {
            TextEditor = editor;
            m_AppSettings = appSettings;

            ConfigTextEditor(editor, appSettings, fontFamily, fontSize);
            editor.TextArea.IndentationStrategy = new GherkinIndentationStrategy(editor);
            m_GherkinCodeCompletion = new GherkinCodeCompletion(editor, appSettings);

            HighlightCurrentLine = appSettings.HighlightCurrentLine;
            ShowCurrentLineBorder = appSettings.ShowCurrentLineBorder;

            EventAggregator<IndentationCompletedArg>.Instance.Event += OnIndentationCompleted;
        }

        private void ConfigTextEditor(TextEditor editor, IAppSettings appSettings, FontFamily fontFamily, string fontSize)
        {
            editor.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            editor.ShowLineNumbers = true;
            editor.LineNumbersForeground = Brushes.DarkGray;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.Options.EnableRectangularSelection = true;
            editor.Options.HighlightCurrentLine = appSettings.HighlightCurrentLine;
            editor.Options.ConvertTabsToSpaces = true;
            editor.Options.AllowToggleOverstrikeMode = true;
            editor.FontFamily = fontFamily;
            editor.FontSize = ToFontSizeByPoint(fontSize);

            UpdateColumnRuler();
            UpdateRequireControlModifierForHyperlinkClick();
        }

        public TextEditor TextEditor { get; set; }
        public FoldingManager FoldingManager { get; set; }

        public bool HasFocus
        {
            get { return TextEditor.TextArea.IsKeyboardFocused; }
        }

        public void UpdateColumnRuler()
        {
            TextEditor.Options.ColumnRulerPosition = m_AppSettings.ColumnRulerPositon;
            TextEditor.Options.ShowColumnRuler = m_AppSettings.ShowColumnRuler;
        }

        public void UpdateRequireControlModifierForHyperlinkClick()
        {
            TextEditor.Options.RequireControlModifierForHyperlinkClick = m_AppSettings.RequireControlModifierForHyperlinkClick;
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
            m_AppSettings.LastSearchedText = m_SearchPanel.SearchPattern;
            m_AppSettings.IsCaseSensitiveInFind = m_SearchPanel.MatchCase;
            m_AppSettings.IsMatchWholeWordInFind = m_SearchPanel.WholeWords;
            m_AppSettings.IsUseWildcardsInFind = m_SearchPanel.UseWildCards;
            m_AppSettings.IsUseRegexInFind = m_SearchPanel.UseRegex;
        }

        private void SetDefaults()
        {
            m_SearchPanel.MatchCase = m_AppSettings.IsCaseSensitiveInFind;
            m_SearchPanel.WholeWords = m_AppSettings.IsMatchWholeWordInFind;
            m_SearchPanel.UseWildCards = m_AppSettings.IsUseWildcardsInFind;
            m_SearchPanel.UseRegex = m_AppSettings.IsUseRegexInFind;
        }

        private void SetInitialSearchText()
        {
            m_SearchPanel.RecentSearchPatterns = m_AppSettings.LastSearchedTexts;
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
            set { TextEditor.FontSize = ToFontSizeByPoint(value); }
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

        public void ScrollCursorTo(int offset)
        {
            TextEditor.TextArea.Caret.Offset = offset;
            TextEditor.TextArea.Caret.BringCaretToView();
            Keyboard.Focus(TextEditor);     // Display Cursor by focusing the editor
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

        public void AppendText(string text)
        {
            TextEditor.AppendText(text);
        }

        private double ToFontSizeByPoint(string fontSize)
        {
            return (double)s_FontSizeConv.ConvertFromString(fontSize + "pt");
        }

        /// <summary>
        /// Scroll cursor to original position if the document of this editor has been indented.
        /// </summary>
        /// <param name="sender">Indented document</param>
        /// <param name="arg"></param>
        private void OnIndentationCompleted(object sender, IndentationCompletedArg arg)
        {
            if (Document == sender)
            {
                int lineNo = Math.Min(Document.LineCount, arg.Line);
                var line = Document.GetLineByNumber(lineNo);
                int column = Math.Min(arg.Column, line.Length);
                ScrollCursorTo(lineNo, column);
            }
        }
    }
}
