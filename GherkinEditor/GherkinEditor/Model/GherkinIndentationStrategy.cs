using System;
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

namespace Gherkin.Model
{
    public class GherkinIndentationStrategy : DefaultIndentationStrategy
    {
        private TextEditor Editor { get; set; }

        public GherkinIndentationStrategy(TextEditor editor)
        {
            Editor = editor;
            editor.TextArea.TextEntered += OnTextEntered;
        }

        public override void IndentLine(TextDocument document, DocumentLine line)
        {
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
                    FormatTable(document, line);
                    break;
                default:
                    base.IndentLine(document, line);
                    break;
            }
        }

        public override void IndentLines(TextDocument document, int beginLine, int endLine)
        {
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

            EventAggregator<IndentationCompletedArg>.Instance.Publish(document, eventArg);
        }

        private IndentationCompletedArg PrepareIndentationCompletedArg(TextDocument document)
        {
            int currentLineNo = Editor.TextArea.Caret.Line;
            int currentColumnNo = Editor.TextArea.Caret.Column;
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

        private TextDocument Document => Editor.Document;

        private void OnTextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(Document.FileName) ||
                !GherkinUtil.IsFeatureFile(Document.FileName)) return;

            // Do nothing because IndentLine would be called by textArea.
            if (e.Text == "\n" || e.Text == "\r" || e.Text == "\r\n") return;

            var line_no = Editor.TextArea.Caret.Line;
            var line = Document.GetLineByNumber(line_no);

            GherkinSimpleParser parser = new GherkinSimpleParser(Document);
            Tuple<TokenType, string> result = parser.Format(GetText(Document, line));
            if (result.Item1 == TokenType.TableRow)
            {
                FormatTable(Document, line);
            }
        }

        private void FormatTable(TextDocument document, DocumentLine line)
        {
            if (CanFormatTable(Editor.TextArea, document, line))
            {
                CursorPosInTable pos = new CursorPosInTable(Editor.TextArea, document, line);
                if (MakeFormattedTable(document, line))
                    MoveCursorToTableRow(pos);
                else
                    base.IndentLine(document, line);
            }
            else
                base.IndentLine(document, line);
        }

        private bool CanFormatTable(TextArea textArea, TextDocument document, DocumentLine line)
        {
            string line_text = GetText(document, line);

            if (string.IsNullOrWhiteSpace(line_text))
                return true;

            int col = textArea.Caret.Column - 1;
            int firstIndex = line_text.IndexOf('|');
            int lastIndex = line_text.LastIndexOf('|');
            return (firstIndex >= 0) &&
                   (col > firstIndex) &&
                   (col <= lastIndex);
        }

        private void MoveCursorToTableRow(CursorPosInTable previousPos)
        {
            TextDocument document = Editor.Document;
            DocumentLine line = document.GetLineByNumber(previousPos.LineNo);

            string line_text = GetText(document, line);
            int cell_counter = 0;
            int pos = 0;
            foreach (var c in line_text)
            {
                if (c == '|') cell_counter++;
                if (cell_counter >= previousPos.CellNo) break;
                pos++;
            }

            int offset = line.Offset + pos + previousPos.PosInCell;
            Editor.TextArea.Caret.Offset = offset;
        }

        class CursorPosInTable
        {
            public CursorPosInTable(TextArea textArea, TextDocument document, DocumentLine line)
            {
                CalcCursorPostion(textArea, document, line);
            }

            public int LineNo { get; private set; }
            public int CellNo { get; private set; }
            public int PosInCell { get; private set; }

            private void CalcCursorPostion(TextArea textArea, TextDocument document, DocumentLine line)
            {
                LineNo = line.LineNumber;
                string line_text = GetText(document, line);
                if (string.IsNullOrWhiteSpace(line_text))
                {
                    CellNo = 1;
                    PosInCell = 1;
                }
                else
                {
                    int col = textArea.Caret.Column;
                    string subStr_before_cursor = line_text.Substring(0, col - 1).Trim();
                    CellNo = Math.Max(subStr_before_cursor.Count(c => c == '|'), 1);
                    PosInCell = CalcPosInCell(subStr_before_cursor);
                }
            }

            private int CalcPosInCell(string text)
            {
                int pos = 0;
                for (int i = text.Length - 1; i >= 0; i--)
                {
                    if (text[i] == '|') break;
                    pos++;
                }

                return pos + 1;
            }
        }
    }
}
