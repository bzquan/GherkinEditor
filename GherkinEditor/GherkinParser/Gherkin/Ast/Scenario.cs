using System.Collections.Generic;

namespace Gherkin.Ast
{
    public class Scenario : ScenarioDefinition, IHasTags
    {
        public Scenario(Tag[] tags, Location location, string keyword, string name, string description, Step[] steps) 
            : base(tags, location, keyword, name, description, steps)
        {
        }

        protected override void AcceptVisitable(IVisitable visitable)
        {
            visitable.Accept(this);
        }
    }
}