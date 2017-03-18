#include <string>
#include <vector>
#include <clocale>
#include <locale>
#include <algorithm>
#include <iostream>
#include <sstream>
#include <stdlib.h>
#include <climits>
#include <cctype>

#include "StringUtility.h"

using namespace std;

void StringUtility::Split(vector<wstring>& tokens, const wstring& wstr)
{
    size_t start = 0;
    size_t end = 0;
    while ((end = wstr.find(L'|', start)) != string::npos) 
	{
        if (start < end)
        {
            wstring column = wstr.substr(start, end - start);
            tokens.push_back(Trim(column));
        }
        start = end + 1;
    }
}

vector<wstring> StringUtility::Split(const wstring &str, wchar_t delimiter, bool trim)
{
    wstringstream iss(str);
    wstring tmp;
    vector<wstring> res;
    while (getline(iss, tmp, delimiter))
    {
		if (trim)
	        res.push_back(Trim(tmp));
		else
	        res.push_back(tmp);
    }

    return res;
}

pair<wstring, wstring> StringUtility::NameValue(const wstring &str, wchar_t delimiter, bool trim)
{
	std::size_t pos = str.find(delimiter);
	if (pos != std::wstring::npos)
	{
		wstring name = str.substr(0, pos);
		wstring value = str.substr(pos + 1);
        if (trim)
        {
            value = Trim(value);
        }

        return std::make_pair(name, value);
	}
	else
	{
		return std::make_pair(L"", str);
	}
}

// take out all empty lines
string StringUtility::strValue4DocStr(const wstring& str)
{
	string work_str = StringUtility::wstring2string(str);
	string trimmed_str;
    size_t start = 0;
    size_t end = 0;
    while ((end = work_str.find('\n', start)) != string::npos) 
	{
        if (start < end)
        {
            string line = work_str.substr(start, end - start + 1);
			trimmed_str.append(line);
        }
        start = end + 1;
    }

	return trimmed_str;
}

int StringUtility::convertToBinary(const wstring& str, char* buffer, size_t buffer_size)
{
	if (NULL == buffer) return -1;

	// 長さは2以上の偶数
	const size_t len = str.length();
	if ((len < 2) || (len % 2 != 0)) return -1;

	const size_t octet_length = len / 2;
	if (buffer_size < octet_length) return -1;

	for (size_t i = 0; (i < octet_length) && (i < buffer_size); i++)
	{
		buffer[i] = char2octet(str[i*2]) * 0x10 + char2octet(str[i*2 + 1]);
	}

	return octet_length;
}

char StringUtility::char2octet(wchar_t ch)
{
	if ((ch >= L'0') && (ch <= L'9'))
	{
		return (char)(ch - L'0');
	}
	else if ((ch >= L'A') && (ch <= L'F'))
	{
		return (char)(10 + (ch - L'A'));
	}
	else if ((ch >= L'a') && (ch <= L'f'))
	{
		return (char)(10 + (ch - L'a'));
	}
	else
	{
		return 0xF;
	}
}

wstring StringUtility::Trim(const wstring& wstr)
{
    const wchar_t* WHITE_SPACES = L" \t";

    wstring result;
    wstring::size_type left = wstr.find_first_not_of(WHITE_SPACES);
    if (left != wstring::npos)
    {
        wstring::size_type right = wstr.find_last_not_of(WHITE_SPACES);
        result = wstr.substr(left, right - left + 1);
    }

    return FilterQuotations(result);
}

wstring StringUtility::FilterQuotations(const wstring& wstr)
{
	wstring ws = wstr;
    wstring::size_type left_quotaion = ws.find_first_of(L'"');
    wstring::size_type right_quotaion = ws.find_last_of(L'"');
    wstring::size_type length = ws.length();
    if ((length > 2) && (left_quotaion == 0) && (right_quotaion == length-1))
    {
        return ws.substr(left_quotaion + 1, right_quotaion - 1);
    }
    else
    {
        return ws;
    }
}

