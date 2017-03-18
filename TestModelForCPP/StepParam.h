#pragma once

#include <string>
#include "GherkinTable.h"

namespace bdd
{
    class StepParam
    {
    public:
        StepParam() : intParam(0), doubleParam(0.0) {}
        StepParam(std::wstring v) : strParam(v), intParam(0), doubleParam(0.0) {}
        StepParam(int v) : intParam(v), doubleParam(0.0) {}
        StepParam(double v) : intParam(0), doubleParam(v) {}
        StepParam(GherkinTable& v) : intParam(0), doubleParam(0.0), tableParam(v) {}
        StepParam(GherkinRow& v) : intParam(0), doubleParam(0.0), tableRowParam(v) {}

        std::wstring strParam;
        int intParam;
        double doubleParam;
        GherkinTable tableParam;
        GherkinRow tableRowParam;
    };

    template <typename T> class StepParamGetter;

    template <> class StepParamGetter<int>
    {
    public:
        static int GetParam(StepParam& param) { return param.intParam; }
    };

    template <> class StepParamGetter<double>
    {
    public:
        static double GetParam(StepParam& param) { return param.doubleParam; }
    };

    template <> class StepParamGetter<std::wstring>
    {
    public:
        static std::wstring GetParam(StepParam& param) { return param.strParam; }
    };

    template <> class StepParamGetter<GherkinTable&>
    {
    public:
        static bdd::GherkinTable& GetParam(StepParam& param) { return param.tableParam; }
    };

    template <> class StepParamGetter<GherkinRow&>
    {
    public:
        static GherkinRow& GetParam(StepParam& param) { return param.tableRowParam; }
    };

    template<typename T>
    T GetParam(StepParam& param)
    {
        return StepParamGetter<T>::GetParam(param);
    }
}


