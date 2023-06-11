using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyDB_5.Models
{
    public class ReferenceChecker
    {
        public static bool CheckForeignKey(Column column)
        {
            if (column.ReferencedTable != null)
            {
                return true;
            }
            return false;
        }

        public static bool CheckPrimaryKey(Column column)
        {
            if (column.IsPrimary && column.ReferencedTable == null)
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
            for (int i = 0; i < table.Scheme.Columns.Count; i++)
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
            foreach (Table table in tables)
            {
                foreach (Column column in table.Scheme.Columns)
                {
                    if (column.ReferencedTable != primaryTable.Scheme.Name)
                    {
                        continue;
                    }
                    if (FindReference(table, column, primaryColumn, deletedRow))
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool FindReference(Table table, Column column, Column primaryColumn, Row deletedRow)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if ((uint)table.Rows[i].Data[column] == (uint)deletedRow.Data[primaryColumn])
                {
                    return true;
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

        public static bool CheckIfDataExist(Table table, Column column, object data)
        {
            foreach (Row row in table.Rows)
            {
                if (row.Data[column].ToString() == data.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Column> CreatePrimaryColumn()
        {
            List<Column> columns = new List<Column>();
            columns.Add(
                new Column
                {
                    Name = "id",
                    Type = "uint",
                    IsPrimary = true,
                    ReferencedTable = null,
                    ReferencedColumn = null
                });
            return columns;
        }

        public static List<string> LoadPrimaryColumnsNames(List<Table> tables, string tableName)
        {
            List<string> names = new List<string>();
            foreach (Table table in tables)
            {
                if (table.Scheme.Name == tableName)
                {
                    names.Add(table.Scheme.Columns[0].Name);
                    break;
                }
            }
            return names;
        }
    }
}
