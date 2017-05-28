using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid;

namespace Gherkin.Model
{
    public class TableExtractorFromGrid
    {
        Worksheet Worksheet { get; set; }

        public TableExtractorFromGrid(Worksheet sheet)
        {
            Worksheet = sheet;
        }

        public string GetTableText()
        {
            int tableColumnCount = GetTableColumnCount();

            if (tableColumnCount == 0) return "";

            List<string> rows = ExtractTableText(tableColumnCount);
            var gherkinTable = GherkinTableBuilder.BuildTable(rows, false);

            return gherkinTable.Format();
        }

        private int GetTableColumnCount()
        {
            int tableColumnCount = 0;
            for (int i = 0; i < Worksheet.ColumnCount; i++)
            {
                string content = Worksheet[0, i]?.ToString() ?? "";
                if (content.Trim().Length > 0)
                {
                    tableColumnCount++;
                }
                else
                    break;
            }

            return tableColumnCount;
        }

        private List<string> ExtractTableText(int tableColumnCount)
        {
            List<string> rows = new List<string>();
            for (int i = 0; i < Worksheet.RowCount; i++)
            {
                StringBuilder rawText = new StringBuilder();
                StringBuilder sb = new StringBuilder();
                sb.Append("|");
                for (int k = 0; k < tableColumnCount; k++)
                {
                    string content = Worksheet[i, k]?.ToString() ?? "";
                    string trimmed = content.Trim();
                    rawText.Append(trimmed);
                    sb.Append(trimmed + "|");
                }
                if (rawText.Length > 0)
                    rows.Add(sb.ToString());
                else
                    break;  // End extracting table if empty row
            }

            return rows;
        }
    }
}
