using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gherkin.Ast;

using System.Text.RegularExpressions;
using Gherkin.Model;

namespace CucumberCpp
{
    public class BDDStepBuilder
    {
        Regex stepRegex;
        List<BDDStepArg> origArgList = new List<BDDStepArg>();
        List<BDDStepArg> adjustdArgList;
        List<string> stepComments = new List<string>();
        string stepPattern = BDDUtil.StringRegex + "|" + BDDUtil.FloatRegex + "|" + BDDUtil.RowParamRegex;
        string StepKeyword { get; set; }
        string StepFunctionName { get; set; }
        string CachedStepImpSkeleton { get; set; }

        public string StepText { get; private set; }
        public DataTable TableArg { get; set; }
        public int TableSeqNo { get; set; }
        public DocString DocStringArg { get; set; }
        public int DocStringSeqNo { get; set; }

        public BDDStepBuilder(Step step)
        {
            StepKeyword = GetStepKeyword(step.Keyword);
            StepText = step.Text;
            stepRegex = new Regex(stepPattern);
        }

        string GetStepKeyword(string keyword)
        {
            string keyword_orig = BDDUtil.RemoveAllWhiteSpaces(keyword);

            if (GherkinKeyword.IsStepKeyword(keyword_orig, BDDStepImplBuilderContext.GherkinDialect.GivenStepKeywords))
                return "Given";
            else if (GherkinKeyword.IsStepKeyword(keyword_orig, BDDStepImplBuilderContext.GherkinDialect.WhenStepKeywords))
                return "When";
            else if (GherkinKeyword.IsStepKeyword(keyword_orig, BDDStepImplBuilderContext.GherkinDialect.ThenStepKeywords))
                return "Then";
            else if (GherkinKeyword.IsStepKeyword(keyword_orig, BDDStepImplBuilderContext.GherkinDialect.AndStepKeywords))
                return "And";
            else if (GherkinKeyword.IsStepKeyword(keyword_orig, BDDStepImplBuilderContext.GherkinDialect.ButStepKeywords))
                return "But";

            return keyword_orig;
        }

        public string StepImpSkeleton
        {
            get
            {
                if (CachedStepImpSkeleton == null)
                {
                    BuildStepImp();
                }
                return CachedStepImpSkeleton;
            }
        }

        public void AddStepComments(string newComment)
        {
            string comment = stepComments.FirstOrDefault(s => (s == newComment));
            if (comment == null)
            {
                stepComments.Add(newComment);
            }
        }

        public List<string> StepComments => stepComments;

        public bool HasMockAttribute()
        {
            return StepText.Contains(BDDUtil.MockAttrSymbol);
        }

        public string BuildStepForScenario(string indent)
        {
            MakeStepFunction();

            StringBuilder stepOfScenario = new StringBuilder();
            BuildTableArg(indent, stepOfScenario);
            BuildDocStringArg(indent, stepOfScenario);
            BuildStepStatement(indent, stepOfScenario);

            return stepOfScenario.ToString();
        }

        private void BuildStepImp()
        {
            MakeStepFunction();

            int arg_num = this.adjustdArgList.Count;
            string step_pattern = MakeStepRegex();
            string step_name = CurryRegex(step_pattern);

            StringBuilder stepImp = new StringBuilder();
            stepImp
                .Append("STEP")
                .Append(arg_num)
                .Append("(")
                .Append("\"" + step_name + "\"")
                .Append(BuildStepFormalArg())
                .AppendLine(")")
                .AppendLine("{")
                .AppendLine(BDDUtil.INDENT + "PendingStep(L\"" + StepFunctionName + "\");")
                .Append("}");

            CachedStepImpSkeleton = stepImp.ToString();
        }

        private string CurryRegex(string step_pattern)
        {
            StringBuilder step_name = new StringBuilder(step_pattern);

            step_name.Replace(BDDUtil.IntRegex, BDDUtil.IntRegexSymbol);
            step_name.Replace(BDDUtil.FloatRegex, BDDUtil.FloatRegexSymbol);
            step_name.Replace(BDDUtil.StringRegex, BDDUtil.StringRegexSymbol);
            step_name.Replace(BDDUtil.MockAttrRegex, BDDUtil.MockAttrSymbol);

            return step_name.ToString();
        }

        private void BuildTableArg(string indent, StringBuilder stepOfScenario)
        {
            if (TableArg != null)
            {
                BDDGherkinTableBuilder tableBuilder = new BDDGherkinTableBuilder();
                stepOfScenario
                    .AppendLine()
                    .Append(tableBuilder.Build(TableArg, TableSeqNo, indent));
            }
        }

        private void BuildDocStringArg(string indent, StringBuilder stepOfScenario)
        {
            if (DocStringArg != null)
            {
                string varDef = indent + "wstring " + DocStringRealArgName + " = ";
                string indentAfterVar = string.Empty.PadRight(varDef.Length, ' ');
                stepOfScenario
                    .AppendLine()
                    .Append(varDef)
                    .Append(BuildMultiLineStringConstant(indentAfterVar));
            }
        }

