using System.Collections.Generic;
using System.Text;
using Gherkin.Ast;

namespace CucumberCpp
{
    public class BDDFeatureBuilder : BDDAbstractBuilder
    {
        StringBuilder featureImpl = new StringBuilder();
        BDDBackgroundBuilder backgoundBuilder = new BDDBackgroundBuilder();
        List<BDDAbstrctScenarioBuilder> scenarioBuilderList = new List<BDDAbstrctScenarioBuilder>();

        public string FeatureFileName => FeatureTitle + "_Feature.cpp";

        BDDAbstrctScenarioBuilder CurrentScenarioBuilder { get; set; }
        public IEnumerable<Tag> FeatureTags { get; set; }
        public IEnumerable<Tag> ScenarioTags { get; set; }

        public string Build()
        {
            featureImpl.Clear();
            featureImpl
                .AppendLine(BuildDisableWarnings())
                .AppendLine(BuildIncludes())
                .AppendLine(BuildAdditionalIncludes())
                .AppendLine(BuildCompileConditionBegin())
                .AppendLine(BuildFeatureTestClass())
                .AppendLine(BuildScenarioes())
                .Append(BuildCompileConditionEnd());


            return featureImpl.ToString();
        }

        public void CreateBackgound()
        {
            CurrentScenarioBuilder = backgoundBuilder;
        }

        public void NewScenario(Scenario scenario)
        {
            AddNewScenario(new BDDScenarioBuilder(scenario));
        }

        public void NewScenarioOutline(ScenarioOutline scenarioOutline)
        {
            AddNewScenario(new BDDScenarioOutlineBuilder(scenarioOutline));
        }

        public void NewExamples(Examples examples)
        {
            BDDScenarioOutlineBuilder scenarioOutlineBuilder = CurrentScenarioBuilder as BDDScenarioOutlineBuilder;
            if (scenarioOutlineBuilder != null)
            {
                scenarioOutlineBuilder.AddExamples(examples);
            }
        }

        void AddNewScenario(BDDAbstrctScenarioBuilder scenarioBuilder)
        {
            scenarioBuilder.FeatureClassName = FeatureClassName;
            scenarioBuilder.ScenarioTags = ScenarioTags;
            scenarioBuilderList.Add(scenarioBuilder);

            CurrentScenarioBuilder = scenarioBuilder;
            ScenarioTags = null;
        }

        public void AddScenarioStep(BDDStepBuilder stepBuilder)
        {
            if (CurrentScenarioBuilder != null)
            {
                CurrentScenarioBuilder.AddScenarioStep(stepBuilder);
            }
        }

        string BuildDisableWarnings()
        {
            StringBuilder disable_warning = new StringBuilder();

            List<string> warnings = Gherkin.Util.ConfigReader.GetLisValue("disable_warning", "");
            if (warnings.Count == 0) return "";

            disable_warning
                .AppendLine("#ifdef WIN32");

            foreach (string warning in warnings)
            {
                disable_warning.AppendLine("#pragma warning(disable : " + warning + ")");
            }

            disable_warning
                .AppendLine("#endif");

            return disable_warning.ToString();
        }

        string BuildIncludes()
        {
            return "#include \"AbstractBDDTest.h\"";
        }

        string BuildAdditionalIncludes()
        {
            string include = Gherkin.Util.ConfigReader.GetValue("additional_include", "");
            if (include.Length == 0) return "";

            StringBuilder additional_include = new StringBuilder();
            additional_include
                .AppendLine()
                .Append("#include")
                .Append(" \"")
                .Append(include)
                .AppendLine("\"");

            return additional_include.ToString();
        }

        string BuildFeatureTestClass()
        {
            StringBuilder testClass = new StringBuilder();
            testClass
                .AppendLine("// " + FeatureClassName)
                .AppendLine("class " + BDDUtil.to_ident(FeatureClassName) + " : public bdd::AbstractBDDTest")
                .AppendLine("{")
                .AppendLine(BuildSetupAndTearDown())
                .Append(backgoundBuilder.BuildScenario())
                .AppendLine("};");

            return testClass.ToString();
        }

        string FeatureClassName
        {
            get { return FeatureTitle + "_Feature";  }
        }

        string BuildSetupAndTearDown()
        {
            StringBuilder setupAndTearDown = new StringBuilder();
            setupAndTearDown
                .AppendLine("public:")
                .AppendLine(BDDUtil.INDENT + "void SetUp() override")
                .AppendLine(BDDUtil.INDENT + "{")
                .AppendLine(BDDUtil.INDENT_DOUBLE + "AbstractBDDTest::SetUp(L\"" + FeatureTitle + "\");")
                .AppendLine(BDDUtil.INDENT_DOUBLE + "FeatureBackground();")
                .AppendLine(BDDUtil.INDENT + "}")
                .AppendLine()
                .AppendLine(BDDUtil.INDENT + "void TearDown() override")
                .AppendLine(BDDUtil.INDENT + "{")
                .AppendLine(BDDUtil.INDENT_DOUBLE + "AbstractBDDTest::TearDown(L\"" + FeatureTitle + "\");")
                .AppendLine(BDDUtil.INDENT + "}");

            return setupAndTearDown.ToString();
        }

        string BuildScenarioes()
        {
            StringBuilder scenarioes = new StringBuilder();

            foreach(BDDAbstrctScenarioBuilder scenarioBuilder in scenarioBuilderList)
            {
                scenarioes
                    .AppendLine(scenarioBuilder.BuildScenario());
            }

            return scenarioes.ToString();
        }

        string BuildCompileConditionBegin()
        {
            string condition = GetConditionalTag();
            if (condition.Length == 0) return "";

            StringBuilder condition_begin = new StringBuilder();
            condition_begin
                .Append("#ifdef")
                .Append(" ")
                .AppendLine(condition);

            return condition_begin.ToString();
        }

        string BuildCompileConditionEnd()
        {
            string condition = GetConditionalTag();
            if (condition.Length == 0) return "";

            StringBuilder condition_end = new StringBuilder();
            condition_end
                .Append("#endif")
                .Append(" // ")
                .Append(condition);

            return condition_end.ToString();
        }

        string GetConditionalTag()
        {
            List<string> conditional_tags = Gherkin.Util.ConfigReader.GetLisValue("conditional_tag", "");
            foreach (Tag tag in FeatureTags)
            {
                string tagName = tag.Name.Substring(1);
                if (conditional_tags.Contains(tagName))
                    return tagName;
            }

            return "";
        }
    }
}
