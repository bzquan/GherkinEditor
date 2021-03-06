﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Document;
using static Gherkin.Model.GherkinFormatUtil;
using static Gherkin.Util.StringBuilderExtension;
using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Input;

namespace Gherkin.Model
{
    public class GherkinIndentationStrategy : DefaultIndentationStrategy
    {
        private TextEditor MainEditor { get; set; }
        private TextEditor ViewerEditor { get; set; }
        private TableFormatter TableFormatter { get; set; }
        public GherkinIndentationStrategy(TextEditor editor, TextEditor viewerEditor)
        {
            MainEditor = editor;
            ViewerEditor = viewerEditor;
            TableFormatter = new TableFormatter(editor);
            editor.TextArea.TextEntered += OnTextEntered;
            editor.TextArea.KeyUp += OnKeyUp;
            editor.TextArea.TextPasted += OnTextPasted;
        }

        /// <summary>
        /// Support multiple lines indenting if it is Gherkin document
        /// </summary>
        /// <param name="document"></param>
        /// <returns>true if Gherkin document</returns>
        public override bool SupportMultiLinesIndent(TextDocument document) => true;

        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (!GherkinUtil.IsFeatureFile(document?.FileName))
            {
                base.IndentLine(document, line);
                return;
            }

            if (document == null) throw new ArgumentNullException(nameof(document));
            if (line == null) throw new ArgumentNullException(nameof(line));

            if (string.IsNullOrEmpty(document.FileName) ||
                !GherkinUtil.IsFeatureFile(document.FileName)) return;

            DocumentLine previousLine = line.PreviousLine;
            if (previousLine == null) return;

            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            Tuple<TokenType, string> result = parser.Format(GetText(document, previousLine));
            switch (result.Item1)
            {
                case TokenType.FeatureLine:
                case TokenType.BackgroundLine:
                case TokenType.ExamplesLine:
                case TokenType.StepLine:
                    document.Replace(previousLine.Offset, previousLine.TotalLength,
                                     MakeTwoLines(result.Item2, GherkinSimpleParser.IDENT2));
                    break;
                case TokenType.ScenarioLine:
                case TokenType.ScenarioOutlineLine:
                    string guid_tag = MakeGUID(document, previousLine.PreviousLine);
                    document.Replace(previousLine.Offset, previousLine.TotalLength,
                                     MakeThreeLines(guid_tag, result.Item2, GherkinSimpleParser.IDENT2));
                    break;
                case TokenType.TableRow:
                    if (!TableFormatter.FormatTable(line, isEnteredNewLine: true, isColumnInserted: false))
                    {
                        base.IndentLine(Document, line);
                    }
                    break;
                default:
                    base.IndentLine(document, line);
                    break;
            }
        }

        public override void IndentLines(TextDocument document, int beginLine, int endLine)
        {
            if (!SupportMultiLinesIndent(document))
            {
                base.IndentLines(document, beginLine, endLine);
                return;
            }

            DocumentLine line = document.GetLineByNumber(beginLine);
            int offset = line?.Offset ?? -1;
            int length = CalcSegmentLength(line, endLine);
            if ((offset == -1) || (length == 0)) return;

            IndentationCompletedArg eventArg = PrepareIndentationCompletedArg(document);

            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            string formatted_text = parser.Format(beginLine, endLine);
            if (endLine == document.LineCount)
            {
                StringBuilder sb = new StringBuilder(formatted_text);
                sb.TrimEnd().AppendLine();
                formatted_text = sb.ToString();
            }
            document.Replace(offset, length, formatted_text);

            EventAggregator<IndentationCompletedArg>.Instance.Publish(MainEditor, eventArg);
        }

        private IndentationCompletedArg PrepareIndentationCompletedArg(TextDocument document)
        {
            int currentLineNo = MainEditor.TextArea.Caret.Line;
            int currentColumnNo = MainEditor.TextArea.Caret.Column;
            DocumentLine line = document.GetLineByNumber(currentLineNo);
            string line_text = GetText(document, line);

            return new IndentationCompletedArg(currentLineNo, currentColumnNo, line_text);
        }

        private int CalcSegmentLength(DocumentLine beginLine, int endLine)
        {
            DocumentLine line = beginLine;
            int length = 0;
            while ((line != null) && (line.LineNumber <= endLine))
            {
                length += line.TotalLength;
                line = line.NextLine;
            }
            return length;
        }

        private TextDocument Document => MainEditor.Document;

        private void OnTextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Do nothing because IndentLine would be called by textArea.
            if (e.Text == "\n" || e.Text == "\r" || e.Text == "\r\n") return;

            bool isColumnInserted = e.Text == "|";
            TryFormatTable(isColumnInserted);
        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Delete) ||
                (e.Key == Key.Back) ||
                (e.Key == Key.Tab))
            {
                TryFormatTable();
            }
        }

        private void OnTextPasted(object sender, TextEventArgs e)
        {
            TryFormatTable();
        }

        private void TryFormatTable(bool isColumnInserted = false)
        {
            var line_no = MainEditor.TextArea.Caret.Line;
            var line = Document.GetLineByNumber(line_no);

            if (!TableFormatter.FormatTable(line, isEnteredNewLine: false, isColumnInserted: isColumnInserted))
            {
                TryMoveCursorToImageOrLaTex(line);
            }
        }

        private void TryMoveCursorToImageOrLaTex(DocumentLine line)
        {
            if ((MainEditor == ViewerEditor) || (ViewerEditor == null) || !HasImageOrLaTex(line)) return;

            int offset = line.Offset;
            ViewerEditor.TextArea.Caret.Offset = offset;
            ViewerEditor.TextArea.Caret.BringCaretToView();
            EventAggregator<ShowViewerEditorRequestArg>.Instance.Publish(this, new ShowViewerEditorRequestArg());
        }

        private bool HasImageOrLaTex(DocumentLine line)
        {
            string text = Document.GetText(line);
            return (text.IndexOf(ImageElementGenerator.ImagePrefix, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                   (text.IndexOf(MathElementGenerator.LaTexPrefix, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}
