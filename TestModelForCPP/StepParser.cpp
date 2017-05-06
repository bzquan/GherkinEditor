#if (_MSC_VER < 1900)   // _MSC_VER == 1900 (Visual Studio 2015, MSVC++ 14.0)
#include <boost/regex.hpp>
using namespace boost;
#else
#include <regex>
using namespace std;
#endif

#include "GherkinTable.h"
#include "StringUtility.h"
#include "RegexSubstituter.h"
#include "StepParser.h"

using namespace bdd;

namespace
{
    wstring PreEscapeSpecialCharacters(const wstring& text)
    {
        wstring result_text(text);
        StringUtility::ReplaceAll(result_text, L"{", L"\\{");
        StringUtility::ReplaceAll(result_text, L"}", L"\\}");
        StringUtility::ReplaceAll(result_text, L"(", L"\\(");
        StringUtility::ReplaceAll(result_text, L")", L"\\)");
        StringUtility::ReplaceAll(result_text, L"[", L"\\[");
        StringUtility::ReplaceAll(result_text, L"]", L"\\]");

        return result_text;
    }

    wstring PostEscapeSpecialCharacters(const wstring& text)
    {
        wstring result_text(text);
        StringUtility::ReplaceAll(result_text, L"\\{", L"{");
        StringUtility::ReplaceAll(result_text, L"\\}", L"}");
        StringUtility::ReplaceAll(result_text, L"\\(", L"(");
        StringUtility::ReplaceAll(result_text, L"\\)", L")");
        StringUtility::ReplaceAll(result_text, L"\\[", L"[");
        StringUtility::ReplaceAll(result_text, L"\\]", L"]");

        return result_text;
    }
}

std::wstring StepParser::Parse(std::wstring step_text)
{
    std::vector<BDDStepArg> argList;
    return Parse(step_text, argList);
}

std::wstring StepParser::Parse(std::wstring step_text, const std::wstring& docStringArg)
{
    std::vector<BDDStepArg> argList;
    argList.push_back(BDDStepArg(BDDStepArg::DocStringArg));

    return Parse(step_text, argList);
}

std::wstring StepParser::Parse(std::wstring step_text, const GherkinTable&)
{
    std::vector<BDDStepArg> argList;
    argList.push_back(BDDStepArg(BDDStepArg::TableArg));

    return Parse(step_text, argList);
}

std::wstring StepParser::Parse(std::wstring step_text, std::vector<BDDStepArg>& argList)
{
    ParseParams(step_text, argList);
    std::wstring step_pattern = CreateStepPattern(step_text, argList);
    size_t actual_step_param_count = CountActualStepParams(argList);

    std::wstring step_imp;
    step_imp
        .append(L"STEP")
        .append(StringUtility::itows(actual_step_param_count))
        .append(L"(")
        .append(L"\"")
        .append(step_pattern)
        .append(L"\"")
        .append(ToParamList(argList))
        .append(L")\n")
        .append(L"{\n")
        .append(L"}\n");

    return step_imp;
}

void StepParser::ParseParams(std::wstring step_text, std::vector<BDDStepArg>& argList)
{
    wregex  stepRegex(RegexSubstituter::StepPattern());
    wsmatch match;
    wstring text(step_text);
    int numArg = 1;
    int strArg = 1;
    while (regex_search(text, match, stepRegex)) {
        BDDStepArg arg(match[0]);
        if ((arg.ArgType() == BDDStepArgType::IntArg) ||
            (arg.ArgType() == BDDStepArgType::doubleArg))
        {
            arg.ArgIndex = numArg++;
        }
        else if (arg.ArgType() == BDDStepArgType::StringArg)
        {
            arg.ArgIndex = strArg++;
        }
        argList.push_back(arg);

        text = match.suffix().str();
    }
}

std::wstring StepParser::CreateStepPattern(std::wstring step_text, std::vector<BDDStepArg>& argList)
{
    static std::wstring paramIndicator(L"QQQQQQ");

    std::wstring preEscapedText = PreEscapeSpecialCharacters(step_text);
    wregex  stepRegex(RegexSubstituter::StepPattern());
    wstring stepRegexText = regex_replace(preEscapedText, stepRegex, paramIndicator);
	for (std::vector<BDDStepArg>::iterator iter = argList.begin(); iter != argList.end(); ++iter)
    {
		BDDStepArg& stepArg = *iter;
        switch (stepArg.ArgType())
        {
        case BDDStepArgType::TableColumnArg:
        case BDDStepArgType::IntArg:
        case BDDStepArgType::doubleArg:
        case BDDStepArgType::StringArg:
            StringUtility::ReplaceFirst(stepRegexText, paramIndicator, stepArg.RegexPattern());
            break;
        default:
            // do nothing
            break;
        }
    }

    return PostEscapeSpecialCharacters(stepRegexText);
}

std::wstring StepParser::ToParamList(std::vector<BDDStepArg>& argList)
{
    std::wstring params;

    // Add only one Gherkin row parameter at the beginning
    std::wstring tableRowArg = FindRowParam(argList);
    if (tableRowArg.length() > 0)
    {
        params.append(L", " + tableRowArg);
    }

    for (std::vector<BDDStepArg>::iterator iter = argList.begin(); iter != argList.end(); ++iter)
    {
        if (iter->ArgType() != BDDStepArgType::TableColumnArg)
        {
            params.append(L", " + iter->GetStepParam());
        }
    }

    return params;
}

std::wstring StepParser::FindRowParam(std::vector<BDDStepArg>& argList)
{
    for (std::vector<BDDStepArg>::iterator iter = argList.begin(); iter != argList.end(); ++iter)
    {
        if (iter->ArgType() == BDDStepArgType::TableColumnArg) return iter->GetStepParam();
    }

    return L"";
}

// Count Gherkin column argument only once
size_t StepParser::CountActualStepParams(std::vector<BDDStepArg>& argList)
{
    size_t column_arg_count = 0;
    for (std::vector<BDDStepArg>::iterator iter = argList.begin(); iter != argList.end(); ++iter)
    {
        if (iter->ArgType() == BDDStepArgType::TableColumnArg) column_arg_count++;
    }

    size_t count = (column_arg_count > 1) ? argList.size() - (column_arg_count - 1) : argList.size();

    return count;
}
