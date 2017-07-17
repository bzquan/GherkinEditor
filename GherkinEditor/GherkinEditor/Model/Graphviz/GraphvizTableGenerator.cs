using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.Document;

namespace Gherkin.Model
{
    public static class GraphvizTableGenerator
    {

        public static GraphvizTable MakeTable(TextDocument document, int offset)
        {
            DocumentLine line = GetTableHeaderLine(document, offset);
            if (line == null) return null;

            GraphvizTable table = new GraphvizTable();
            while (line != null)
            {
                string rowText = document.GetText(line).Trim();
                var row = ToRow(rowText);
                if (row?.CellCount > 0)
                {
                    table.Add(row);
                    line = line.NextLine;
                }
                else
                    break;
            }

            if (table.RowCount > 1)
                return table;
            else
                return null;
        }

        private static DocumentLine GetTableHeaderLine(TextDocument document, int offset)
        {
            var line = document.GetLineByOffset(offset)?.NextLine;
            while (line != null)
            {
                string tableHeader = document.GetText(line).Trim();
                if (IsEmptyOrCommentLine(tableHeader))
                    line = line.NextLine;
                else if (IsTableRow(tableHeader))
                    return line;
                else
                    return null;
            }

            return null;
        }

        private static bool IsEmptyOrCommentLine(string line)
        {
            return string.IsNullOrEmpty(line) ||
                   line.StartsWith("#", StringComparison.InvariantCultureIgnoreCase);
        }

        private static GraphvizTableRow ToRow(string tableRow)
        {
            GraphvizTableRow row = new GraphvizTableRow();

            if (!IsTableRow(tableRow)) return row;

            var cells = tableRow.Split('|');
            for (int i = 1; i < cells.Length - 1; i++)
            {
                // create cells except first and last empty string
                row.Add(new GraphvizTableCell(cells[i].Trim()));
            }

            return row;
        }

        private static bool IsTableRow(string tableRow) =>
            (tableRow?.StartsWith("|") == true) && (tableRow?.Count(x => x == '|') > 1);
    }
}
