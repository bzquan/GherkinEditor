using System.Collections.Generic;
using System.Text;
using Gherkin.Ast;
using System;

namespace CucumberCpp
{
    public class BDDScenarioOutlineBuilder : BDDAbstrctScenarioBuilder
    {
        BDDInstantiatedTestClassBuilder instantiatedTestClassBuilder = new BDDInstantiatedTestClassBuilder();
        List<Examples> examplesList = new List<Examples>();
        ScenarioOutline ScenarioOutline { get; set; }

        string m_ScenarioOutlineClassName = "ScenarioOutline_" + GUIDIdent();

        public BDDScenarioOutlineBuilder(ScenarioOutline scenarioOutline)
        {
            ScenarioOutline = scenarioOutline;
        }

        public string ScenarioOutlineClassName => m_ScenarioOutlineClassName;

        public override string BuildScenarioImpl()
        {
            StringBuilder scenarioOutlineIml = new StringBuilder();
            scenarioOutlineIml
                .AppendLine(BuildParameterizedTestClass())
                .AppendLine(BuildTestBody())
                .Append(BuildInstantiatedTestClassBuildTestCases());

            return scenarioOutlineIml.ToString();
        }

        public void AddExamples(Examples examples)
        {
            examplesList.Add(examples);
        }

        static string GUIDIdent()
        {
            string guid = Guid.NewGuid().ToString();
            return guid.Replace('-', '_');
        }

        string BuildParameterizedTestClass()
        {
            StringBuilder scenarioOutlineClass = new StringBuilder();

            scenarioOutlineClass
                .AppendLine("class " + BDDUtil.to_ident(ScenarioOutlineClassName) + " :")
                .AppendLine(BDDUtil.INDENT + "public " + BDDUtil.to_ident(FeatureClassName) + ",")
                .AppendLine(BDDUtil.INDENT + "public WithParamInterface<GherkinRow>")
                .AppendLine("{")
                .AppendLine("public:")
                .Append(BuildSetupFunction())
                .AppendLine("};");

            return scenarioOutlineClass.ToString();
        }

        string BuildTestBody()
        {
            StringBuilder scenarioOutlineTestBody = new StringBuilder();

            string scenarioOutline = BDDUtil.MakeIdentifier(ScenarioOutline.Name);
            if (!BDDUtil.SupportUnicode)
            {
                scenarioOutlineTestBody
                    .AppendLine("// TEST_P(" + ScenarioOutlineClassName + ", " + ScenarioOutline + ")");
            }
            scenarioOutlineTestBody
                .AppendLine("TEST_P(" + BDDUtil.to_ident(ScenarioOutlineClassName) + ", " + BDDUtil.to_ident(scenarioOutline) +")")
                .AppendLine("{")
                .AppendLine(BDDUtil.INDENT + "GherkinRow param = GetParam();")
                .AppendLine()
                .Append(BuildSteps(BDDUtil.INDENT))
                .AppendLine("}");

            return scenarioOutlineTestBody.ToString();
        }

        string BuildInstantiatedTestClassBuildTestCases()
        {
            instantiatedTestClassBuilder.StartToBuild(FeatureClassName, ScenarioOutlineClassName);
            StringBuilder testCases = new StringBuilder();
            foreach(Examples examples in examplesList)
            {
                testCases.Append(instantiatedTestClassBuilder.Build(examples));
            }

            return testCases.ToString();
        }

        string BuildSetupFunction()
        {
            StringBuilder setupFunction = new StringBuilder();
            if (!BDDUtil.SupportUnicode)
            {
                setupFunction
                    .AppendLine("// " + BDDUtil.INDENT_DOUBLE + FeatureClassName + "::SetUp();");
            }
            setupFunction
                .AppendLine(BDDUtil.INDENT + "void SetUp() override")
                .AppendLine(BDDUtil.INDENT + "{")
                .AppendLine(BDDUtil.INDENT_DOUBLE + BDDUtil.to_ident(FeatureClassName) + "::SetUp();")
                .AppendLine(BDDUtil.INDENT_DOUBLE + BuildGUIDTag())
                .AppendLine(BDDUtil.INDENT + "}");

            return setupFunction.ToString();
        }
    }
}
