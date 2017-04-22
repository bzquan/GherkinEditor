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
		static StepCommandContainder& Instance();

        void Register(IStepCommand* step);
        void Unregister(IStepCommand* step);

        // slots
        void OnStartTest(const std::wstring feature_name);
        void OnEndTest(const std::wstring feature_name);
        void OnExecuteStep(std::wstring step_text, StepParameters& params);

    private:
		StepCommandContainder();
		StepCommandContainder(StepCommandContainder&);
		StepCommandContainder& operator=(const StepCommandContainder& other);

		IStepCommand* GetStep(const std::wstring step_text);
        void TryAddStepMap(std::wstring feature);

    private:
        TStepMap m_StepMap;
        std::wstring m_FeatureToRun;
    };
}
