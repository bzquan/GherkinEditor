using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Ast
{
    public interface IVisitable
    {
        //void Accept(Language language);
        void Accept(IEnumerable<Tag> tags);
        void Accept(Feature feature);
        void Accept(Background background);
        void AcceptSenarioTags(IEnumerable<Tag> tags);
        void Accept(Scenario scenario);
        void Accept(ScenarioOutline scenarioOutline);
        void Accept(Step step);
        void Accept(Examples examples);
        void Accept(DocString docString);
        void Accept(DataTable dataTable);
    }
}
