using DummyDB.Core;
using System.Text.Json;
using System.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text;
using System.Windows.Data;

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
            if (SelectedColumn == null || ColumnName == "" || ColumnName == null)
            {
                return;
            }
            if (IfColumnExist(ColumnName))
            {
                ShowMessage($"Колонка с названием {ColumnName} уже существует");
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
            if (DeletedColumn == null || DeletedColumn == "")
            {
                return;
            }
            if(!Validation("Вы уверены, что хотите удалить столбец?"))
            {
                return;
            }
            foreach (Column column in scheme.Columns)
            {
                if (DeletedColumn == column.Name)
                {
                    ColumnNames.Remove(column.Name);
                    scheme.Columns.Remove(column);
                    foreach (var row in table.Rows)
                    {
                        row.Data.Remove(column);
                    }
                    break;
                }
            }
            UpdateColumnNames();
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
            if (NewColumnType == null || NewColumnName == "" || NewColumnName == null)
            {
                return;
            }
            if (IfColumnExist(NewColumnName))
            {
                ShowMessage($"Колонка с названием {NewColumnName} уже существует");
                return;
            }
            Column newColumn = new Column 
            { 
                Name = NewColumnName, 
                Type = NewColumnType
            };
            scheme.Columns.Add(newColumn);
            ColumnNames.Add(NewColumnName);
            object addValue = GetDefaultValue(newColumn);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i].Data[newColumn] = addValue;
            }
            UpdateColumnNames();
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

        public ICommand AddRow => new CommandDelegate(param =>
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
            DataRow selectedRow = DataTable.Rows[index];
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
            for (int i = 0; i < DataTable.Rows.Count; i++)
            {
                for (int j = 0; j < scheme.Columns.Count; j++)
                {
                    object data = CheckData(DataTable.Rows[i], scheme.Columns[j]);
                    if (data == null) 
                    { 
                        return false;
                    }
                    if (i >= table.Rows.Count)
                    {
                        table.Rows.Add(
                            new Row() 
                            { 
                                Data = new Dictionary<Column, object>() 
                            });
                    }
                    table.Rows[i].Data[scheme.Columns[j]] = data;
                }
            }
            return true;
        }

        public object CheckData(DataRow row, Column column)
        {
            string value = row[column.Name].ToString();
            switch (column.Type)
            {
                case ("int"):
                    {
                        return IntCase(value, row, column);
                    }
                case ("uint"):
                    {
                        return UintCase(value, row, column);
                    }
                case ("datetime"):
                    {
                        return DateTimeCase(value, row, column);
                    }
                case ("double"):
                    {
                        return DoubleCase(value, row, column);
                    }
                default:
                    break;
            }
            return value;
        }

        public object IntCase(string value, DataRow row, Column column)
        {
            if (int.TryParse(value, out int data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1} в столбце {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object UintCase(string value, DataRow row, Column column)
        {
            if (uint.TryParse(value, out uint data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1} в столбце {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object DateTimeCase(string value, DataRow row, Column column)
        {
            if (DateTime.TryParse(value, out DateTime data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1}  в столбце  {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object DoubleCase(string value, DataRow row, Column column)
        {
            if (double.TryParse(value, out double data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1}  в столбце  {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}