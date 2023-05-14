using DummyDB_5.Model;
using System.Text.Json;
using System.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text;

namespace DummyDB_5.ViewModel
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
        public string oldFileName;
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
        public string DeleteColumn
        {
            get { return _deleteColumn; }
            set
            {
                _deleteColumn = value; OnPropertyChanged();
            }
        }
        public string folderPath;
        public DataGrid dataGrid { get; set; }
        public Dictionary<TableScheme, Table> schemeTablePairs { get; set; }


        public ICommand Save => new CommandDelegate(parameter =>
        {
            string[] csv = File.ReadAllLines($"{folderPath}\\{oldFileName}.csv");
            File.Delete($"{folderPath}\\{oldFileName}.csv");
            if (TableName != scheme.Name)
            {
                scheme.Name = TableName;
                File.Delete(folderPath + $"\\{oldFileName}.json");
            }
            if (SelectedColumn != null && ColumnName != "" && ColumnName != null)
            {
                RenameColumn();
            }
            if (NewColumnType != null && NewColumnName != "" && NewColumnName != null)
            {
                csv = AddNewColumnToTable(csv, NewColumnType);
            }
            if (DeleteColumn != null && DeleteColumn != "")
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Вы уверены что хотите удалить столбец?", "Some Title", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    csv = DeleteColumnFromTable(csv);
                }
            }
            string json = JsonSerializer.Serialize(scheme);
            File.WriteAllText($"{folderPath}\\{scheme.Name}.json", json);
            File.WriteAllText($"{folderPath}\\{scheme.Name}.csv", String.Join("\n", csv));
            ColumnName = DeleteColumn = NewColumnName = "";
            SelectedColumn = NewColumnType = null;
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

        public void RenameColumn()
        {
            if (IfColumnExist(ColumnName))
            {
                System.Windows.MessageBox.Show($"Колонка с названием {ColumnName} уже существует");
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

        public string[] DeleteColumnFromTable(string[] csv)
        {
            if (!IfColumnExist(DeleteColumn))
            {
                System.Windows.MessageBox.Show($"Колонки с названием {DeleteColumn} нет в таблице");
                return csv;
            }
            int count = 0;
            foreach (Column column in scheme.Columns)
            {
                if (DeleteColumn == column.Name)
                {
                    ColumnNames.Remove(column.Name);
                    scheme.Columns.Remove(column);
                    foreach (var row in table.Rows)
                    {
                        row.Data.Remove(column);
                    }
                    break;
                }
                count += 1;
            }
            string[] newFile = new string[csv.Length];
            for (int i = 0; i < csv.Length; i++)
            {
                List<string> columns = (csv[i].Split(";")).ToList();
                columns.RemoveAt(count);
                string newLine = String.Join(";", columns);
                if (newLine[newLine.Length - 1] == ';')
                {
                    newLine.Remove(newLine.Length - 1);
                }
                newFile[i] = String.Join(";", columns);
            }
            UpdateColumnNames();
            return newFile;
        }

        public string[] AddNewColumnToTable(string[] csv, string type)
        {
            if (IfColumnExist(NewColumnName))
            {
                System.Windows.MessageBox.Show($"Колонка с названием {NewColumnName} уже существует");
                return csv;
            }
            Column newColumn = new Column { Name = NewColumnName, Type = NewColumnType };
            scheme.Columns.Add(newColumn);
            ColumnNames.Add(NewColumnName);
            string[] newFile = new string[csv.Length];
            string addValue = "";
            if (type == "string") 
            { 
                addValue = ""; 
            }
            else if (type == "datetime") 
            { 
                addValue = $"{DateTime.MinValue}"; 
            }
            else 
            { 
                addValue = "0";
            }
            for (int i = 0; i < csv.Length; i++)
            {
                newFile[i] = csv[i] += $";{addValue}";
                table.Rows[i].Data[newColumn] = addValue;
            }
            UpdateColumnNames();
            return newFile;
        }
        public void DisplayTable()
        {
            DataTable.Clear();
            DataTable dataTable = new DataTable();
            foreach (var pair in schemeTablePairs)
            {
                if (pair.Key.Name == _tableName)
                {
                    dataTable.TableName = _tableName;
                    foreach (Column column in pair.Key.Columns)
                    {
                        dataTable.Columns.Add(column.Name);
                    }
                    for (int i = 0; i < pair.Value.Rows.Count; i++)
                    {
                        DataRow newRow = dataTable.NewRow();
                        foreach (var rowPair in pair.Value.Rows[i].Data)
                        {
                            newRow[rowPair.Key.Name] = rowPair.Value;
                        }
                        dataTable.Rows.Add(newRow);
                    }
                    break;
                }
            }
            DataTable = dataTable;
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
            SaveChangesToFile();
            DisplayTable();
        });

        public ICommand DeleteRow => new CommandDelegate(param =>
        {
            int index = dataGrid.SelectedIndex;
            if (index == -1)
            {
                MessageBox.Show("Вы не выбрали строку для удаления");
                return;
            }
            DataRow selectedRow = DataTable.Rows[index];
            string row = "| ";
            for(int i = 0; i < scheme.Columns.Count; i++)
            {
                row = row + $"{selectedRow[scheme.Columns[i].Name]} | ";
            }
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show($"Вы уверены что хотите удалить строку\n{row}?", "Подтверждение", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return;
            }
            DataTable.Rows[index].Delete();
            table.Rows.Remove(table.Rows[index]);
            LoadChanges();
        });

        public void SaveChangesToFile()
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
            int countOfRows = DataTable.Rows.Count;
            if (table.Rows.Count < DataTable.Rows.Count)
            {
                countOfRows = table.Rows.Count;
            }
            for (int i = 0; i < countOfRows; i++)
            {
                for (int j = 0; j < scheme.Columns.Count; j++)
                {
                    if (DataTable.Rows[i][scheme.Columns[j].Name].ToString() == table.Rows[i].Data[scheme.Columns[j]].ToString())
                    {
                        continue;
                    }
                    object data = CheckData(i, j);
                    if (data == null) { continue; }
                    table.Rows[i].Data[scheme.Columns[j]] = CheckData(i, j);
                }
            }
            if (countOfRows < DataTable.Rows.Count)
            {
                for (int i = table.Rows.Count; i < DataTable.Rows.Count; i++)
                {
                    Row newRow = new Row();
                    for (int j = 0; j < scheme.Columns.Count; j++)
                    {
                        object data = CheckData(i, j);
                        if (data == null) { MessageBox.Show($"Невозможно добавить строку {i + 1}, т.к. в ней некорректные данные"); return false; }
                        newRow.Data.Add(scheme.Columns[j], data);
                    }
                    table.Rows.Add(newRow);
                }
            }
            return true;
        }

        public object CheckData(int i, int j)
        {
            string value = DataTable.Rows[i][scheme.Columns[j].Name].ToString();
            switch (scheme.Columns[j].Type)
            {
                case ("int"):
                    {
                        if (int.TryParse(value, out int data))
                        {
                            return data;
                        }
                        else
                        {
                            MessageBox.Show($"В сроке {i + 1} в столбце {scheme.Columns[j].Name} указаны некорректные данные");
                            return null;
                        }
                    }
                case ("uint"):
                    {
                        if (uint.TryParse(value, out uint data))
                        {
                            return data;
                        }
                        else
                        {
                            MessageBox.Show($"В сроке {i + 1} в столбце {scheme.Columns[j].Name} указаны некорректные данные");
                            return null;
                        }
                    }
                case ("datetime"):
                    {
                        if (DateTime.TryParse(value, out DateTime data))
                        {
                            return data;
                        }
                        else
                        {
                            MessageBox.Show($"В сроке {i + 1} в столбце {scheme.Columns[j].Name} указаны некорректные данные");
                            return null;
                        }
                    }
                case ("double"):
                    {
                        if (double.TryParse(value, out double data))
                        {
                            return data;
                        }
                        else
                        {
                            MessageBox.Show($"В сроке {i + 1} в столбце {scheme.Columns[j].Name} указаны некорректные данные");
                            return null;
                        }
                    }
                default:
                    break;
            }
            return value;
        }
    }
}
