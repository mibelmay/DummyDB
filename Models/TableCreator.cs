using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyDB_5.Models
{
    public static class TableCreator
    {
        public static void DeleteColumnFromTable(Table table, Column column)
        {
            foreach (var row in table.Rows)
            {
                row.Data.Remove(column);
            }
        }

        public static void AddColumnToTable(Table table, Column newColumn)
        {
            object addValue = GetDefaultValue(newColumn);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i].Data[newColumn] = addValue;
            }
        }

        public static object GetDefaultValue(Column column)
        {
            if (column.Type == "string")
            {
                return "";
            }
            else if (column.Type == "datetime")
            {
                return DateTime.MinValue;
            }
            return 0;
        }

        public static void WriteData(Table table, int rowId, Column column, object data)
        {
            bool isPrimary = ReferenceChecker.CheckPrimaryKey(column);
            if (rowId >= table.Rows.Count)
            {
                AddRowToTable(table);
                if (isPrimary)
                {
                    table.Rows[rowId].Data[column] = (uint)table.Rows[rowId - 1].Data[column] + 1;
                    return;
                }
            }
            if (!isPrimary)
            {
                table.Rows[rowId].Data[column] = data;
            }
        }

        private static void AddRowToTable(Table table)
        {
            table.Rows.Add(
                new Row()
                {
                    Data = new Dictionary<Column, object>()
                });
        }

    }
}
