using DummyDB_5.Model;
using System.Text.Json;
using System.Data;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Documents;
using System.IO;

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
        private TableScheme scheme;
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

        public EditViewModel() 
        {
            DataTable = MainViewModel.selectedTable;
            TableName = MainViewModel.selectedTable.TableName;
            foreach (var pair in MainViewModel.schemeTablePairs)
            {
                if (pair.Key.Name == TableName) { scheme = pair.Key; break; }
            }
            ColumnNames = new List<string>();
            foreach(Column column in scheme.Columns)
            {
                ColumnNames.Add(column.Name);
            }
        }

        public ICommand Save => new CommandDelegate(parameter =>
        {
            if (_tableName != scheme.Name)
            {
                scheme.Name = _tableName;
            }
            if(SelectedColumn != null && ColumnName != "" && ColumnName != null)
            {
                foreach(Column column in scheme.Columns)
                {
                    if(column.Name == _selectedColumn) 
                    {
                        column.Name = _columnName;
                    }
                }
            }
            if(NewColumnType != null && NewColumnName != "" && NewColumnName != null) 
            {
                scheme.Columns.Add(new Column { Name = _newColumnName, Type = _newColumnType });
            }
            string json = JsonSerializer.Serialize(scheme);
            //File.WriteAllText($"{MainViewModel.folderPath}\\{scheme.Name}.json", json);
            DisplayTable();
            //MessageBox.Show(json);
        });

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
