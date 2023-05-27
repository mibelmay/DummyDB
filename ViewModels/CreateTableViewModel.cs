using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using System.IO;

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
        private List<Column> _columns = new List<Column>();
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
        
        public ICommand AddColumn => new CommandDelegate(patameter =>
        {
            if (ColumnName == "" || Type == null)
            {
                return;
            }
            if(IfColumnExist(ColumnName))
            {
                MessageBox.Show($"Столбец с именем {ColumnName} уже добавлен в таблицу");
                return;
            }
            _columns.Add(
                new Column 
                { 
                    Name = $"{ColumnName}", 
                    Type = $"{Type}"
                });
            UpdateColumns();
        });

        public ICommand CreateTable => new CommandDelegate(patameter =>
        {
            if (TableName == "" || TableName == null || _columns.Count == 0)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            TableScheme scheme = new TableScheme()
            {
                Name = TableName,
                Columns = _columns
            };
            CreateJson(scheme);
            CreateEmptyTable(scheme);
            MessageBox.Show($"Таблица создана по пути {FolderPath}");
            Window.Close();
        });

        public bool IfColumnExist(string columnName)
        {
            foreach (Column column in Columns)
            {
                if (column.Name == columnName)
                {
                    return true;
                }
            }
            return false;
        }

        public void CreateJson(TableScheme scheme)
        {
            string json = JsonSerializer.Serialize<TableScheme>(scheme);
            File.WriteAllText($"{FolderPath}\\{TableName}.json", json);
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
    }
}