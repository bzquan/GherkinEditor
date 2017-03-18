#if (_MSC_VER < 1900)   // _MSC_VER == 1900 (Visual Studio 2015, MSVC++ 14.0)
#include <boost/regex.hpp>
using namespace boost;
#else
#include <regex>
using namespace std;
#endif

#include "StringUtility.h"
#include "StepCommandContainder.h"
#include "RegexSubstituter.h"
#include "IStepCommand.h"

using namespace bdd;
using namespace std;

IStepCommand::IStepCommand(const std::wstring feature, const std::wstring step_pattern) :
    m_FeatureName(feature),
    m_StepPattern(step_pattern)
{
    RegexSubstituter::ExpandRegex(m_StepPattern);
    StepCommandContainder container;
    container.Register(this);
}

IStepCommand::~IStepCommand()
{
    StepCommandContainder container;
    container.Unregister(this);
}

bool IStepCommand::IsMatch(const std::wstring step)
{
    wregex exp(m_StepPattern);
    wsmatch match;
    return regex_match(step, match, exp);
}

void IStepCommand::PendingStep(const std::wstring step)
{
    throw domain_error("Pending step : " + StringUtility::wstring2string(step));
}
