using System.Text;
using Gherkin.Ast;

namespace CucumberCpp
{
    public class BDDScenarioBuilder : BDDAbstrctScenarioBuilder
    {
        Scenario scenario;

        public BDDScenarioBuilder(Scenario scenario)
        {
            this.scenario = scenario;
        }

        public override string BuildScenarioImpl()
        {
            StringBuilder scenarioImpl = new StringBuilder();
            if (!BDDUtil.SupportUnicode)
            {
                scenarioImpl.AppendLine("// TEST_F(" + FeatureClassName + ", " + ScenarioName + ")");
            }

            scenarioImpl
                .AppendLine("TEST_F(" + BDDUtil.to_ident(FeatureClassName) + ", " + BDDUtil.to_ident(ScenarioName) + ")")
                .AppendLine("{")
                .AppendLine(BDDUtil.INDENT + BuildGUIDTag())
                .AppendLine()
                .Append(BuildSteps(BDDUtil.INDENT))
                .AppendLine("}");

            return scenarioImpl.ToString();
        }

        string ScenarioName
        {
            get { return BDDUtil.MakeIdentifier(scenario.Name);  }
        }
    }
}
