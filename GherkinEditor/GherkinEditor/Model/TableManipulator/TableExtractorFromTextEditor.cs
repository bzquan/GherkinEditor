using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Gherkin.Model.GherkinFormatUtil;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace Gherkin.Model
{
    public class TableExtractorFromTextEditor
    {
        private TextEditor Editor { get; set; }
        private TextDocument Document => Editor.Document;

        public TableExtractorFromTextEditor(TextEditor textEditor)
        {
            Editor = textEditor;
        }

        public List<List<string>> GetCurrentTable()
        {
            List<List<string>> table = new List<List<string>>();
            List<string> rows = GetCurrentTableText();
            foreach (var rowText in rows)
            {
                List<string> cells = SplitToCells(rowText);
                table.Add(cells);
            }

            return table;
        }

        public static void ExtractTableRange(TextArea textArea, out DocumentLine beginLine, out DocumentLine endLine)
        {
            int offset = textArea.Caret.Offset;
            TextDocument document = textArea.Document;
            DocumentLine currentLine = document.GetLineByOffset(offset);
            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            beginLine = GetBeginOfTable(parser, document, currentLine);
            endLine = GetEndOfTable(parser, document, beginLine);
        }

        private static DocumentLine GetEndOfTable(GherkinSimpleParser parser, TextDocument document, DocumentLine beginLine)
        {
            DocumentLine endLine = beginLine;
            DocumentLine tableLine = beginLine.NextLine;
            while (tableLine != null)
            {
                Token token = parser.ParseToken(GetText(document, tableLine));
                if (token.MatchedType == TokenType.TableRow)
                {
                    endLine = tableLine;
                    tableLine = tableLine.NextLine;
                }
                else
                {
                    break;
                }
            }

            return endLine;
        }

        /// <summary>
        /// Split row text to cells.
        /// </summary>
        /// <param name="rowText">format is |cell0|cell1|...|celln|</param>
        /// <returns></returns>
        private List<string> SplitToCells(string rowText)
        {
            List<string> cells = new List<string>();
            var items = rowText.Split('|');
            for (int i = 1; i < items.Length - 1; i++)
            {
                cells.Add(items[i].Trim());
            }

            return cells;
        }

        private List<string> GetCurrentTableText()
        {
            int offset = Editor.TextArea.Caret.Offset;
            DocumentLine line = Document.GetLineByOffset(offset);
            GherkinSimpleParser parser = new GherkinSimpleParser(Document);

            DocumentLine beginOfTable = GetBeginOfTable(parser, Document, line);
            if (beginOfTable != null)
                return ExtractTableText(parser, beginOfTable);
            else
                return new List<string>();
        }

        private static DocumentLine GetBeginOfTable(GherkinSimpleParser parser, TextDocument document, DocumentLine line)
        {
            Token token = parser.ParseToken(GetText(document, line));
            if (token.MatchedType != TokenType.TableRow) return null;

            DocumentLine beginOfTable = line;
            line = line.PreviousLine;
            while (line != null)
            {
                token = parser.ParseToken(GetText(document, line));
                if (token.MatchedType == TokenType.TableRow)
                {
                    beginOfTable = line;
                    line = line.PreviousLine;
                }
                else
                {
                    break;
                }
            }

            return beginOfTable;
        }

        private List<string> ExtractTableText(GherkinSimpleParser parser, DocumentLine beginOfTable)
        {
            List<string> tableText = new List<string>();
            DocumentLine line = beginOfTable;

            while (line != null)
            {
                Token token = parser.ParseToken(GetText(Document, line));
                if (token.MatchedType == TokenType.TableRow)
                {
                    tableText.Add(GetText(Document, line).Trim());
                    line = line.NextLine;
                }
                else
                {
                    break;
                }
            }

            return tableText;
        }
    }
}
