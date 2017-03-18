using System.Collections.Generic;

namespace Gherkin.Ast
{
    public class Background : ScenarioDefinition
    {
        public Background(Location location, string keyword, string name, string description, Step[] steps)
            : base(new List<Tag>().ToArray(), location, keyword, name, description, steps)
        {
        }

        protected override void AcceptVisitable(IVisitable visitable)
        {
            visitable.Accept(this);
        }
    }
}