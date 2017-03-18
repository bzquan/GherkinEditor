#include "StringUtility.h"
#include "EventAggregator4BDDTest.h"
#include "SetupTearDownSlot.h"

using namespace bdd;

bool SetupTearDownSlot::IsCurrentFeature(const std::wstring& feature)
{
    std::wstring current_feature = StringUtility::to_lower_case(feature);
    return (current_feature == m_feature);
}

SetupSlot::SetupSlot(const std::wstring feature) :
    SetupTearDownSlot(feature)
{
    EventAggregator4BDDTest::SetupEvent().connect(this, &SetupSlot::OnSetup);
}

void SetupSlot::OnSetup(std::wstring feature)
{
    if (IsCurrentFeature(feature)) this->DoSetup();
}

TearDownSlot::TearDownSlot(const std::wstring feature) :
    SetupTearDownSlot(feature)
{
    EventAggregator4BDDTest::TearDownEvent().connect(this, &TearDownSlot::OnTearDown);
}

void TearDownSlot::OnTearDown(std::wstring feature)
{
    if (IsCurrentFeature(feature)) this->DoTearDown();
}
