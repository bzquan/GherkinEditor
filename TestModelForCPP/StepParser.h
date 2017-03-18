#pragma once

#include <string>
#include <vector>
#include "BDDStepArg.h"

namespace bdd
{
    class GherkinTable;
    class GherkinRow;

    class StepParser
    {
    public:
        static std::wstring Parse(std::wstring step_text);
        static std::wstring Parse(std::wstring step_text, const std::wstring& docStringArg);
        static std::wstring Parse(std::wstring step_text, const GherkinTable&);

    private:
        static std::wstring Parse(std::wstring step_text, std::vector<BDDStepArg>& argList);
        static void ParseParams(std::wstring step_text, std::vector<BDDStepArg>& argList);
        static std::wstring CreateStepPattern(std::wstring step_text, std::vector<BDDStepArg>& argList);
        static std::wstring ToParamList(std::vector<BDDStepArg>& argList);
        static std::wstring FindRowParam(std::vector<BDDStepArg>& argList);
        static size_t CountActualStepParams(std::vector<BDDStepArg>& argList);
    };
}
