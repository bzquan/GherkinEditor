using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gherkin.Model.GherkinFormatUtil;

namespace Gherkin.Model
{
    public class GherkinTableBuilder
    {
        TextDocument m_Doc;
        DocumentLine m_Line;
        bool m_HaveTrailingRows;
        List<string> m_Rows;

        /// <summary>
        /// 指定lineの前後テキストからGherkinTableを作成する
        /// </summary>
        /// <param name="document">document</param>
        /// <param name="line">指定line : 空かテーブル行である</param>
        public GherkinTableBuilder(TextDocument document, DocumentLine line)
        {
            m_Doc = document;
            m_Line = line;
        }

        public int Offset { get; private set; }
        public int Length { get; private set; }

        public GherkinTable Build()
        {
            Offset = 0;
            Length = 0;
            m_HaveTrailingRows = false;
            m_Rows = new List<string>();

            if (!CollectRows())
            {
                return null;
            }

            return BuildTable(m_Rows, m_HaveTrailingRows);
        }

        private bool CollectRows()
        {
            CollectLeadingRows();

            string text = GetText(m_Doc, m_Line).Trim();
            if ((m_Rows.Count == 0) && (text.Length == 0)) return false;

            if (text.Length != 0)
            {
                if (m_Rows.Count == 0)
                {
                    Offset = m_Line.Offset;
                }
                m_Rows.Add(text);
                Length += m_Line.TotalLength;
            }
            else
            {
                m_Rows.Add("||");   // add dummy row corresponding to m_Line
            }

            CollectTrailingRows();
            return true;
        }

        private void CollectLeadingRows()
        {
            DocumentLine work_line = m_Line.PreviousLine;
            while (work_line != null)
            {
                string text = GetText(m_Doc, work_line);
                if (IsTableRow(text))
                {
                    Offset = work_line.Offset;
                    Length += work_line.TotalLength;
                    m_Rows.Insert(0, text.Trim());
                    work_line = work_line.PreviousLine;
                }
                else
                    return;
            }
        }

        private void CollectTrailingRows()
        {
            DocumentLine work_line = m_Line.NextLine;
            while (work_line != null)
            {
                string text = GetText(m_Doc, work_line);
                if (IsTableRow(text))
                {
                    m_HaveTrailingRows = true;
                    Length += work_line.TotalLength;
                    m_Rows.Add(text.Trim());
                    work_line = work_line.NextLine;
                }
                else
                    return;
            }
        }

        public static GherkinTable BuildTable(List<string> rows, bool haveTrailingRows = true)
        {
            GherkinTable table = new GherkinTable(add_last_new_line: haveTrailingRows);
            int max_columns = 1;
            foreach (string row_text in rows)
            {
                GherkinTableRow row = new GherkinTableRow();
                string[] cells = row_text.Split('|');
                max_columns = Math.Max(max_columns, cells.Length - 2);
                for (int i = 1; i < cells.Length - 1; i++)
                {
                    // create cells except first and last empty string
                    row.Add(new GherkinTableCell(cells[i]));
                }
                table.Add(row);
            }

            table.PadCells(max_columns);

            return table;
        }
    }
}
