using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DummyDB_5.Models;

namespace DummyDB.ViewModel
{
    public class EditViewModel : ViewModel
    {
        private DataTable _dataTable;
        public DataTable DataTable
        {
            get { return _dataTable; }
            set { _dataTable = value; OnPropertyChanged(); }
        }
        private string _tableName;
        public string TableName
        {
            get { return _tableName; }
            set 
            { 
                if (value == _tableName) return; 
                _tableName = value; 
                OnPropertyChanged();
            }
        }
        public string oldFileName { get; set; }
        public TableScheme scheme;
        public Table table;
        private List<string> _columnNames;
        public List<string> ColumnNames
        {
            get { return _columnNames; }
            set
            { _columnNames = value; OnPropertyChanged(); }
        }
        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; OnPropertyChanged(); }
        }
        private string _selectedColumn;
        public string SelectedColumn
        {
            get { return _selectedColumn; }
            set { _selectedColumn = value; OnPropertyChanged(); }
        }
        private string _newColumnName;
        public string NewColumnName
        {
            get { return _newColumnName; }
            set { _newColumnName = value; OnPropertyChanged(); }
        }
        private string _newColumnType;
        public string NewColumnType
        {
            get { return _newColumnType; }
            set { _newColumnType = value; OnPropertyChanged(); }
        }
        private string _deleteColumn;
        public string DeletedColumn
        {
            get { return _deleteColumn; }
            set
            {
                _deleteColumn = value; OnPropertyChanged();
            }
        }
        private string _referencedTable;
        public string ReferencedTable
        {
            get { return _referencedTable; }
            set { _referencedTable = value; OnPropertyChanged(); }
        }
        private string _referencedColumn;
        public string ReferencedColumn
        {
            get { return _referencedColumn; }
            set { _referencedColumn = value; OnPropertyChanged(); }
        }
        private bool _isPrimaryKey;
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; OnPropertyChanged(); }
        }
        public List<Table> Tables { get; set; }
        public List<string> TableNames { get; set; }
        public string folderPath { get; set; }
        public DataGrid dataGrid { get; set; }

        public ICommand SaveScheme => new CommandDelegate(parameter =>
        {
            RenameTable();
            RenameColumn();
            AddNewColumn();
            DeleteColumn();

            string json = JsonSerializer.Serialize(scheme);
            File.WriteAllText($"{folderPath}\\{scheme.Name}.json", json);
            SaveChangesToCsv();
            ColumnName = NewColumnName = "";
            SelectedColumn = NewColumnType = DeletedColumn = null;
            DisplayTable();
        });

        public bool IfColumnExist(string columnName)
        {
            foreach(Column column in scheme.Columns)
            {
                if (column.Name == columnName)
                {
                    ShowMessage($"Столбец с названием {ColumnName} уже существует");
                    return true;
                }
            }
            return false;
        }

        public void RenameTable()
        {
            if (TableName == scheme.Name)
            {
                return;
            }
            scheme.Name = TableName;
            File.Delete(folderPath + $"\\{oldFileName}.json");
            File.Delete(folderPath + $"\\{oldFileName}.csv");
        }

        public void RenameColumn()
        {
            if (SelectedColumn == null || ColumnName == "" || ColumnName == null || IfColumnExist(ColumnName))
            {
                return;
            }
            if (SelectedColumn == "id")
            {
                ShowMessage($"Нельзя переименовать столбец id");
                return;
            }
            foreach (Column column in scheme.Columns)
            {
                if (column.Name == SelectedColumn)
                {
                    column.Name = ColumnName;
                }
            }
            UpdateColumnNames();
        }

        public void DeleteColumn()
        {
            if(!IsRemovalValid())
            {
                return;
            }
            foreach (Column column in scheme.Columns)
            {
                if (column.Name == DeletedColumn)
                {
                    scheme.Columns.Remove(column);
                    DeleteColumnFromTable(column);
                    break;
                }
            }
            UpdateColumnNames();
        }

        private void DeleteColumnFromTable(Column column)
        {
            foreach (var row in table.Rows)
            {
                row.Data.Remove(column);
            }
        }

        private bool IsRemovalValid()
        {
            if (DeletedColumn == null || DeletedColumn == "")
            {
                return false;
            }
            if (DeletedColumn == "id")
            {
                ShowMessage("Нельзя удалить столбец id");
                return false;
            }
            if (!Validation("Вы уверены, что хотите удалить столбец?"))
            {
                return false;
            }
            return true;
        }

        public bool Validation(string message)
        {
            DialogResult dialogResult = MessageBox.Show(message, "Подтверждение", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return false;
            }
            return true;
        }

        public void AddNewColumn()
        {
            if (NewColumnType == null || NewColumnName == null || IfColumnExist(NewColumnName) || !CheckForeignKey())
            {
                return;
            }
            Column newColumn = new Column 
            { 
                Name = NewColumnName,
                Type = NewColumnType,
                IsPrimary = IsPrimaryKey,
                ReferencedTable = ReferencedTable,
                ReferencedColumn= ReferencedColumn
            };
            scheme.Columns.Add(newColumn);
            AddColumnToTable(newColumn);
            UpdateColumnNames();
        }

        private void AddColumnToTable(Column newColumn)
        {
            object addValue = GetDefaultValue(newColumn);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i].Data[newColumn] = addValue;
            }
        }

        public object GetDefaultValue(Column column)
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

        public void DisplayTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = TableName;

            AddColumnsToDataTable(dataTable);
            AddRowsToDataTable(dataTable);

            DataTable = dataTable;
        }

        public void AddColumnsToDataTable(DataTable dataTable)
        {
            foreach (Column column in scheme.Columns)
            {
                dataTable.Columns.Add(column.Name);
            }
        }

        public void AddRowsToDataTable(DataTable dataTable)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow newRow = dataTable.NewRow();
                foreach (var rowPair in table.Rows[i].Data)
                {
                    newRow[rowPair.Key.Name] = rowPair.Value;
                }
                dataTable.Rows.Add(newRow);
            }
        }

        public void UpdateColumnNames()
        {
            List<string> newColumnNames = new List<string>();
            foreach (Column column in scheme.Columns)
            {
                newColumnNames.Add(column.Name);
            }
            ColumnNames = newColumnNames;
        }

        public ICommand AddRowToDataTable => new CommandDelegate(param =>
        {
            DataTable.Rows.Add(DataTable.NewRow());
        });

        public ICommand LoadDataTable => new CommandDelegate(param =>
        {
            if(!LoadChanges())
            {
                return;
            }
            SaveChangesToCsv();
            DisplayTable();
        });

        public ICommand DeleteRow => new CommandDelegate(param =>
        {
            int index = dataGrid.SelectedIndex;
            if (index == -1 || index >= table.Rows.Count)
            {
                return;
            }
            if(!ReferenceChecker.IsRemovalValid(Tables, table, table.Rows[index]))
            {
                ShowMessage($"Вы не можете удалить эту строку, т.к. на нее ссылаются данные из других таблиц");
                return;
            }
            if(!Validation("Вы хорошо подумали?"))
            {
                return;
            }
            DataTable.Rows[index].Delete();
            table.Rows.Remove(table.Rows[index]);
            LoadChanges();
        });

        public void SaveChangesToCsv()
        {
            StringBuilder newFile = new StringBuilder();
            foreach(Row row in table.Rows)
            {
                string newRow = "";
                foreach (Column column in scheme.Columns)
                {
                    newRow = newRow + $"{row.Data[column]};" ;
                }
                newRow = newRow.Substring(0, newRow.Length - 1);
                newFile.AppendLine(newRow);
            }
            File.WriteAllText(folderPath + $"\\{TableName}.csv", newFile.ToString());
        }

        public bool LoadChanges()
        {
            DataChecker dataChecker = new DataChecker(DataTable);
            for (int i = 0; i < DataTable.Rows.Count; i++)
            {
                for (int j = 0; j < scheme.Columns.Count; j++)
                {
                    object data = dataChecker.CheckData(DataTable.Rows[i], scheme.Columns[j]);
                    if (data == null) 
                    { 
                        return false;
                    }
                    if(!CheckColumnReferences(scheme.Columns[j], data))
                    {
                        return false;
                    }
                    WriteData(i, scheme.Columns[j], data);
                }
            }
            return true;
        }

        private void WriteData(int rowId, Column column, object data)
        {
            bool isPrimary = ReferenceChecker.CheckPrimaryKey(column);
            if (rowId >= table.Rows.Count)
            {
                AddRowToTable();
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

        private void AddRowToTable()
        {
            table.Rows.Add(
                new Row()
                {
                    Data = new Dictionary<Column, object>()
                });
        }

        private bool CheckColumnReferences(Column column, object data)
        {
            if (!ReferenceChecker.CheckForeignKey(column))
            {
                return true;
            }
            if (!ReferenceChecker.CheckReference(Tables, column, data))
            {
                ShowMessage($"Не обнаружена строка с {column.ReferencedColumn} {data} в таблице {column.ReferencedTable}");
                return false;
            }
            return true;
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public bool CheckForeignKey()
        {
            if (!IsPrimaryKey || ReferencedTable == null || ReferencedColumn == null)
            {
                ReferencedTable = null;
                ReferencedColumn = null;
                IsPrimaryKey = false;
                return true;
            }
            if (NewColumnType != "uint")
            {
                ShowMessage("Столбец с Foreign key должен быть типа uint");
                return false;
            }
            return true;
        }
    }
}