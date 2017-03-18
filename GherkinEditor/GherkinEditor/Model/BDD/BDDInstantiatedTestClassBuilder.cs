using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gherkin.Ast;

namespace CucumberCpp
{
    public class BDDInstantiatedTestClassBuilder
    {
        string FeatureClassName { get; set; }
        string ScenarioOutlineClassName { get; set; }
        static int InstanceNo { get; set; } = 0;

        public void StartToBuild(string featureClassName, string scenarioOutlineClassName)
        {
            FeatureClassName = featureClassName;
            ScenarioOutlineClassName = scenarioOutlineClassName;
        }

        public string Build(Examples examples)
        {
            string instantiationName = FeatureClassName + "_" + InstanceNo.ToString();
            InstanceNo++;

            string exampleTableName = "s_table_" + InstanceNo.ToString();
            StringBuilder instantiatedTestClass = new StringBuilder();
            instantiatedTestClass
                .AppendLine()
                .AppendLine(BDDGherkinTableBuilder.BuildTableVariable(examples.ExampleTable, exampleTableName, indent: "", is_static: true))
                .Append(InstantiatedTestClass(instantiationName, exampleTableName));

            return instantiatedTestClass.ToString();
        }

        string InstantiatedTestClass(string instantiationName, string exampleTableName)
        {
            StringBuilder instantiatedTestClass = new StringBuilder();

            if (!BDDUtil.SupportUnicode)
            {
                instantiatedTestClass.AppendLine("// " + instantiationName);
            }

            instantiatedTestClass
                .AppendLine("INSTANTIATE_TEST_CASE_P(")
                .Append(BDDUtil.INDENT_DOUBLE)
                .Append(BDDUtil.to_ident(instantiationName))
                .AppendLine(",");

            instantiatedTestClass
                .Append(BDDUtil.INDENT_DOUBLE)
                .Append(BDDUtil.to_ident(ScenarioOutlineClassName))
                .AppendLine(",")
                .Append(BDDUtil.INDENT_DOUBLE)
                .Append("testing::ValuesIn(")
                .AppendLine(exampleTableName + ".Rows()));");

            return instantiatedTestClass.ToString();
        }

        string BuildParameters(IEnumerable<TableRow> rows)
        {
            StringBuilder parameters = new StringBuilder();
            int ROWS = rows.Count();

            int index = 0;
            foreach (TableRow row in rows)
            {
                parameters
                    .Append(BDDUtil.INDENT_DOUBLE_PLUS)
                    .Append(BDDUtil.ParameterClassName + "(")
                    .Append("L\"")
                    .Append(row.TrimmedFormattedText)
                    .Append("\")");

                index++;
                if (index < ROWS)
                {
                    parameters.AppendLine(",");
                }
                else
                {
                    parameters.AppendLine();
                }
            }

            return parameters.ToString();
        }
    }
}
