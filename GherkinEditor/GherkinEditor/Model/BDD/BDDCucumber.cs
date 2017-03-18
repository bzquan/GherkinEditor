using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Ast;
using Gherkin;
using System.IO;
using Gherkin.Util;

namespace CucumberCpp
{
    public class BDDCucumber
    {
        public string GenCucumberTestCode(TextReader reader, string outputPath)
        {
            try
            {
                GherkinDocument gherkinDocument = ParseFeature(reader);
                string generated_file_names = GenerateBDDTestCodes(gherkinDocument.Feature, outputPath);
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg("Completed C++ test code"));

                return generated_file_names;
            }
            catch
            {
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg("Error"));
                throw;
            }
        }

        GherkinDocument ParseFeature(TextReader reader)
        {
            var parser = new Parser();
            parser.StopAtFirstError = true;
            return parser.Parse(reader);
        }

        string GenerateBDDTestCodes(Feature feature, string outputFileDir)
        {
            BDDASTVisitor visitor = new BDDASTVisitor();
            BDDStepImplBuilderContext.StartBuildFeature(feature);
            visitor.BuildCPPTestCode(feature);

            // Build step implementation
            string stepImplFilePath = Path.Combine(outputFileDir, visitor.StepImplFileName);
            string stepImplTemplate = visitor.StepDefsImpl;
            WriteToOutputFile(stepImplFilePath, stepImplTemplate);

            // Build Feature test code
            string featureFilePath = Path.Combine(outputFileDir, visitor.FeatureFileName);
            WriteToOutputFile(featureFilePath, visitor.FeatureImpl);

            StringBuilder sb = new StringBuilder();
            sb
                .AppendLine(stepImplFilePath)
                .Append(featureFilePath);

            return sb.ToString();
        }

        void WriteToOutputFile(string featureOutputFilePath, string formattedText)
        {
            using (StreamWriter writer = new StreamWriter(featureOutputFilePath, false, Encoding.UTF8))
            {
                writer.Write(formattedText);
            }
        }
    }
}
