﻿using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
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

        public ObservableCollection<Column> _columns = new ObservableCollection<Column>();
        public IEnumerable<Column> Columns
        {
            get
            {
                return _columns;
            }
        }

        public string FolderPath { get; set; }
        public CreateTableWindow Window { get; set; }
        


        public ICommand AddColumn => new CommandDelegate(patameter =>
        {
            if (ColumnName == "" || Type == null) return;
            _columns.Add(new Column { Name = $"{ColumnName}", Type = $"{Type}"});
        });

        public ICommand CreateTable => new CommandDelegate(patameter =>
        {
            if(FolderPath == "")
            {
                MessageBox.Show("Вернитесь на главное окно и выберите папку");
                return;
            }
            if (TableName == "" || TableName == null || _columns.Count == 0)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            List<Column> columnsOfNewTable = new List<Column>();
            foreach (Column column in _columns)
            {
                columnsOfNewTable.Add(column);
            }
            TableScheme scheme = new TableScheme()
            {
                Name = TableName,
                Columns = columnsOfNewTable
            };
            string json = JsonSerializer.Serialize<TableScheme>(scheme);
            File.WriteAllText($"{FolderPath}\\{TableName}.json", json);
            CreateEmptyTable(scheme);
            MessageBox.Show($"Таблица создана по пути {FolderPath}");
            Window.Close();
        });

        public void CreateEmptyTable(TableScheme scheme)
        {
            string newFile = "";
            foreach(Column column in scheme.Columns)
            {
                string addValue = "";
                if (column.Type == "string")
                {
                    addValue = "";
                }
                else if (column.Type == "datetime")
                {
                    addValue = $"{DateTime.MinValue}";
                }
                else
                {
                    addValue = "0";
                }
                newFile = newFile + $"{addValue};";
            }
            newFile = newFile.Substring(0, newFile.Length - 1);
            File.WriteAllText($"{FolderPath}\\{TableName}.csv", newFile);
        }
    }
}
