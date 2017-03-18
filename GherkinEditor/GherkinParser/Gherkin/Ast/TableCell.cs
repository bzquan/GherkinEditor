namespace Gherkin.Ast
{
    public class TableCell : IHasLocation
    {
        public Location Location { get; private set; }
        public string TrimmedValue { get; private set; }
        public string OriginalValue { get; private set; }

        public TableCell(Location location, string value)
        {
            Location = location;
            OriginalValue = value;
            TrimmedValue = TrimmedValue = Trim(value);
        }

        private string Trim(string value)
        {
            return value.Replace("\"", string.Empty);
        }
    }
}