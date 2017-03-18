using System;
using System.Collections.Generic;

namespace Gherkin.Ast
{
    public class DataTable : StepArgument, IHasRows, IHasLocation, IVisit
    {
        public Location Location { get; private set; }
        public IEnumerable<TableRow> Rows { get; private set; }

        public DataTable(TableRow[] rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (rows.Length == 0) throw new ArgumentException("DataTable must have at least one row", nameof(rows));

            Location = rows[0].Location;
            Rows = rows;
        }

        public override void Visit(IVisitable visitable)
        {
            visitable.Accept(this);
        }
    }
}