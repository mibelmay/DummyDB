using System;
using System.Collections.Generic;
using System.Linq;
using DummyDB.Core;
using System.Text;
using System.Threading.Tasks;

namespace DummyDB.ViewModel
{
    public class ReferenceChecker
    {
        public static bool CheckPrimaryKeys(Table table)
        {
            foreach (Column column in table.Scheme.Columns)
            {
                if (column.IsPrimary == true && column.ReferencedTable == null)
                {
                    if (column.Type != "uint")
                    {
                        return false;
                    }
                    if (!CheckIfDataUnique(table, column))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool CheckForeignKey(Column column)
        {
            if(column.ReferencedColumn != null)
            {
                return true;
            }
            return false;
        }

        public static bool CheckPrimaryKey(Column column)
        {
            if (column.IsPrimary && column.ReferencedColumn == null)
            {
                return true;
            }
            return false;
        }

        public static bool CheckReference(List<Table> tables, Column column, object data)
        {
            Table referencedTable = FindTable(tables, column.ReferencedTable);
            Column referencedColumn = FindColumn(referencedTable, column.ReferencedColumn);
            if (!CheckIfDataExist(referencedTable, referencedColumn, data))
            {
                return false;
            }
            return true;
        }

        public static bool IsRemovalValid(List<Table> tables, Table table, Row row)
        {
            for(int i = 0; i < table.Scheme.Columns.Count; i++)
            {
                if (!CheckPrimaryKey(table.Scheme.Columns[i]))
                {
                    continue;
                }
                if (!IsReferenceExist(tables, table, table.Scheme.Columns[i], row))
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public static bool IsReferenceExist(List<Table> tables, Table primaryTable, Column primaryColumn, Row deletedRow)
        {
            foreach(Table table in tables)
            {
                foreach(Column column in table.Scheme.Columns)
                {
                    if(column.ReferencedTable != primaryTable.Scheme.Name || column.ReferencedColumn != primaryColumn.Name)
                    {
                        continue;
                    }
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if ((uint)table.Rows[i].Data[column] == (uint)deletedRow.Data[primaryColumn])
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        public static Table FindTable(List<Table> tables, string tableName)
        {
            foreach (Table table in tables)
            {
                if (table.Scheme.Name == tableName)
                {
                    return table;
                }
            }
            return null;
        }

        public static Column FindColumn(Table table, string columnName)
        {
            foreach (Column column in table.Scheme.Columns)
            {
                if (column.Name == columnName)
                {
                    return column;
                }
            }
            return null;
        }
        public static bool CheckIfDataUnique(Table table, Column column)
        {
            List<string> data = new List<string>();
            foreach (Row row in table.Rows)
            {
                if (!data.Contains(row.Data[column].ToString()))
                {
                    data.Add(row.Data[column].ToString());
                    continue;
                }
                return false;
            }
            return true;
        }

        public static bool CheckIfDataExist(Table table, Column column, object data)
        {
            foreach(Row row in table.Rows)
            {
                if (row.Data[column].ToString() == data.ToString())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
