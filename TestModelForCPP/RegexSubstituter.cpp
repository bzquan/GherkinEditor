#include "StringUtility.h"
#include "RegexSubstituter.h"

using namespace bdd;

void RegexSubstituter::ExpandRegex(std::wstring& step_pattern)
{
    StringUtility::ReplaceAll(step_pattern, IntRegexSymbol(), IntRegex());
    StringUtility::ReplaceAll(step_pattern, FloatRegexSymbol(), FloatRegex());
    StringUtility::ReplaceAll(step_pattern, StringRegexSymbol(), StringRegex());
    StringUtility::ReplaceAll(step_pattern, MockAttrSymbol(), MockAttrRegex());
}

void RegexSubstituter::CurryRegex(std::wstring& step_pattern)
{
    StringUtility::ReplaceAll(step_pattern, IntRegex(), IntRegexSymbol());
    StringUtility::ReplaceAll(step_pattern, FloatRegex(), FloatRegexSymbol());
    StringUtility::ReplaceAll(step_pattern, StringRegex(), StringRegexSymbol());
    StringUtility::ReplaceAll(step_pattern, MockAttrRegex(), MockAttrSymbol());
}

// On demand singleton function
#define SINGLETON_SYMBOL(arg)\
static std::wstring s_Singleton(arg);\
return s_Singleton;

std::wstring RegexSubstituter::IntRegexSymbol()
{
	SINGLETON_SYMBOL(L"(_INT_)");
}

std::wstring RegexSubstituter::IntRegex()
{
	SINGLETON_SYMBOL(L"([-+]?\\d[\\d,]*)");
}

std::wstring RegexSubstituter::FloatRegexSymbol()
{
	SINGLETON_SYMBOL(L"(_DOUBLE_)");
}

std::wstring RegexSubstituter::FloatRegex()
{
	SINGLETON_SYMBOL(L"([-+]?\\d[\\d,]*\\.\\d+)");
}

std::wstring RegexSubstituter::StringRegexSymbol()
{
	SINGLETON_SYMBOL(L"(_STR_)");
}

std::wstring RegexSubstituter::StringRegex()
{
	SINGLETON_SYMBOL(L"(\"[^\"]*\")");
}

std::wstring RegexSubstituter::MockAttrSymbol()
{
	SINGLETON_SYMBOL(L"[[mock]]");
}

std::wstring RegexSubstituter::MockAttrRegex()
{
	SINGLETON_SYMBOL(L"\\[\\[mock\\]\\]");
}

std::wstring RegexSubstituter::RowParamRegex()
{
	SINGLETON_SYMBOL(L"(<[^>]+>)");
}

std::wstring RegexSubstituter::StepPattern()
{
	SINGLETON_SYMBOL(StringRegex() + L"|" + FloatRegex() + L"|" + RowParamRegex());
}
