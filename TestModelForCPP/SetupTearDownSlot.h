#pragma once

#include "Signal.h"
#include <string>

namespace bdd
{
    class SetupTearDownSlot : public has_slots<>
    {
    public:
        SetupTearDownSlot(const std::wstring feature) : m_feature(feature) {}

    protected:
        bool IsCurrentFeature(const std::wstring& feature);

    private:
        std::wstring m_feature;
    };

    class SetupSlot : public SetupTearDownSlot
    {
    public:
        SetupSlot(const std::wstring feature);

    protected:
        virtual void DoSetup() {}

    private:
        void OnSetup(std::wstring feature);
    };

    class TearDownSlot : public SetupTearDownSlot
    {
    public:
        TearDownSlot(const std::wstring feature);

    protected:
        virtual void DoTearDown() {}

    private:
        void OnTearDown(std::wstring feature);
    };
}
