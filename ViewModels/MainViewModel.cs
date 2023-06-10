using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data;
using System.Linq;

namespace DummyDB.ViewModel
{
    public class MainViewModel : ViewModel
    {
        public Dictionary<TableScheme, Table> schemeTablePairs = new Dictionary<TableScheme, Table>();
       
        private DataTable _dataTable;
        public DataTable DataTable
        {
            get { return _dataTable; }
            set
            {
                _dataTable = value;
                OnPropertyChanged();
            }
        }
        private DataTable _foreignKeys;
        public DataTable ForeignKeys
        {
            get { return _foreignKeys; }
            set
            {
                _foreignKeys = value;
                OnPropertyChanged();
            }
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set 
            {
                _message = value;
                OnPropertyChanged();    
            }
        }
        public DataTable SelectedDataTable { get; set; }
        public TableScheme SelectedTable { get; set; }
        public string folderPath { get; set; }
        public List<Table> Tables { get; set; }
        private List<string> _columnNames;
        public List<string> ColumnNames
        {
            get { return _columnNames; }
            set
            {
                _columnNames = value;
                OnPropertyChanged();
            }
        }
        private string _selectedColumn;
        public string SelectedColumn
        {
            get { return _selectedColumn; }
            set
            {
                _selectedColumn = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSourceClick => new CommandDelegate(parameter =>
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataGrid.Columns.Clear();
            schemeTablePairs.Clear();
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            folderPath = "";
            Message = "";
            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                folderPath = openFolderDialog.SelectedPath;
            }
            DisplayDataBase();
        });

        public void DisplayDataBase()
        {
            if (folderPath == "")
            {
                Message = "Папка не выбрана";
                return;
            }
            string[] splits = folderPath.Split('\\');
            string folderName = splits[splits.Length - 1];
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Header = folderName;
            LoadTreeView();
        }

        public void LoadTreeView()
        {
            Tables= new List<Table>();
            ((MainWindow)System.Windows.Application.Current.MainWindow).foreignKeys.Columns.Clear();
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Clear();
            List<TableScheme> schemes = LoadSchemes();
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                if (!file.Contains(".csv"))
                {
                    continue;   
                }
                Table table = LoadTable(schemes, file);
                if (table == null)
                {
                    string[] line = file.Split("\\");
                    Message = $"Не найдена схема для таблицы {line[line.Length - 1].Replace(".csv", "")}";
                }
                Tables.Add(table);
            }
        }

