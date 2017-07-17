using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using static Gherkin.Model.GherkinFormatUtil;

namespace Gherkin.Model
{
    public class TableFormatter
    {
        private TextEditor Editor { get; set; }
        private TextDocument Document => Editor.Document;

        public TableFormatter(TextEditor editor)
        {
            Editor = editor;
        }

        public bool FormatTable(DocumentLine line, bool isEnteredNewLine, bool isColumnInserted)
        {
            if (!(isEnteredNewLine || CanFormatTable(Document, line))) return false;

            int? newColumnIndex = isColumnInserted ? NewColumnIndex() : null;
            CursorPosInTable pos = new CursorPosInTable(Editor.TextArea, Document, line);
            if (MakeFormattedTable(line, isEnteredNewLine, newColumnIndex))
            {
                MoveCursorToTableRow(pos);
                return true;
            }

            return false;
        }

        private int? NewColumnIndex()
        {
            int currentLineNo = Editor.TextArea.Caret.Line;
            int currentColumnNo = Editor.TextArea.Caret.Column;
            DocumentLine line = Document.GetLineByNumber(currentLineNo);
            string text = GetText(Document, line);
            string textBeforeCursor = text.Substring(0, currentColumnNo - 1);

            var count = textBeforeCursor.Count(x => x == '|');
            if (count > 0)
                return count - 1;
            else
                return null;
        }

        private bool MakeFormattedTable(DocumentLine line, bool isEnteredNewLine, int? newColumnIndex)
        {
            GherkinTableBuilder builder = new GherkinTableBuilder(Document, line, isEnteredNewLine, newColumnIndex);
            GherkinTable table = builder.Build();
            if (table == null) return false;

            string table_text = table.Format();
            if (!table_text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                table_text += Environment.NewLine;
            }

            // We need to replace newly entered new line when formatting table by entering new line
            // New line length is 2 because it is "\r\n"
            int new_line_length = isEnteredNewLine ? 2 : 0;
            Document.Replace(builder.Offset, builder.Length + new_line_length, table_text);

            return true;
        }

        private bool CanFormatTable(TextDocument document, DocumentLine line)
        {
            if (!GherkinUtil.IsFeatureFile(Document.FileName)) return false;

            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            Tuple<TokenType, string> lineType = parser.Format(GetText(document, line));
            if (lineType.Item1 != TokenType.TableRow) return false;

            var previousLine = line?.PreviousLine;
            if (previousLine == null) return false;

            Tuple<TokenType, string> previousLineType = parser.Format(GetText(document, previousLine));
            if (previousLineType.Item1 == TokenType.TableRow)
                return true;
            else
            {
                Tuple<TokenType, string> currentLineType = parser.Format(GetText(document, line));
                return (currentLineType.Item1 == TokenType.TableRow);
            }
        }

        private void MoveCursorToTableRow(CursorPosInTable previousPos)
        {
            DocumentLine line = Document.GetLineByNumber(previousPos.LineNo);

            string line_text = GetText(Document, line);
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
    }
}
