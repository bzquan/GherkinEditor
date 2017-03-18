using System.Linq;
using System.Text;
using Gherkin.Ast;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CucumberCpp
{
    class BDDGherkinTableBuilder
    {

        public string Build(DataTable dataTable, int tableSeqNo = -1, string indent = "")
        {
            FormatTable(dataTable);

            string strTableSeqNo = (tableSeqNo >= 0) ? tableSeqNo.ToString() : "";
            string tableVariableName = "table" + strTableSeqNo;
            return BuildTableVariable(dataTable, tableVariableName, indent, false);
        }

        private static void FormatTable(DataTable dataTable)
        {
            List<string> table_rows = BuildUnformattedTableText(dataTable);
            string[] formatted_rows = FormatTable(table_rows);
            BuildFormattedRows(dataTable, formatted_rows);
        }

        private static List<string> BuildUnformattedTableText(DataTable dataTable)
        {
            List<string> table_rows = new List<string>();
            foreach (TableRow row in dataTable.Rows)
            {
                StringBuilder row_sb = new StringBuilder("|");
                foreach (var cell in row.Cells)
                {
                    row_sb
                        .Append(cell.OriginalValue)
                        .Append("|");
                }
                string row_str = row_sb.ToString();
                table_rows.Add(row_str);
                row.OriginalFormattedText = row_str;
            }

            return table_rows;
        }

        private static string[] FormatTable(List<string> table_rows)
        {
            Gherkin.Model.GherkinTable table = Gherkin.Model.GherkinTableBuilder.BuildTable(table_rows);
            string table_text = table.Format();
            string[] formatted_rows = Regex.Split(table_text, "\r\n|\r|\n");
            return formatted_rows;
        }

        private static void BuildFormattedRows(DataTable dataTable, string[] formatted_rows)
        {
            int row_index = 0;
            foreach (TableRow row in dataTable.Rows)
            {
                row.TrimmedFormattedText = formatted_rows[row_index].Trim();
                row_index++;
            }
        }

        public static string BuildTableVariable(DataTable dataTable, string tableVariableName, string indent, bool is_static)
        {
            FormatTable(dataTable);

            StringBuilder tableDefinition = new StringBuilder();
            tableDefinition
                .Append(AppendGherkinTableDefinition(tableVariableName, indent, is_static))
                .Append(AppendGherkinTableBody(dataTable, indent));

            return tableDefinition.ToString();
        }

        static string AppendGherkinTableDefinition(string tableVariableName, string indent, bool is_static)
        {
            StringBuilder tableDef = new StringBuilder();
            tableDef
                .Append(indent)
                .Append(is_static ? "static " : "")
                .AppendLine("GherkinTable " + tableVariableName + "(");

            return tableDef.ToString();
        }

        static string AppendGherkinTableBody(DataTable dataTable, string indent)
        {
            StringBuilder tableBody = new StringBuilder();
            TableRow[] rows = dataTable.Rows.ToArray();

            int ROW_COUNT = rows.Count();
            for (int index = 0; index < ROW_COUNT; index++)
            {
                bool is_not_last_row = (index < ROW_COUNT - 1);
                tableBody
                    .Append(BDDUtil.INDENT_DOUBLE)
                    .Append(indent + "L\"")
                    .Append(rows[index].TrimmedFormattedText)
                    .AppendLine(EndingOfRow(is_not_last_row));
            }

            return tableBody.ToString();
        }

        static string EndingOfRow(bool is_not_last_row)
        {
            StringBuilder ending_row = new StringBuilder();
            if (is_not_last_row) ending_row.Append("\\n");
            ending_row.Append("\"");

            if (!is_not_last_row) ending_row.Append(");");

            return ending_row.ToString();
        }
    }
}
