#pragma once

#include <string>
#include <vector>
#include <map>

#include "StringUtility.h"

using namespace std;

namespace bdd
{
	class GherkinColumn
	{
	public:
		GherkinColumn() : m_Value(L"") {}
		GherkinColumn(wstring str, bool is_tuple = false) : m_Value(str), m_IsTuple(is_tuple) {}

		int intValue() { return StringUtility::stoi(m_Value); }
		int intAsHex() { return StringUtility::hextoi(m_Value); }
		long longValue() { return StringUtility::stol(m_Value); }
		double doubleValue() { return StringUtility::stod(m_Value); }
		wstring wstrValue() { return m_Value; }
		NString NStrValue() { return StringUtility::wstring2NString(m_Value); }
		BYTE byteValue() { return (BYTE)(StringUtility::stoi(m_Value) & 0xFF); }
		string strValue() { return StringUtility::wstring2string(m_Value); }
		bool boolValue() { return (m_Value == L"true") || (m_Value == L"TRUE"); }
		int getAsBinary(char* buffer, size_t buffer_size){ return StringUtility::convertToBinary(m_Value, buffer, buffer_size); }

		bool isTuple() { return m_IsTuple; }
		std::vector<GherkinColumn> tupleValue();
		std::pair<wstring, GherkinColumn> pairValue();
		std::map<wstring, GherkinColumn> mapValue();

		bool operator==(const GherkinColumn& column) const;
		bool operator!=(const GherkinColumn& column) const;

	private:
		wstring m_Value;
		bool    m_IsTuple;
	};

	class GherkinTable;

	class GherkinRow
	{
	public:
		GherkinRow() : m_pTable(NULL){}
		GherkinRow(wstring row_value) : m_pTable(NULL) { InitializeColumns(row_value); }
        GherkinRow(GherkinTable* pTable, wstring row_value) : m_pTable(pTable) { InitializeColumns(row_value); }

		int ColumnCount() const { return m_Columns.size(); }
		GherkinColumn& operator[](int index);
        GherkinColumn& operator[](wstring col_name);
		bool HasColumn(wstring col_name);

		std::vector<GherkinColumn>::iterator begin() { return m_Columns.begin(); }
		std::vector<GherkinColumn>::iterator end() { return m_Columns.end(); }

		bool operator==(const GherkinRow& row) const;
		bool operator!=(const GherkinRow& row) const;

		void SetTable(GherkinTable* pTable) { m_pTable = pTable; }

	protected:
		void InitializeColumns(wstring& row_value);

	private:
		vector<GherkinColumn> m_Columns;
        GherkinTable* m_pTable;
	};

	class GherkinTable
	{
	public:
        GherkinTable() {}
        GherkinTable(wstring table);
		GherkinTable(const GherkinTable& table);
		GherkinTable& operator=(const GherkinTable& table);

		int RowCount() const { return m_Rows.size(); }
        vector<wstring> ColumnNames() { return m_ColumNames; }
        int ColIndexFromName(wstring col_name);

		GherkinTable& AddRow(wstring row);
		GherkinRow& operator[](int index);
		vector<GherkinRow>& Rows() { return m_Rows; }

		std::vector<GherkinRow>::iterator begin() { return m_Rows.begin(); }
		std::vector<GherkinRow>::iterator end() { return m_Rows.end(); }


		bool operator==(const GherkinTable& table) const;

	private:
		void Copy(const GherkinTable& table);

	private:
		vector<GherkinRow> m_Rows;
        vector<wstring>    m_ColumNames;
	};
}

using namespace bdd;	// for compatibility with previous version
