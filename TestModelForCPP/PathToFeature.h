#pragma once

#include <string>

namespace bdd
{
    class PathToFeature
    {
    public:
        static std::wstring ToFeature(const std::wstring file_path);

    private:
        static std::wstring GetFileNameFromFilePath(const std::wstring& path);
    };
}

