#pragma once

#include <string>
#include "Signal.h"

namespace bdd
{
    class StepParameters;

    typedef  signal1<const std::wstring> TSetupEvent;
    typedef  signal1<const std::wstring> TTearDownEvent;
    typedef  signal2<std::wstring, StepParameters&> TExecuteStepEvent;

    class EventAggregator4BDDTest
    {
    public:
        static TSetupEvent& SetupEvent();
        static TTearDownEvent& TearDownEvent();
        static TExecuteStepEvent& ExecuteStepEvent();
    };
}
