#include "StringUtility.h"
#include "PathToFeature.h"

using namespace bdd;

std::wstring PathToFeature::ToFeature(const std::wstring file_path)
{
    static std::wstring __STEP(L"_steps");
    static size_t LEN = __STEP.length();

    if (file_path.length() == 0) return std::wstring();

    std::wstring feature_name = StringUtility::to_lower_case(GetFileNameFromFilePath(file_path));
    const size_t feature_name_len = feature_name.length();
    if ((feature_name_len >= LEN) && (feature_name.compare(feature_name_len - LEN, LEN, __STEP) == 0))
    {
        feature_name.resize(feature_name_len - LEN);
    }

    return feature_name;
}

std::wstring PathToFeature::GetFileNameFromFilePath(const std::wstring& path)
{
#ifdef __GNUC__
    wchar_t seperator = L'/';
#else
    wchar_t seperator = L'\\';
#endif

    size_t begin = path.find_last_of(seperator);
    size_t pos = (begin != std::wstring::npos) ? begin + 1 : 0;

    size_t end = path.find_last_of(L'.');
    size_t len = (end != std::wstring::npos) ? end - pos : path.length() - pos;

    return path.substr(pos, len);
}
