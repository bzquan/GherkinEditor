#pragma once

#include "gmock/gmock.h"
#include "gtest/gtest.h"

#include "GherkinTable.h"

using namespace ::testing;

namespace bdd
{
    class AbstractBDDTest : public testing::Test
    {
    public:
        void Given(std::wstring step_text) { DoStep(step_text); }
        void Given(std::wstring step_text, std::wstring docStringArg) { DoStep(step_text, docStringArg); }
        void Given(std::wstring step_text, bdd::GherkinTable& tableArg) { DoStep(step_text, tableArg); }
        void Given(std::wstring step_text, bdd::GherkinRow& tableRowArg) { DoStep(step_text, tableRowArg); }

        void When(std::wstring step_text) { DoStep(step_text); }
        void When(std::wstring step_text, std::wstring docStringArg) { DoStep(step_text, docStringArg); }
        void When(std::wstring step_text, bdd::GherkinTable& tableArg) { DoStep(step_text, tableArg); }
        void When(std::wstring step_text, bdd::GherkinRow& tableRowArg) { DoStep(step_text, tableRowArg); }

        void Then(std::wstring step_text) { DoStep(step_text); }
        void Then(std::wstring step_text, std::wstring docStringArg) { DoStep(step_text, docStringArg); }
        void Then(std::wstring step_text, bdd::GherkinTable& tableArg) { DoStep(step_text, tableArg); }
        void Then(std::wstring step_text, bdd::GherkinRow& tableRowArg) { DoStep(step_text, tableRowArg); }

        void And(std::wstring step_text) { DoStep(step_text); }
        void And(std::wstring step_text, std::wstring docStringArg) { DoStep(step_text, docStringArg); }
        void And(std::wstring step_text, bdd::GherkinTable& tableArg) { DoStep(step_text, tableArg); }
        void And(std::wstring step_text, bdd::GherkinRow& tableRowArg) { DoStep(step_text, tableRowArg); }

        void But(std::wstring step_text) { DoStep(step_text); }
        void But(std::wstring step_text, std::wstring docStringArg) { DoStep(step_text, docStringArg); }
        void But(std::wstring step_text, bdd::GherkinTable& tableArg) { DoStep(step_text, tableArg); }
        void But(std::wstring step_text, bdd::GherkinRow& tableRowArg) { DoStep(step_text, tableRowArg); }

        void Spec(const char* guid)
        {
            std::cout << guid << std::endl;
        }

    protected:
        void SetUp(std::wstring feature_name);
        void TearDown(std::wstring feature_name);

    private:
        void DoStep(std::wstring step_text);
        void DoStep(std::wstring step_text, std::wstring docStringArg);
        void DoStep(std::wstring step_text, bdd::GherkinTable& tableArg);
        void DoStep(std::wstring step_text, bdd::GherkinRow& tableRowArg);
        void DisplayRecommendedStepImp(std::wstring step_imp);
    };
}
