using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public class GraphvizTableCell
    {
        public GraphvizTableCell(string value)
        {
            Value = value;
        }

        public bool? IsDotNode()
        {
            if (string.IsNullOrEmpty(Value)) return null;
            return !(Value.Contains("->") || Value.Contains("--"));
        }

        public string Value { get; set; }
    }

    public class GraphvizTableRow
    {
        private List<GraphvizTableCell> Cells = new List<GraphvizTableCell>();

        public void Add(GraphvizTableCell cell)
        {
            if (cell != null)
            {
                Cells.Add(cell);
            }
        }

        public int Index(string cellValue)
        {
            for (int index = 0; index < Cells.Count; index++)
            {
                if (string.Compare(Cells[index].Value, cellValue, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        public int CellCount => Cells.Count;

        public GraphvizTableCell this[int index]
        {
            get
            {
                if (IsValidIndex(index))
                    return Cells[index];
                else
                    return null;
            }
            set
            {
                if (IsValidIndex(index))
                {
                    Cells[index] = value;
                }
            }
        }

        private bool IsValidIndex(int index) => (index >= 0) && (index < Cells.Count);
    }

    public class GraphvizTable
    {
        private List<GraphvizTableRow> Rows = new List<GraphvizTableRow>();

        public void Add(GraphvizTableRow row)
        {
            if (row != null)
            {
                Rows.Add(row);
            }
        }

        public int RowCount => Rows.Count;

        public GraphvizTableRow this[int index]
        {
            get
            {
                if (IsValidIndex(index))
                    return Rows[index];
                else
                    return null;
            }
            set
            {
                if (IsValidIndex(index))
                {
                    Rows[index] = value;
                }
            }
        }

        private bool IsValidIndex(int index) => (index >= 0) && (index < Rows.Count);
    }
}
