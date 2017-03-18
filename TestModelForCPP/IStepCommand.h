#pragma once

#include <string>

namespace bdd
{
    class StepParameters;

    class IStepCommand
    {
    public:
        IStepCommand(const std::wstring feature, const std::wstring step_pattern);
        virtual ~IStepCommand();

        bool IsMatch(const std::wstring step);
        virtual void Execute(StepParameters& params) = 0;

        void PendingStep(const std::wstring step);

        std::wstring m_FeatureName;
        std::wstring m_StepPattern;
    };
}
