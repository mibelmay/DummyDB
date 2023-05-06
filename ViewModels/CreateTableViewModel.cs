﻿using DummyDB_5.Model;
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

namespace DummyDB_5.ViewModel
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
        public IEnumerable<Column> Columns => _columns;
        public List<Column> columnsOfNewTable = new List<Column>();


        public ICommand AddColumn => new CommandDelegate(patameter =>
        {
            if (ColumnName == "" || Type == null) return;
            _columns.Add(new Column { Name = $"{ColumnName}", Type = $"{Type}"});
        });

        public ICommand CreateTable => new CommandDelegate(patameter =>
        {
            foreach (Column column in _columns)
            {
                columnsOfNewTable.Add(column);
            }
            TableScheme scheme = new TableScheme()
            {
                Name = TableName,
                Columns = columnsOfNewTable
            };
            string json = JsonSerializer.Serialize(scheme);
            MessageBox.Show(json);
        });
    }
}
