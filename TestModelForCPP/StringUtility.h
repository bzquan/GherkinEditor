#pragma once

#include <string>
#include <utility>
#include <map>
#include <list>
#include <vector>
#include <math.h>

//#include "NString.h"

using namespace std;

#ifndef NString
#define NString wstring
#endif

#ifndef BYTE
#define BYTE int
#endif

class StringUtility
{
public:
    static void Split(vector<wstring>& tokens, const wstring& wstr);
    static vector<wstring> Split(const wstring &str, wchar_t delimiter, bool trim = false);
	static pair<wstring, wstring> NameValue(const wstring &str, wchar_t delimiter = L':', bool trim = false);

    static wstring Trim(const wstring& str);
    static string strValue4DocStr(const wstring& str);
    static int convertToBinary(const wstring& str, char* buffer, size_t buffer_size);

	static string wstring2string(const wstring& wstr);
	static wstring string2wstring(const string& str);
	static NString wstring2NString(const wstring& wstr);

	static bool IsDigit(char ch);
	static bool IsNumber(const string& str);
    static int stoi(const wstring& wstr);
    static int hextoi(const wstring& wstr);
    static long stol(const wstring& wstr);
    static double stod(const wstring& wstr);

    static void ReplaceAll(std::wstring& text, std::wstring from, std::wstring to);
    static void ReplaceFirst(std::wstring& text, std::wstring from, std::wstring to);
    static wstring RemoveAllChar(const wstring& str, char ch);
	static string itos(int n);
    static wstring itows(int n);
    static string int2hex(int value);
    static wstring to_lower_case(const wstring& str);

private:
    static wstring FilterQuotations(const wstring& wstr);
	static void RemoveAllChar(string& str, char ch);
	static void RemoveAllComma(string& str){ RemoveAllChar(str, ','); }
    static bool IsValidStartOfNumber(const string& str);
	static char char2octet(wchar_t ch);
};

