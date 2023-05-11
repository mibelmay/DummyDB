using DummyDB_5.Model;
using System.Text.Json;
using System.Data;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Forms;

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
        private string oldFileName;
        private TableScheme scheme;
        private Table table;
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
        private string folderPath;

        public EditViewModel() 
        {
            DataTable = MainViewModel.selectedTable;
            TableName = MainViewModel.selectedTable.TableName;
            foreach (var pair in MainViewModel.schemeTablePairs)
            {
                if (pair.Key.Name == TableName) { scheme = pair.Key; table = pair.Value; break; }
            }
            ColumnNames = new List<string>();

            foreach (Column column in scheme.Columns)
            {
                ColumnNames.Add(column.Name);
            }
            oldFileName = TableName;
            folderPath = MainViewModel.folderPath;
        }

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
            ColumnNames.Remove(SelectedColumn);
            ColumnNames.Add(ColumnName);
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
                addValue = null; 
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
            return newFile;
        }
        public void DisplayTable()
        {
            DataTable.Clear();
            DataTable dataTable = new DataTable();
            foreach (var pair in MainViewModel.schemeTablePairs)
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

    }
}
