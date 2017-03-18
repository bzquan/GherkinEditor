using System.Collections.Generic;
using System.Text;

namespace CucumberCpp
{
    class BDDStepImplCppBuilder
    {
        public string BuildStepImplCpp()
        {
            StringBuilder stepImplCpp = new StringBuilder();
            stepImplCpp
                .AppendLine(BuildInclude())
                .AppendLine()
                .AppendLine(BuildStepImps());

            return stepImplCpp.ToString();
        }

        string BuildInclude()
        {
            StringBuilder includes = new StringBuilder();
            includes
                .AppendLine("#include \"StepCommand.h\"")
                .AppendLine()
                .Append("using namespace bdd;");

            return includes.ToString();
        }

        string BuildStepImps()
        {
            StringBuilder stepImps = new StringBuilder();
            stepImps
                .AppendLine()
                .AppendLine("// The model object will be created at the beginning of SETUP and")
                .AppendLine("// will be deleted at the end of TEAR_DOWN.")
                .AppendLine()
                .AppendLine("// YourTestModelClass* model;")
                .AppendLine()
                .AppendLine("SETUP(YourTestModelClass)")
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine()
                .AppendLine("TEAR_DOWN()")
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine();

            List<string> stepBuilt = new List<string>();

            foreach (BDDStepBuilder stepBuilder in BDDStepImplBuilderContext.NonDuplicatedStepBuilders)
            {
                stepImps
                    .Append(MakeStepComments(stepBuilder))
                    .AppendLine(stepBuilder.StepImpSkeleton)
                    .AppendLine();
            }

            return stepImps.ToString();
        }

        string MakeStepComments(BDDStepBuilder stepBuilder)
        {
            StringBuilder comments = new StringBuilder();
            foreach (string str in stepBuilder.StepComments)
            {
                comments.AppendLine("// " + str);
            }

            return comments.ToString();
        }
    }
}
