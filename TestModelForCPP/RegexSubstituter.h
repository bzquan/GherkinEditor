#pragma once

#include <string>

namespace bdd
{
    class RegexSubstituter
    {
    public:
        static std::wstring IntRegex();
        static std::wstring FloatRegex();
        static std::wstring StringRegex();
        static std::wstring RowParamRegex();
        static std::wstring StepPattern();

    public:
        static void ExpandRegex(std::wstring& step_pattern);
        static void CurryRegex(std::wstring& step_pattern);

    private:
        static std::wstring IntRegexSymbol();
        static std::wstring FloatRegexSymbol();
        static std::wstring StringRegexSymbol();
        static std::wstring MockAttrSymbol();
        static std::wstring MockAttrRegex();
    };
}