string StringUtility::wstring2string(const wstring& wstr)
{
	if (wstr.length() == 0) return std::string();

    setlocale(LC_CTYPE, "");

    size_t output_size = wstr.length() * MB_CUR_MAX + 1;
    char *output = new char[output_size];
    output[0] = '\0';
#ifdef WIN32
    // wcstombs_s is supported since C11
    size_t count;
    wcstombs_s(&count, output, output_size, wstr.c_str(), output_size);
#else
    std::wcstombs(output, wstr.c_str(), output_size);
#endif
    std::string result(output);
    delete[] output;

    return result;
}

wstring StringUtility::string2wstring(const string& str)
{
    wstring resultStr(str.begin(), str.end());

    return resultStr;
}

NString StringUtility::wstring2NString(const wstring& wstr)
{
	return NString(wstr.c_str());
}

bool StringUtility::IsNumber(const string& str)
{
    if (!IsValidStartOfNumber(str))
    {
        return false;
    }

    string::const_iterator it = str.begin();
    it++;   // skip the first character because it is valid beginning of a number
    while (it != str.end() && (IsDigit(*it) || (*it == '.'))) ++it;
    return !str.empty() && it == str.end();
}

bool StringUtility::IsDigit(char ch)
{
	return (ch >= '0') && (ch <= '9');
}

bool StringUtility::IsValidStartOfNumber(const string& str)
{
    if (str.length() == 0)
    {
        return false;
    }

    char ch = str[0];
    return (ch == '-') || (ch == '+') || IsDigit(ch);
}

int StringUtility::stoi(const wstring& wstr)
{
	return (int)stol(wstr);
}

int StringUtility::hextoi(const wstring& wstr)
{
    int out;
    std::wstringstream ss;
	ss << std::hex << wstr;
    ss >> out;

	return out;
}

long StringUtility::stol(const wstring& wstr)
{
    string str = wstring2string(wstr);
	RemoveAllComma(str);
    if (!IsNumber(str))
    {
        return LONG_MAX;
    }

    char* error = NULL;
    long value = strtol(str.c_str(), &error, 10);
    if (*error == '\0')
        return value;
    else
        return LONG_MAX;
}

double StringUtility::stod(const wstring& wstr)
{
    string str = wstring2string(wstr);
	RemoveAllComma(str);
	if (!IsNumber(str))
    {
        return HUGE_VAL;
    }

    char* error = NULL;
    double value = strtod(str.c_str(), &error);
    if (*error == '\0')
        return value;
    else
        return HUGE_VAL;
}

void StringUtility::ReplaceAll(std::wstring& text, std::wstring from, std::wstring to)
{
    size_t start_pos = 0;
    while ((start_pos = text.find(from, start_pos)) != std::wstring::npos) {
        text.replace(start_pos, from.length(), to);
        start_pos += to.length();
    }
}

void StringUtility::ReplaceFirst(std::wstring& text, std::wstring from, std::wstring to)
{
    size_t start_pos = 0;
    if ((start_pos = text.find(from, start_pos)) != std::wstring::npos) {
        text.replace(start_pos, from.length(), to);
    }
}

void StringUtility::RemoveAllChar(string& str, char ch)
{
	// std::string - erase : Erases the sequence of characters in the range [first,last).
	// std::remove : Transforms the range [first,last) into a range with all the elements
	//               that compare equal to val removed, and returns an iterator to the new end of that range.

	str.erase(std::remove(str.begin(), str.end(), ch), str.end());
}

wstring StringUtility::RemoveAllChar(const wstring& str, char ch)
{
	wstring workStr = str;
	workStr.erase(std::remove(workStr.begin(), workStr.end(), ch), workStr.end());

	return workStr;
}

string StringUtility::itos(int n)
{
	stringstream s;
	s << n;
	return s.str();
}

wstring StringUtility::itows(int n)
{
    wstringstream s;
    s << n;
    return s.str();
}

string StringUtility::int2hex(int value)
{
	std::stringstream stream;
	stream << std::hex << value;
	std::string result( stream.str() );

	return result;
}

wstring StringUtility::to_lower_case(const wstring& str)
{
	static std::locale s_locale("");

    wstring new_str;

    for (wstring::const_iterator iter = str.begin(); iter != str.end(); ++iter)
    {
        if ((*iter) < 0x80)
            new_str += std::tolower(*iter, s_locale);
        else
            new_str += *iter;
    }
    return new_str;
}