        private string BuildMultiLineStringConstant(string indent)
        {
            StringBuilder multiLineString = new StringBuilder();
            string docStringContent = DocStringArg.Content;
            
            string[] docStrings = docStringContent.Split(new string[]{Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < docStrings.Length; i++)
            {
                if (i > 0)
                {
                    multiLineString.Append(indent);
                }

                multiLineString.Append("L\"" + docStrings[i]);
                if (i < docStrings.Length - 1)
                    multiLineString.AppendLine("\\n\"");
                else
                    multiLineString.AppendLine("\";");
            }

            return multiLineString.ToString();
        }

        private void BuildStepStatement(string indent, StringBuilder stepOfScenario)
        {
            string stepTextByEscapingQuotaion = StepText.Replace("\"", "\\\"");

            stepOfScenario
                .Append(indent + StepKeyword + "(L\"" + stepTextByEscapingQuotaion + "\"")
                .Append(BuildStepRealArg())
                .Append(");");
        }

        private string MakeStepRegex()
        {
            string paramIndicator = "QQQQQQ";
            Regex regex = new Regex(paramIndicator);
            string preEscapedText = PreEscapeSpecialCharacters(StepText);
            string stepRegexText = stepRegex.Replace(preEscapedText, paramIndicator);
            foreach (BDDStepArg stepArg in origArgList)
            {
                switch (stepArg.ArgType)
                {
                    case BDDStepArgType.TableColumnArg:
                    case BDDStepArgType.IntArg:
                    case BDDStepArgType.FloatArg:
                    case BDDStepArgType.StringArg:
                        stepRegexText = regex.Replace(stepRegexText, stepArg.RegexPattern, 1);
                        break;
                }
            }

            return PostEscapeSpecialCharacters(stepRegexText);
        }

        private string BuildStepFormalArg()
        {
            const string delimiter = ", ";
            StringBuilder args = new StringBuilder();
            foreach (BDDStepArg stepArg in this.adjustdArgList)
            {
                args.Append(delimiter + stepArg.StepParam);
            }

            return args.ToString();
        }

        private string BuildStepRealArg()
        {
            StringBuilder args = new StringBuilder();
            foreach (BDDStepArg stepArg in adjustdArgList)
            {
                switch (stepArg.ArgType)
                {
                    case BDDStepArgType.TableArg:
                        args.Append(", table" + TableSeqNo);
                        break;
                    case BDDStepArgType.TableColumnArg:
                        args.Append(", param");
                        break;
                    case BDDStepArgType.DocStringArg:
                        args.Append(", " + DocStringRealArgName);
                        break;
                }
            }

            return args.ToString();
        }

        private string DocStringRealArgName
        {
            get { return "docString" + DocStringSeqNo; }
        }

        private void MakeAdjustedArgList()
        {
            adjustdArgList = new List<BDDStepArg>();
            if (TableArg != null)
            {
                adjustdArgList.Add(new BDDStepArg(BDDStepArg.TableArg));
            }

            BDDStepArg tableRowArg = origArgList.Find(x => x.ArgType == BDDStepArgType.TableColumnArg);
            if (tableRowArg != null)
            {
                adjustdArgList.Add(tableRowArg);
            }

            foreach (BDDStepArg stepArg in origArgList)
            {
                if ( (stepArg.ArgType != BDDStepArgType.TableArg) && (stepArg.ArgType != BDDStepArgType.TableColumnArg) )
                {
                    adjustdArgList.Add(stepArg);
                }
            }
        }

        private void MakeStepFunction()
        {
            if (StepFunctionName == null)
            {
                MakeStepArgList();
                MakeAdjustedArgList();
                MakeStepFunctionName();
            }
        }

        private void MakeStepFunctionName()
        {
            string paramIndicator = "QQQQQQ";
            Regex regex = new Regex(paramIndicator);
            string stepRegexText = stepRegex.Replace(StepText, paramIndicator);
            foreach (BDDStepArg stepArg in origArgList)
            {
                stepRegexText = regex.Replace(stepRegexText, stepArg.StepFunctionPlaceHolder, 1);
            }

            StepFunctionName = BDDUtil.MakeIdentifier(stepRegexText);
        }

        private void MakeStepArgList()
        {
            if (origArgList.Count > 0) return;  // step argument list has been created


            if (DocStringArg != null)
            {
                BDDStepArg arg = new BDDStepArg(BDDStepArg.DocStringArg);
                origArgList.Add(arg);
            }

            int numArg = 1;
            int strArg = 1;

            MatchCollection matches = stepRegex.Matches(StepText);
            foreach (Match match in matches)
            {
                BDDStepArg arg = new BDDStepArg(match.Value);
                if ((arg.ArgType == BDDStepArgType.IntArg) || (arg.ArgType == BDDStepArgType.FloatArg))
                {
                        arg.ArgIndex = numArg++;
                }
                else if (arg.ArgType == BDDStepArgType.StringArg)
                {
                    arg.ArgIndex = strArg++;
                }
                origArgList.Add(arg);
            }
        }

        private string PreEscapeSpecialCharacters(string text)
        {
            StringBuilder result_text = new StringBuilder(text);
            result_text.Replace("{", "\\{");
            result_text.Replace("}", "\\}");
            result_text.Replace("(", "\\(");
            result_text.Replace(")", "\\)");
            result_text.Replace("[", "\\[");
            result_text.Replace("]", "\\]");

            return result_text.ToString();
        }

        private string PostEscapeSpecialCharacters(string text)
        {
            StringBuilder result_text = new StringBuilder(text);
            result_text.Replace("\\{", "{");
            result_text.Replace("\\}", "}");
            result_text.Replace("\\(", "(");
            result_text.Replace("\\)", ")");
            result_text.Replace("\\[", "[");
            result_text.Replace("\\]", "]");

            return result_text.ToString();
        }
    }
}

