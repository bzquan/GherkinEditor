#include <locale>
#include "EventAggregator4BDDTest.h"
#include "StepParameters.h"
#include "StepParser.h"
#include "RegexSubstituter.h"
#include "AbstractBDDTest.h"
#include "UndefinedStepException.h"

using namespace bdd;

void AbstractBDDTest::SetUp(std::wstring feature_name)
{
    EventAggregator4BDDTest::SetupEvent().emit(feature_name);
}

void AbstractBDDTest::TearDown(std::wstring feature_name)
{
    EventAggregator4BDDTest::TearDownEvent().emit(feature_name);
}

void AbstractBDDTest::DoStep(std::wstring step_text)
{
    try
    {
        StepParameters params;
        EventAggregator4BDDTest::ExecuteStepEvent().emit(step_text, params);
    }
    catch (UndefinedStepException)
    {
        std::wstring recommended_step_imp = StepParser::Parse(step_text);
        DisplayRecommendedStepImp(recommended_step_imp);
        throw;
    }
}

void AbstractBDDTest::DoStep(std::wstring step_text, std::wstring docStringArg)
{
    try
    {
        StepParameters params;
        params.Append(docStringArg);
        EventAggregator4BDDTest::ExecuteStepEvent().emit(step_text, params);
    }
    catch (UndefinedStepException)
    {
        std::wstring recommended_step_imp = StepParser::Parse(step_text, docStringArg);
        DisplayRecommendedStepImp(recommended_step_imp);
        throw;
    }
}

void AbstractBDDTest::DoStep(std::wstring step_text, bdd::GherkinTable& tableArg)
{
    try
    {
        StepParameters params;
        params.Append(tableArg);
        EventAggregator4BDDTest::ExecuteStepEvent().emit(step_text, params);
    }
    catch (UndefinedStepException)
    {
        std::wstring recommended_step_imp = StepParser::Parse(step_text, tableArg);
        DisplayRecommendedStepImp(recommended_step_imp);
        throw;
    }
}

void AbstractBDDTest::DoStep(std::wstring step_text, bdd::GherkinRow& tableRowArg)
{
    try
    {
        StepParameters params;
        params.Append(tableRowArg);
        EventAggregator4BDDTest::ExecuteStepEvent().emit(step_text, params);
    }
    catch (UndefinedStepException)
    {
        std::wstring recommended_step_imp = StepParser::Parse(step_text);
        DisplayRecommendedStepImp(recommended_step_imp);
        throw;
    }
}

void AbstractBDDTest::DisplayRecommendedStepImp(std::wstring step_imp)
{
    RegexSubstituter::CurryRegex(step_imp);
    std::wcout.imbue(std::locale(""));
    std::wcout << "Recommended step implementation." << std::endl << std::endl;
    std::wcout << step_imp << std::endl;
}
