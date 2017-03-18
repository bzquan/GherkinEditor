using System.Collections.Generic;

namespace Gherkin.Ast
{
    public class ScenarioOutline : ScenarioDefinition, IHasTags
    {
        public IEnumerable<Examples> Examples { get; private set; }

        public ScenarioOutline(Tag[] tags, Location location, string keyword, string name, string description, Step[] steps, Examples[] examples) 
            : base(tags, location, keyword, name, description, steps)
        {
            Examples = examples;
        }

        public override void Visit(IVisitable visitable)
        {
            base.Visit(visitable);

            foreach (Examples examples in Examples)
            {
                examples.Visit(visitable);
            }
        }

        protected override void AcceptVisitable(IVisitable visitable)
        {
            visitable.Accept(this);
        }
    }
}