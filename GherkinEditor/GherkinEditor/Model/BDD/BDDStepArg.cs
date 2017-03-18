using System.Text.RegularExpressions;

namespace CucumberCpp
{
    enum BDDStepArgType { TableArg, DocStringArg, IntArg, FloatArg, StringArg, TableColumnArg }

    class BDDStepArg
    {
        public const string TableArg = "$table$";
        public const string DocStringArg = "$doc$";
        public BDDStepArg(string arg)
        {
            RegexPattern = BDDUtil.StringRegex; // default is a string argument
            ArgText = arg;
            DecideArgType(arg);
        }

        public int ArgIndex { get; set; }
        public string RegexPattern { get; set; }
        public BDDStepArgType ArgType { get; set; }
        string ArgText { get; set; }

        public string StepParam
        {
            get
            {
                switch (ArgType)
                {
                    case BDDStepArgType.TableArg:
                        return "GherkinTable&, table";
                    case BDDStepArgType.TableColumnArg:
                        return "GherkinRow&, row";
                    case BDDStepArgType.DocStringArg:
                        return "std::wstring, docStr";
                    case BDDStepArgType.IntArg:
                        return "int, num" + ArgIndex.ToString();
                    case BDDStepArgType.FloatArg:
                        return "double, num" + ArgIndex.ToString();
                    case BDDStepArgType.StringArg:
                        return "std::wstring, str" + ArgIndex.ToString();
                    default:
                        return "std::wstring, str" + ArgIndex.ToString();
                }
            }
        }

        public string StepParamTypeName
        {
            get
            {
                switch (ArgType)
                {
                    case BDDStepArgType.TableArg:
                        return "GherkinTable&";
                    case BDDStepArgType.TableColumnArg:
                        return "GherkinRow&";
                    case BDDStepArgType.DocStringArg:
                        return "wstring";
                    case BDDStepArgType.FloatArg:
                        return "double";
                    case BDDStepArgType.StringArg:
                        return "wstring";
                    default:
                        return "wstring";
                }
            }
        }

        public string StepFunctionPlaceHolder
        {
            get
            {
                switch (ArgType)
                {
                    case BDDStepArgType.TableArg:
                        return "_T_";
                    case BDDStepArgType.TableColumnArg:
                        string tableColumnName = ArgText.Substring(1, ArgText.Length - 2);
                        return BDDUtil.MakeIdentifier(tableColumnName);
                    case BDDStepArgType.DocStringArg:
                        return "_S_";
                    case BDDStepArgType.IntArg:
                        return "_I_";
                    case BDDStepArgType.FloatArg:
                        return "_D_";
                    case BDDStepArgType.StringArg:
                        return "_S_";
                    default:
                        return "";
                }
            }
        }

        void DecideArgType(string arg)
        {
            if (arg == TableArg)
            {
                ArgType = BDDStepArgType.TableArg;
            }
            else if (arg == DocStringArg)
            {
                ArgType = BDDStepArgType.DocStringArg;
            }
            else if ((arg.Length == 0) || (arg[0] == '\"'))
            {
                ArgType = BDDStepArgType.StringArg;
                RegexPattern = BDDUtil.StringRegex;
            }
            else if ((arg[0] == '<') && (arg[arg.Length - 1] == '>'))
            {
                ArgType = BDDStepArgType.TableColumnArg;
                RegexPattern = arg;
            }
            else if (!arg.Contains("."))
            {
                ArgType = BDDStepArgType.IntArg;
                RegexPattern = BDDUtil.IntRegex;
            }
            else
            {
                ArgType = BDDStepArgType.FloatArg;
                RegexPattern = BDDUtil.FloatRegex;
            }
        }
    }
}
