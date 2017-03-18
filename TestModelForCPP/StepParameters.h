#pragma once

#include <vector>
#include "StepParam.h"

namespace bdd
{
    class StepParameters
    {
    public:
        void Append(bdd::GherkinTable& table);
        void Append(bdd::GherkinRow& row);
        void Append(std::wstring doc_string);
        void Append(std::wstring step_pattern, std::wstring step_text);

        StepParam& GetParam(size_t index);
        StepParam& operator[] (size_t index) { return GetParam(index); }
    private:
        StepParam DeduceParam(std::wstring value);

    private:
        std::vector<StepParam> m_Params;
    };
}
