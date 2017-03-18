#include "Tuple.h"
#include "StringUtility.h"

using namespace std;
using namespace bdd;

Tuple::Tuple(std::wstring tupleStr)
{

	if (tupleStr.length() == 0) return;

	Start(tupleStr);
}

void Tuple::Start(std::wstring& tupleStr)
{
	m_Stream << tupleStr;
	m_LastChar = L' ';
	ParseTuple();
}

void Tuple::ParseTuple()
{
	while (HasNextChar())
	{
		GherkinColumn elem = Elemement();
		m_ElemList.push_back(elem);
		if (m_LastChar == L',') NextChar();
	}
}

GherkinColumn Tuple::Elemement()
{
	SkipWhiteSpace();
	if (m_LastChar == L'[')
	{
		return SubTupleElement();
	}
	else
	{
		return TupleElement();
	}
}

GherkinColumn Tuple::SubTupleElement()
{
	wstring str;

	NextChar();
	int left_brackets_count = 1;
	while (HasNextChar() && (left_brackets_count != 0))
	{
		if (m_LastChar == L'[') left_brackets_count++;
		if (m_LastChar == L']') left_brackets_count--;

		if (m_LastChar == L']')
		{
			if (left_brackets_count != 0) str += m_LastChar;
		}
		else
		{
			str += m_LastChar;
		}

		NextChar();
	}
	if (m_LastChar == L']') NextChar();

	return GherkinColumn(StringUtility::Trim(str), true);
}

GherkinColumn Tuple::TupleElement()
{
	wstring str;

	int left_brackets_count = 0;
	bool is_end_of_element = false;
	while (HasNextChar() && !is_end_of_element)
	{
		str += m_LastChar;
		if (m_LastChar == L'[') left_brackets_count++;
		if (m_LastChar == L']') left_brackets_count--;

		NextChar();
		is_end_of_element = (m_LastChar == L',') && (left_brackets_count <= 0);
	}

	return GherkinColumn(StringUtility::Trim(str), false);
}

void Tuple::NextChar()
{
	m_LastChar = HasNextChar() ? m_Stream.get() : EOF_CH;
}

void Tuple::SkipWhiteSpace()
{
	while (IsWhiteSpace(m_LastChar) && HasNextChar())
	{
		NextChar();
	}
}

bool Tuple::IsWhiteSpace(wchar_t ch)
{
	return (ch == L' ') || (ch == L'\t');
}
