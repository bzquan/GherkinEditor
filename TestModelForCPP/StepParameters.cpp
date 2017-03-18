#if (_MSC_VER < 1900)   // _MSC_VER == 1900 (Visual Studio 2015, MSVC++ 14.0)
#include <boost/regex.hpp>
using namespace boost;
#else
#include <regex>
using namespace std;
#endif

#include "StepParameters.h"

using namespace bdd;

void StepParameters::Append(bdd::GherkinTable& table)
{
    m_Params.push_back(StepParam(table));
}

void StepParameters::Append(bdd::GherkinRow& row)
{
    m_Params.push_back(StepParam(row));
}

void StepParameters::Append(std::wstring doc_string)
{
    m_Params.push_back(StepParam(doc_string));
}

void StepParameters::Append(std::wstring step_pattern, std::wstring step_text)
{
    wregex exp(step_pattern);
    wsmatch match;
    if (!regex_match(step_text, match, exp)) return;

    for (size_t i = 1; i < match.size(); i++)
    {
        m_Params.push_back(DeduceParam(match[i]));
    }
}

StepParam StepParameters::DeduceParam(std::wstring value)
{
    if (value[0] == L'\"')
    {
        std::wstring quotation_removed = value.substr(1, value.length() - 2);
        return StepParam(quotation_removed);
    }
    else if (value.find(L'.') == std::wstring::npos)
    {
        int v = StringUtility::stoi(value);
        return StepParam(v);
    }
    else
    {
        double v = StringUtility::stod(value);
        return StepParam(v);
    }
}

StepParam& StepParameters::GetParam(size_t index)
{
    static StepParam s_empty;

    return (index < m_Params.size()) ? m_Params[index] : s_empty;
}
