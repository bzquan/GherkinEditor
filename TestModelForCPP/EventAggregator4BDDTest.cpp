#include "EventAggregator4BDDTest.h"

using namespace bdd;

// On demand singleton function
#define SINGLETON_FUNC_BODY(T)\
static T s_Singleton;\
return s_Singleton;

TSetupEvent& EventAggregator4BDDTest::SetupEvent()
{
    SINGLETON_FUNC_BODY(TSetupEvent);
}

TTearDownEvent& EventAggregator4BDDTest::TearDownEvent()
{
    SINGLETON_FUNC_BODY(TTearDownEvent);
}

TExecuteStepEvent& EventAggregator4BDDTest::ExecuteStepEvent()
{
    SINGLETON_FUNC_BODY(TExecuteStepEvent);
}
