#pragma once

#include <string>
#include <sstream>
#include <vector>

#include "GherkinTable.h"

namespace bdd
{
	/////////////////////////////////
	/// Input: [a1, a2, ..., an], [b1, b2, ..., bm], ...,  [p1, p2, ..., pk]
	/// Output: vector<GherkinColumn>
	/// tuple -> element | tuple ',' element
	/// element -> string : '[' tuple ']'
	/////////////////////////////////
	class Tuple
	{
		const wchar_t EOF_CH = 0xFFFF;
	public:
		Tuple(std::wstring tupleStr);
		std::vector<GherkinColumn>& TupleValue() { return m_ElemList; }

	private:
		void Start(std::wstring& tupleStr);

		GherkinColumn SubTupleElement();
		GherkinColumn TupleElement();
		void NextChar();
		void SkipWhiteSpace();
		bool IsWhiteSpace(wchar_t ch);
		bool HasNextChar() { return !m_Stream.eof(); }

		void ParseTuple();
		GherkinColumn Elemement();

	private:
		std::vector<GherkinColumn> m_ElemList;
		std::wstringstream m_Stream;
		wchar_t m_LastChar;
	};
}