        public List<TableScheme> LoadSchemes()
        {
            List<TableScheme> schemes = new List<TableScheme>();
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                if (file.Contains(".json"))
                {
                    TableScheme scheme = TableScheme.ReadFile(file);
                    schemes.Add(scheme);
                }
            }
            return schemes;
        }

        public Table LoadTable(List<TableScheme> schemes, string file)
        {
            Table table = new Table();
            foreach (TableScheme scheme in schemes)
            {
                table = TableReader.Read(Tables, scheme, file);
                if (table == null)
                {
                    continue;
                }
                schemeTablePairs.Add(scheme, table);
                AddTreeItem(file, scheme);
                schemes.Remove(scheme);
                break;
            }
            return table;
        }

        public void AddTreeItem(string file, TableScheme scheme)
        {
            TreeViewItem treeItem = new TreeViewItem();
            string[] line = file.Split("\\");
            treeItem.Header = (line[line.Length - 1]).Substring(0, line[line.Length - 1].Length - 4);
            treeItem.Selected += TableTreeSelected;
            for (int i = 0; i < scheme.Columns.Count; i++)
            {
                treeItem.Items.Add(new TreeViewItem { Header = scheme.Columns[i].Name + " - " + scheme.Columns[i].Type + " - primary: " + scheme.Columns[i].IsPrimary });
            }
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(treeItem);
        }

        private void TableTreeSelected(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).foreignKeys.Columns.Clear();
            DataTable dataTable = new DataTable();
            string tableName = ((TreeViewItem)sender).Header.ToString();
            foreach(var pair in schemeTablePairs)
            {
                if(pair.Key.Name == tableName)
                {
                    dataTable.TableName = tableName;
                    AddColumnsToDataTable(pair.Key.Columns, dataTable);
                    AddRowsToDataTable(pair.Value.Rows, dataTable);
                    UpdateColumnNames(pair.Value);
                    SelectedTable = pair.Key;
                    break;
                }
            }
            DataTable = dataTable;
            SelectedDataTable = dataTable;
        }

        private void UpdateColumnNames(Table table)
        {
            List<string> columnNames = new List<string>();
            foreach(Column column in table.Scheme.Columns)
            {
                if(column.ReferencedColumn == null)
                {
                    continue;
                }
                columnNames.Add(column.Name);
            }
            ColumnNames = columnNames;
        }

        private void CreateForeignKeysTable(Table table, Column column)
        {
            DataTable newDataTable = new DataTable();
            Table primaryTable = ReferenceChecker.FindTable(schemeTablePairs.Values.ToList(), column.ReferencedTable);
            Column primaryColumn = primaryTable.Scheme.Columns.Find(el => el.Name == column.ReferencedColumn);
            var ids = table.Rows.Select(row => row.Data[column]);
            AddColumnsToDataTable(primaryTable.Scheme.Columns, newDataTable);
            List<Row> newRows= new List<Row>();
            foreach(Row row in primaryTable.Rows)
            {
                if (ids.Contains(row.Data[primaryColumn]))
                {
                    newRows.Add(row);
                }
            }
            AddRowsToDataTable(newRows, newDataTable);
            ForeignKeys = newDataTable;
        }

        public ICommand ShowForeignKeys => new CommandDelegate(param =>
        {
            Table table = Tables.Find(el => el.Scheme.Name == SelectedDataTable.TableName);
            Column column = table.Scheme.Columns.Find(col => col.Name == SelectedColumn);
            CreateForeignKeysTable(table, column);
        });

        private void AddColumnsToDataTable(List<Column> columns, DataTable dataTable)
        {
            foreach (Column column in columns)
            {
                dataTable.Columns.Add(column.Name);
            }
        }

        private void AddRowsToDataTable(List<Row> rows, DataTable dataTable)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                DataRow newRow = dataTable.NewRow();
                foreach (var rowPair in rows[i].Data)
                {
                    newRow[rowPair.Key.Name] = rowPair.Value;
                }
                dataTable.Rows.Add(newRow);
            }
        }

        public ICommand Update_Click => new CommandDelegate(parameter =>
        {
            if (folderPath == null || folderPath == "")
            {
                return;
            }
            LoadTreeView();
        });

        public ICommand CreateDB_Click => new CommandDelegate(parameter =>
        {
            CreateDBWindow CreateDB = new CreateDBWindow();
            CreateDBViewModel vmCreate = new CreateDBViewModel();
            CreateDB.DataContext = vmCreate;
            vmCreate.Window = CreateDB;
            CreateDB.ShowDialog();
        });

        public ICommand CreateTable_Click => new CommandDelegate(parameter =>
        {
            if(folderPath == null || folderPath == "")
            {
                Message = "Выберите путь до базы данных";
                return;
            }
            CreateTableWindow CreateTable = new CreateTableWindow();
            CreateTableViewModel vmCreate = new CreateTableViewModel();
            CreateTable.DataContext = vmCreate;
            vmCreate.FolderPath = folderPath;
            vmCreate.Window = CreateTable;
            vmCreate.Tables = Tables;
            vmCreate.TableNames = Tables.Select(x => x.Scheme.Name).ToList();
            CreateTable.ShowDialog();
        });

        public void Click(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            System.Windows.MessageBox.Show("ура победа");
        }
        public ICommand Edit_Click => new CommandDelegate(parameter =>
        {
            if (SelectedDataTable == null) return;
            EditWindow newWindow = new EditWindow();
            EditViewModel vmEdit = new EditViewModel();
            newWindow.DataContext = vmEdit;
            vmEdit.dataGrid = newWindow.dataGrid;
            vmEdit.DataTable = SelectedDataTable;
            vmEdit.TableName = SelectedDataTable.TableName;
            vmEdit.scheme = SelectedTable;
            vmEdit.table = schemeTablePairs[SelectedTable];
            vmEdit.Tables = schemeTablePairs.Values.ToList();
            vmEdit.ColumnNames = CreateColumnNamesList(vmEdit.scheme);
            vmEdit.oldFileName = SelectedDataTable.TableName;
            vmEdit.TableNames = Tables.Select(x => x.Scheme.Name).ToList();
            vmEdit.folderPath = folderPath;
            newWindow.ShowDialog();
        });

        private List<string> CreateColumnNamesList(TableScheme scheme)
        {
            List<string> columns = new List<string>();
            foreach (Column column in scheme.Columns)
            {
                columns.Add(column.Name);
            }
            return columns;
        }
    }
}