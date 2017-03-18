#pragma once

#include <string>

namespace bdd
{
    struct BDDStepArgType
    {
        enum Enum { TableArg, DocStringArg, IntArg, FloatArg, StringArg, TableColumnArg };
    };

    class BDDStepArg
    {
    public:
        static const std::wstring TableArg;         // = L"$table$";
        static const std::wstring DocStringArg;     // = L"$doc$";

    public:
        BDDStepArg(std::wstring arg);

        int ArgIndex;
        std::wstring& RegexPattern() { return m_RegexPattern; }
        BDDStepArgType::Enum ArgType() { return m_ArgType; }
        std::wstring& ArgText() { return m_ArgText; }

        std::wstring GetStepParam();

    private:
        void DeduceArgType(std::wstring arg);

    private:
        std::wstring m_RegexPattern;
        BDDStepArgType::Enum m_ArgType;
        std::wstring m_ArgText;
    };
}
