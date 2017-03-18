#include "StringUtility.h"
#include "RegexSubstituter.h"

using namespace bdd;

std::wstring RegexSubstituter::IntRegexSymbol(L"(_INT_)");
std::wstring RegexSubstituter::IntRegex(L"([-+]?\\d[\\d,]*)");
std::wstring RegexSubstituter::FloatRegexSymbol(L"(_DOUBLE_)");
std::wstring RegexSubstituter::FloatRegex(L"([-+]?\\d[\\d,]*\\.\\d+)");
std::wstring RegexSubstituter::StringRegexSymbol(L"(_STR_)");
std::wstring RegexSubstituter::StringRegex(L"(\"[^\"]*\")");
std::wstring RegexSubstituter::MockAttrSymbol(L"[[mock]]");
std::wstring RegexSubstituter::MockAttrRegex(L"\\[\\[mock\\]\\]");
std::wstring RegexSubstituter::RowParamRegex(L"(<[^>]+>)");
std::wstring RegexSubstituter::StepPattern(StringRegex + L"|" + FloatRegex + L"|" + RowParamRegex);

void RegexSubstituter::ExpandRegex(std::wstring& step_pattern)
{
    StringUtility::ReplaceAll(step_pattern, IntRegexSymbol, IntRegex);
    StringUtility::ReplaceAll(step_pattern, FloatRegexSymbol, FloatRegex);
    StringUtility::ReplaceAll(step_pattern, StringRegexSymbol, StringRegex);
    StringUtility::ReplaceAll(step_pattern, MockAttrSymbol, MockAttrRegex);
}

void RegexSubstituter::CurryRegex(std::wstring& step_pattern)
{
    StringUtility::ReplaceAll(step_pattern, IntRegex, IntRegexSymbol);
    StringUtility::ReplaceAll(step_pattern, FloatRegex, FloatRegexSymbol);
    StringUtility::ReplaceAll(step_pattern, StringRegex, StringRegexSymbol);
    StringUtility::ReplaceAll(step_pattern, MockAttrRegex, MockAttrSymbol);
}
