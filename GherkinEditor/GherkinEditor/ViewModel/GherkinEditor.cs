using Gherkin.Model;
using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
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

namespace Gherkin.ViewModel
{
    public class GherkinEditor
    {
        static readonly Brush s_LineBackground = new SolidColorBrush(Color.FromArgb(22, 20, 220, 224));
        static readonly Pen s_CurrentLineBorderPen = new Pen(Brushes.LightGray, thickness: 1);
        static readonly Pen s_CurrentLineBorderTransparentPen = new Pen(Brushes.Transparent, thickness: 1);
        private static FontSizeConverter s_FontSizeConv = new FontSizeConverter();

        private bool m_ShowCurrentLineBorder = true;
        private bool m_HighlightCurrentLine = true;

        public GherkinEditor(TextEditor editor, IAppSettings appSettings)
        {
            TextEditor = editor;
            editor.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            editor.ShowLineNumbers = true;
            editor.LineNumbersForeground = Brushes.DarkGray;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.Options.EnableRectangularSelection = true;
            editor.Options.HighlightCurrentLine = appSettings.HighlightCurrentLine;
            editor.Options.ConvertTabsToSpaces = true;
            editor.FontFamily = new FontFamily(appSettings.FontFamilyName);
            editor.FontSize = ToFontSizeByPoint(appSettings.FontSize);

            editor.TextArea.IndentationStrategy = new GherkinIndentationStrategy();

            HighlightCurrentLine = appSettings.HighlightCurrentLine;
            ShowCurrentLineBorder = appSettings.ShowCurrentLineBorder;
        }

        public TextEditor TextEditor { get; set; }
        public FoldingManager FoldingManager { get; set; }

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

        public void ChangeToEmptyFile()
        {
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
            ScrollCursorTo(doc_line.Offset + column - 1);
        }

        public void ScrollCursorTo(int offset)
        {
            TextEditor.TextArea.Caret.Offset = offset;
            TextEditor.TextArea.Caret.BringCaretToView();
            System.Windows.Input.Keyboard.Focus(TextEditor);     // Display Cursor by focusing the editor
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

        private double ToFontSizeByPoint(string fontSize)
        {
            return (double)s_FontSizeConv.ConvertFromString(fontSize + "pt");
        }
    }
}
