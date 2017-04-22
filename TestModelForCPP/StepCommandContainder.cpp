#include "StringUtility.h"
#include "EventAggregator4BDDTest.h"
#include "StepParameters.h"
#include "UndefinedStepException.h"
#include "StepCommandContainder.h"

using namespace bdd;

StepCommandContainder& StepCommandContainder::Instance()
{
	static StepCommandContainder s_Singleton;

	return s_Singleton;
}

StepCommandContainder::StepCommandContainder()
{
	EventAggregator4BDDTest::SetupEvent().connect(this, &StepCommandContainder::OnStartTest);
	EventAggregator4BDDTest::TearDownEvent().connect(this, &StepCommandContainder::OnEndTest);
	EventAggregator4BDDTest::ExecuteStepEvent().connect(this, &StepCommandContainder::OnExecuteStep);
}

void StepCommandContainder::Register(IStepCommand* pStep)
{
    TryAddStepMap(pStep->m_FeatureName);

    std::map<std::wstring, IStepCommand*>& cmd_map = m_StepMap[pStep->m_FeatureName];
    cmd_map[pStep->m_StepPattern] = pStep;
}

void StepCommandContainder::Unregister(IStepCommand* pStep)
{
    std::map<std::wstring, IStepCommand*>& cmd_map = m_StepMap[pStep->m_FeatureName];
    cmd_map.erase(pStep->m_StepPattern);
}

IStepCommand* StepCommandContainder::GetStep(const std::wstring step_text)
{
    StepCommandContainder::TStepMap::iterator iter_feature = m_StepMap.find(m_FeatureToRun);
    if (iter_feature == m_StepMap.end())
    {
        return NULL;
    }

    std::map<std::wstring, IStepCommand*>& cmd_map = m_StepMap[m_FeatureToRun];

    for (std::map<std::wstring, IStepCommand*>::iterator iter = cmd_map.begin(); iter != cmd_map.end(); ++iter)
    {
        IStepCommand* pCmd = iter->second;
        if (pCmd->IsMatch(step_text)) return pCmd;
    }

    return NULL;
}

void StepCommandContainder::TryAddStepMap(std::wstring feature)
{
    StepCommandContainder::TStepMap::iterator iter = m_StepMap.find(feature);
    if (iter == m_StepMap.end())
    {
        m_StepMap[feature] = std::map<std::wstring, IStepCommand*>();
    }
}

void StepCommandContainder::OnStartTest(const std::wstring feature_name)
{
    m_FeatureToRun = StringUtility::to_lower_case(feature_name);
}

void StepCommandContainder::OnEndTest(const std::wstring)
{
    m_FeatureToRun.clear();
}

void StepCommandContainder::OnExecuteStep(std::wstring step_text, StepParameters& params)
{
    IStepCommand* step_command = GetStep(step_text);
    if (NULL != step_command)
    {
        params.Append(step_command->m_StepPattern, step_text);
        step_command->Execute(params);
    }
    else
    {
        std::wstring msg;
        msg
            .append(L"Undefined step : ")
            .append(step_text)
            .append(L"(")
            .append(m_FeatureToRun)
            .append(L")");

		std::string ex_msg = StringUtility::wstring2string(msg);

        throw UndefinedStepException(ex_msg);
    }
}
