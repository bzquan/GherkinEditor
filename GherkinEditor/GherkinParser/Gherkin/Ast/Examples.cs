using System.Collections.Generic;
using System.Linq;

namespace Gherkin.Ast
{
    public class Examples : IHasLocation, IHasDescription, IHasRows, IHasTags, IVisit
    {
        public IEnumerable<Tag> Tags { get; private set; }
        public Location Location { get; private set; }
        public string Keyword { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public TableRow TableHeader { get; private set; }
        public IEnumerable<TableRow> TableBody { get; private set; }
        public DataTable ExampleTable { get; private set; }

        public Examples(Tag[] tags, Location location, string keyword, string name, string description, TableRow header, TableRow[] body)
        {
            Tags = tags;
            Location = location;
            Keyword = keyword;
            Name = name;
            Description = description;
            TableHeader = header;
            TableBody = body;

            ExampleTable = CreateExampleTable();
        }

        IEnumerable<TableRow> IHasRows.Rows
        {
            get { return new TableRow[] {TableHeader}.Concat(TableBody); }
        }

        public void Visit(IVisitable visitable)
        {
            visitable.Accept(Tags);
            visitable.Accept(this);
        }

        private DataTable CreateExampleTable()
        {
            List<TableRow> table = new List<TableRow>();
            table.Add(TableHeader);
            table.AddRange(TableBody);

            return new DataTable(table.ToArray());
        }
    }
}