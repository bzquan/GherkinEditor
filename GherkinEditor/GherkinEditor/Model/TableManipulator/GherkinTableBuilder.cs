﻿using ICSharpCode.AvalonEdit.Document;
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
        private TextDocument m_Doc;
        private DocumentLine m_Line;
        private bool m_HaveTrailingRows;
        private List<string> m_Rows;
        private bool m_IsEnteredNewLine;
        private int? m_NewColumnIndex;

        /// <summary>
        /// 指定lineの前後テキストからGherkinTableを作成する
        /// </summary>
        /// <param name="document">document</param>
        /// <param name="line">指定line : 空かテーブル行である</param>
        /// <param name="isEnteredNewLine">true : 新しい行の追加</param>
        /// <param name="newColumnIndex">null以外の場合、現在行以外に新しい列の追加</param>
        public GherkinTableBuilder(TextDocument document, DocumentLine line, bool isEnteredNewLine, int? newColumnIndex)
        {
            m_Doc = document;
            m_Line = line;
            m_IsEnteredNewLine = isEnteredNewLine;
            m_NewColumnIndex = newColumnIndex;
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
            else if (m_IsEnteredNewLine)
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
                    string rowText = AddNewColumn(text);
                    m_Rows.Insert(0, rowText);
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
                    string rowText = AddNewColumn(text);
                    m_Rows.Add(rowText);
                    work_line = work_line.NextLine;
                }
                else
                    return;
            }
        }

        private string AddNewColumn(string rowText)
        {
            string row = rowText.Trim();
            if (m_NewColumnIndex == null)
                return row;
            else if (m_NewColumnIndex == 0)
            {
                return "|" + row;
            }
            else
            {
                int index = Util.StringUtil.GetNthIndex(row, '|', (int)m_NewColumnIndex);
                if (index >= 0)
                    return row.Substring(0, index + 1) + "|" + row.Substring(index + 1);
                else
                    return row;
            }
        }
    }
}
