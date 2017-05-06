using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using static Gherkin.Model.GherkinFormatUtil;

namespace Gherkin.Model
{
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
