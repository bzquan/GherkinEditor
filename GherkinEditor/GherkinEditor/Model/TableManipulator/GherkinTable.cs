using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gherkin.Model
{
    public class GherkinTableCell
    {
        public GherkinTableCell(string text)
        {
            Text = text.Trim();
        }

        public string Text { get; set; } = "";

        public int Width
        {
            get
            {
                int width = 0;
                foreach (char ch in Text)
                {
                    width += (ch > 0xFF) ? 2 : 1;
                }
                return width;
            }
        }

        public void Pad(int expected_width)
        {
            StringBuilder sb = new StringBuilder(Text);
            sb.Append(' ', expected_width - Width);
            Text = sb.ToString();
        }
    }

    public class GherkinTableRow
    {
        List<GherkinTableCell> m_Cells = new List<GherkinTableCell>();

        public void Add(GherkinTableCell cell) { m_Cells.Add(cell); }

        public void PadCells(int max_columns)
        {
            int pad_cells = max_columns - m_Cells.Count;
            for (int i = 0; i < pad_cells; i++)
            {
                m_Cells.Add(new GherkinTableCell(""));
            }
        }

        public GherkinTableCell this[int index]
        {
            get { return m_Cells[index]; }
        }

        public string GetText()
        {
            StringBuilder sb = new StringBuilder("|");
            foreach (GherkinTableCell cell in m_Cells)
            {
                sb.Append(cell.Text).Append("|");
            }

            return sb.ToString();
        }
    }

    public class GherkinTable
    {
        int m_MaxColumns;
        bool m_AddLastNewLine;

        List<GherkinTableRow> m_Rows = new List<GherkinTableRow>();

        public GherkinTable(bool add_last_new_line)
        {
            m_AddLastNewLine = add_last_new_line;
        }

        public void Add(GherkinTableRow row) { m_Rows.Add(row); }

        public string Format()
        {
            PaddingCells();
            return CreateTableText();
        }

        public GherkinTableRow this[int index]
        {
            get { return m_Rows[index]; }
        }

        private void PaddingCells()
        {
            for (int col = 0; col < m_MaxColumns; col++)
            {
                int max_width = MaxWidth(col);
                for (int row = 0; row < m_Rows.Count(); row++)
                {
                    m_Rows[row][col].Pad(max_width);
                }
            }
        }

        private string CreateTableText()
        {
            StringBuilder sb = new StringBuilder();
            bool is_first = true;
            foreach (GherkinTableRow row in m_Rows)
            {
                if (!is_first) sb.AppendLine();
                sb.Append(GherkinSimpleParser.IDENT4)
                  .Append(row.GetText());
                is_first = false;
            }

            if (m_AddLastNewLine) sb.AppendLine();

            return sb.ToString();
        }

        public void PadCells(int max_columns)
        {
            m_MaxColumns = max_columns;
            foreach (GherkinTableRow row in m_Rows)
            {
                row.PadCells(max_columns);
            }
        }

        int MaxWidth(int col)
        {
            int max_width = 0;
            foreach (GherkinTableRow row in m_Rows)
            {
                max_width = Math.Max(max_width, row[col].Width);
            }

            return max_width;
        }
    }
}
