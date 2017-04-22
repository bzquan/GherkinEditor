#include "RegexSubstituter.h"
#include "StringUtility.h"
#include "BDDStepArg.h"

using namespace std;
using namespace bdd;

const std::wstring BDDStepArg::TableArg(L"$table$");
const std::wstring BDDStepArg::DocStringArg(L"$doc$");

BDDStepArg::BDDStepArg(wstring arg) :
	ArgIndex(0),
	m_RegexPattern(RegexSubstituter::StringRegex()), // default is a string argument
	m_ArgText(arg)
{
    DeduceArgType(arg);
}

wstring BDDStepArg::GetStepParam()
{
    switch (m_ArgType)
    {
    case BDDStepArgType::TableArg:
        return L"GherkinTable&, table";
    case BDDStepArgType::TableColumnArg:
        return L"GherkinRow&, row";
    case BDDStepArgType::DocStringArg:
        return L"std::wstring, docStr";
    case BDDStepArgType::IntArg:
        return wstring(L"int, num") + StringUtility::itows(ArgIndex);
    case BDDStepArgType::FloatArg:
        return wstring(L"double, num") + StringUtility::itows(ArgIndex);
    case BDDStepArgType::StringArg:
        return wstring(L"std::wstring, str") + StringUtility::itows(ArgIndex);
    default:
        return wstring(L"std::wstring, str") + StringUtility::itows(ArgIndex);
    }
}

void BDDStepArg::DeduceArgType(wstring arg)
{
    if (arg == TableArg)
    {
        m_ArgType = BDDStepArgType::TableArg;
    }
    else if (arg == DocStringArg)
    {
        m_ArgType = BDDStepArgType::DocStringArg;
    }
    else if ((arg.length() == 0) || (arg[0] == L'\"'))
    {
        m_ArgType = BDDStepArgType::StringArg;
        m_RegexPattern = RegexSubstituter::StringRegex();
    }
    else if ((arg[0] == L'<') && (arg[arg.length() - 1] == L'>'))
    {
        m_ArgType = BDDStepArgType::TableColumnArg;
        m_RegexPattern = arg;
    }
    else if (arg.find(L'.') == std::wstring::npos)
    {
        m_ArgType = BDDStepArgType::IntArg;
        m_RegexPattern = RegexSubstituter::IntRegex();
    }
    else
    {
        m_ArgType = BDDStepArgType::FloatArg;
        m_RegexPattern = RegexSubstituter::FloatRegex();
    }
}
