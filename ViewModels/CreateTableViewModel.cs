using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Linq;

namespace DummyDB.ViewModel
{
    public class CreateTableViewModel : ViewModel
    {
        private string _tableName;
        public string TableName
        {
            get { return _tableName; }
            set 
            { 
                _tableName = value; 
                OnPropertyChanged();
            }
        }
        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set
            {
                if (value == _columnName) return;
                _columnName = value;
                OnPropertyChanged();
            }
        }
        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        private List<Column> _columns;
        public List<Column> Columns
        {
            get 
            { 
                return _columns; 
            }
            set
            {
                _columns = value; 
                OnPropertyChanged();
            }
        }
        public string FolderPath { get; set; }
        public CreateTableWindow Window { get; set; }
        private bool _isPrimaryKey;
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; OnPropertyChanged(); }
        }
        public List<Table> Tables { get; set; }
        private string _referencedTable;
        public string ReferencedTable
        {
            get { return _referencedTable; }
            set 
            { 
                _referencedTable = value; 
                LoadColumnNames(_referencedTable); 
                OnPropertyChanged();
            }
        }
        private List<string> _tableNames;
        public List<string> TableNames
        {
            get { return _tableNames; }
            set { _tableNames = value; OnPropertyChanged(); }
        }
        private List<string> _columnNames;
        public List<string> ColumnNames
        {
            get { return _columnNames; }
            set { _columnNames = value; OnPropertyChanged(); }
        }
        private string _referencedColumn;
        public string ReferencedColumn
        {
            get { return _referencedColumn; }
            set { _referencedColumn = value; OnPropertyChanged(); }
        }


        public ICommand AddColumn => new CommandDelegate(patameter =>
        {
            if (ColumnName == "" || Type == null || IfColumnExist(ColumnName) || !CheckForeignKey())
            {
                return;
            }
            _columns.Add(
                new Column
                {
                    Name = $"{ColumnName}",
                    Type = $"{Type}",
                    IsPrimary = IsPrimaryKey,
                    ReferencedTable = ReferencedTable,
                    ReferencedColumn = ReferencedColumn
                });
            UpdateColumns();
        });


        public ICommand CreateTable => new CommandDelegate(patameter =>
        {
            if (TableName == "" || TableName == null || _columns.Count == 1)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            CreateEmptyTable(CreateJson());
            MessageBox.Show($"Таблица создана по пути {FolderPath}");
            Window.Close();
        });

        public bool CheckForeignKey()
        {
            if(!IsPrimaryKey || ReferencedTable == null || ReferencedColumn == null)
            {
                ReferencedTable = null;
                ReferencedColumn = null;
                IsPrimaryKey = false;
                return true;
            }
            if(Type == "uint")
            {
                return true;
            }
            MessageBox.Show($"Столбец с Foreign key должен быть типа uint");
            return false;
        }

        public bool IfColumnExist(string columnName)
        {
            foreach (Column column in Columns)
            {
                if (column.Name == columnName)
                {
                    MessageBox.Show($"Столбец с именем {ColumnName} уже добавлен в таблицу");
                    return true;
                }
            }
            return false;
        }

        public TableScheme CreateJson()
        {
            TableScheme scheme = new TableScheme()
            {
                Name = TableName,
                Columns = _columns
            };
            string json = JsonSerializer.Serialize<TableScheme>(scheme);
            File.WriteAllText($"{FolderPath}\\{TableName}.json", json);
            return scheme;
        }

        public void CreateEmptyTable(TableScheme scheme)
        {
            string newFile = "";
            foreach(Column column in scheme.Columns)
            {
                string addValue = GetDefaultValue(column).ToString();
                newFile = newFile + $"{addValue};";
            }
            newFile = newFile.Substring(0, newFile.Length - 1);
            File.WriteAllText($"{FolderPath}\\{TableName}.csv", newFile);
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

        public void UpdateColumns()
        {
            List<Column> newColumns = new List<Column>();
            foreach (Column column in _columns)
            {
                newColumns.Add(column);
            }
            Columns = newColumns;
        }

        public void LoadColumnNames(string tableName)
        {
            List<string> names = new List<string>();
            foreach (Table table in Tables)
            {
                if (table.Scheme.Name == tableName)
                {
                    names.Add(table.Scheme.Columns[0].Name);
                    break;
                }
            }
            ColumnNames = names;
        }
    }
}