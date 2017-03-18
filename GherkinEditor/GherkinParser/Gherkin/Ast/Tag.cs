using System;

namespace Gherkin.Ast
{
    public class Tag : IHasLocation
    {
        const string GUID_TAG_PREFIX = "@guid-";

        public Location Location { get; private set; }
        public string Name { get; private set; }

        public Tag(Location location, string name)
        {
            Name = name;
            Location = location;
        }

        public bool IsGUID()
        {
            return Name.StartsWith(GUID_TAG_PREFIX, StringComparison.CurrentCulture);
        }

        public static Tag CreateNextGUIDTag(Location location)
        {
            Guid guidValue = Guid.NewGuid();
            // Location of GUID is the same as scenario
            return new Tag(location, GUID_TAG_PREFIX + guidValue.ToString()); // "N" means to remove '-' within tha guid
        }
    }
}