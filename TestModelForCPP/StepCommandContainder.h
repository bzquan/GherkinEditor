#pragma once

#include <string>
#include <map>
#include "IStepCommand.h"
#include "Signal.h"

namespace bdd
{
    class StepParameters;

    class StepCommandContainder : public has_slots<>
    {
        typedef std::map<std::wstring, std::map<std::wstring, IStepCommand*>> TStepMap;
    public:
        StepCommandContainder(){}

        void Register(IStepCommand* step);
        void Unregister(IStepCommand* step);

        // slots
        void OnStartTest(const std::wstring feature_name);
        void OnEndTest(const std::wstring feature_name);
        void OnExecuteStep(std::wstring step_text, StepParameters& params);

    private:
        IStepCommand* GetStep(const std::wstring step_text);
        StepCommandContainder(bool is_persistent);
        void TryAddStepMap(std::wstring feature);

    private:
        static StepCommandContainder s_persistent;
        static TStepMap s_container;
        static std::wstring s_FeatureToRun;
    };
}
