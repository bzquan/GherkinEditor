#include "Tuple.h"
#include "GherkinTable.h"

bool GherkinColumn::operator==(const GherkinColumn& column) const
{
	return m_Value == column.m_Value;
}

bool GherkinColumn::operator!=(const GherkinColumn& column) const
{
	bool result = *this == column;
	return !result;
}

/////////////////////////////////////////
/// tuple -> element | tuple ',' element
/// element -> string : '[' tuple ']'
/////////////////////////////////////////
std::vector<GherkinColumn> GherkinColumn::tupleValue()
{
	Tuple tupleList(m_Value);
	std::vector<GherkinColumn> tuple_value = tupleList.TupleValue();
	if (tuple_value.size() == 1)
		return tuple_value[0].tupleValue();
	else
		return tuple_value;
}

std::pair<wstring, GherkinColumn> GherkinColumn::pairValue()
{
	std::pair<wstring, wstring> name_value = StringUtility::NameValue(m_Value, L':', true);
	return std::make_pair(name_value.first, GherkinColumn(name_value.second));
}

std::map<wstring, GherkinColumn> GherkinColumn::mapValue()
{
	std::map<wstring, GherkinColumn> dictionary;

	std::vector<GherkinColumn> items = tupleValue();
	for (vector<GherkinColumn>::iterator it = items.begin(); it != items.end(); it++)
	{
		std::pair<wstring, GherkinColumn> item = (*it).pairValue();
		if (item.first.size() > 0)
		{
			dictionary[item.first] = item.second;
		}
	}

	return dictionary;
}

//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

void GherkinRow::InitializeColumns(wstring& row_value)
{
	vector<wstring> tokens;
	StringUtility::Split(tokens, row_value);
	for (vector<wstring>::iterator it = tokens.begin(); it != tokens.end(); it++)
	{
		m_Columns.push_back(GherkinColumn(*it));
	}
}

bool GherkinRow::operator==(const GherkinRow& row) const
{
	if (this->ColumnCount() != row.ColumnCount())
	{
		return false;
	}

	for (int i = 0; i < this->ColumnCount(); i++ )
	{
		GherkinRow& other_row = const_cast<GherkinRow&>(row);
		if ( m_Columns[i] != other_row[i] )
		{
			return false;
		}
	}

	return true;
}

bool GherkinRow::operator!=(const GherkinRow& row) const
{
	bool result = *this == row;
	return !result;
}

GherkinColumn& GherkinRow::operator[](int index)
{
    if ( (index >= 0) && (index < ColumnCount()) )
    {
        return m_Columns[index];
    }
    else
    {
        std::string msg("Out of range in GherkinRow: ");
		msg.append(StringUtility::itos(index));
        throw std::runtime_error(msg.c_str());
    }
}

GherkinColumn& GherkinRow::operator[](wstring col_name)
{
    if (m_pTable == NULL) throw std::runtime_error("No table set to GherkinRow.");

    int index = m_pTable->ColIndexFromName(col_name);
    return (*this)[index];
}

bool GherkinRow::HasColumn(wstring col_name)
{
    if (m_pTable == NULL) return false;

    int index = m_pTable->ColIndexFromName(col_name);
	return (index >= 0);
}

GherkinTable::GherkinTable(const wstring table)
{
    vector<wstring> rows = StringUtility::Split(table, L'\n');
    if (rows.size() == 0) return;

    StringUtility::Split(m_ColumNames, rows[0]);

    for (size_t i = 1; i < rows.size(); i++)
    {
        AddRow(rows[i]);
    }
}

GherkinTable::GherkinTable(const GherkinTable& table)
{
	Copy(table);
}

GherkinTable& GherkinTable::operator=(const GherkinTable& table)
{
	if( this != &table )
	{
		Copy(table);
	}

	return *this;
}

void GherkinTable::Copy(const GherkinTable& table)
{
	m_ColumNames = table.m_ColumNames;
	m_Rows       = table.m_Rows;

	for (size_t i = 0; i < m_Rows.size(); i++)
	{
		m_Rows[i].SetTable(this);
	}
}

GherkinTable& GherkinTable::AddRow(wstring row)
{
	m_Rows.push_back(GherkinRow(this, row));
	return *this;
}

GherkinRow& GherkinTable::operator[](int index)
{
	if (index < RowCount())
	{
		return m_Rows[index];
	}
	else
	{
        std::string msg("Out of range in GherkinTable: ");
        msg.append(StringUtility::itos(index));
        throw std::runtime_error(msg.c_str());
	}
}

bool GherkinTable::operator==(const GherkinTable& table) const
{
	if (this->RowCount() != table.RowCount())
	{
		return false;
	}

	for (int i = 0; i < this->RowCount(); i++ )
	{
		GherkinTable& other_table = const_cast<GherkinTable&>(table);
		if ( m_Rows[i] != other_table[i] )
		{
			return false;
		}
	}

	return true;
}

int GherkinTable::ColIndexFromName(wstring col_name)
{
	for (size_t i = 0; i < m_ColumNames.size(); i++)
    {
		wstring& name = m_ColumNames[i];
        if (name == col_name)
		{
			return i;
		}
    }

    return -1;
}
