using System.Collections.Generic;
using System.Linq;

namespace Gherkin.Ast
{
    public abstract class ScenarioDefinition : IHasLocation, IHasDescription, IHasSteps, IVisit
    {
        public IEnumerable<Tag> Tags { get; private set; }
        public Location Location { get; private set; }
        public string Keyword { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IEnumerable<Step> Steps { get; private set; }

        protected ScenarioDefinition(Tag[] tags, Location location, string keyword, string name, string description, Step[] steps)
        {
            Location = location;
            Keyword = keyword;
            Name = name;
            Description = description;
            Steps = steps;
            Tags = AppendGUIDTag(tags);
        }

        public virtual void Visit(IVisitable visitable)
        {
            visitable.AcceptSenarioTags(Tags);
            AcceptVisitable(visitable);

            foreach (Step step in Steps)
            {
                step.Visit(visitable);
            }
        }

        public bool HasGUIDTag()
        {
            Tag guidTag = Tags.FirstOrDefault(tag => tag.IsGUID());
            return (guidTag != null);
        }

        protected abstract void AcceptVisitable(IVisitable visitable);

        private IEnumerable<Tag> AppendGUIDTag(Tag[] tags)
        {
            List<Tag> tagList = tags.ToList();
            Tag guidTag = tagList.FirstOrDefault(tag => tag.IsGUID());
            if (guidTag != null)
            {
                // move GUID tag to the end of the list
                tagList.Remove(guidTag);
                tagList.Add(guidTag);
            }
            else
            {
                tagList.Add(Tag.CreateNextGUIDTag(Location));
            }

            return tagList;
        }
    }
}