using System.Text;

namespace CucumberCpp
{
    public class BDDBackgroundBuilder : BDDAbstrctScenarioBuilder
    {
        public override string BuildScenarioImpl()
        {
            StringBuilder background = new StringBuilder();
            background
                .AppendLine(BDDUtil.INDENT + "void FeatureBackground()")
                .AppendLine(BDDUtil.INDENT + "{")
                .Append(BuildSteps(BDDUtil.INDENT_DOUBLE))
                .AppendLine(BDDUtil.INDENT + "}");

            return background.ToString();
        }
    }
}
