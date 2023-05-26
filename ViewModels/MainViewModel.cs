using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data;

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
        public DataTable SelectedTable { get; set; }
        public string folderPath { get; set; }

        public ICommand OpenSourceClick => new CommandDelegate(parameter =>
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataGrid.Columns.Clear();
            schemeTablePairs.Clear();
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            folderPath = "";
            Message = "";
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = openFolderDialog.SelectedPath;
            }
            if (folderPath == "")
            {
                Message = "Папка не выбрана";
                return;
            }
            string[] splits = folderPath.Split('\\');
            string folderName = splits[splits.Length - 1];
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Header = folderName;
            LoadTreeView();
        });

        public void LoadTreeView()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Clear();
            List<TableScheme> schemes = LoadSchemes();
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                if (file.Contains(".csv"))
                {
                    Table table = LoadTable(schemes, file);
                    if (table == null)
                    {
                        string[] line = file.Split("\\");
                        Message = $"Не найдена схема для таблицы {line[line.Length - 1].Replace(".csv", "")}";
                    }
                }
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
                table = TableReader.Read(scheme, file);
                if (table == null)
                {
                    continue;
                }
                schemeTablePairs.Add(scheme, table);
                TreeViewItem treeItem = new TreeViewItem();
                string[] line = file.Split("\\");
                treeItem.Header = (line[line.Length - 1]).Substring(0, line[line.Length - 1].Length - 4);
                treeItem.Selected += TableTreeSelected;
                for (int i = 0; i < scheme.Columns.Count; i++)
                {
                    treeItem.Items.Add(new TreeViewItem {Header = scheme.Columns[i].Name + " - " + scheme.Columns[i].Type, });
                    ((TreeViewItem)treeItem.Items[i]).Selected += ColumnTreeSelected;
                }
                ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(treeItem);
                schemes.Remove(scheme);
                break;
            }
            return table;
        }
        private void ColumnTreeSelected(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show($"{((TreeViewItem)sender).Header.ToString()} вот такой узелок вы выбрали");
        }
        private void TableTreeSelected(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            string tableName = ((TreeViewItem)sender).Header.ToString();
            foreach(var pair in schemeTablePairs)
            {
                if(pair.Key.Name == tableName)
                {
                    dataTable.TableName = tableName;
                    foreach(Column column in pair.Key.Columns)
                    {
                        dataTable.Columns.Add(column.Name);
                    }
                    for(int i = 0; i < pair.Value.Rows.Count; i++)
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
            SelectedTable = dataTable;
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
            CreateDB.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
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
            CreateTable.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            CreateTable.ShowDialog();
        });

        public ICommand Edit_Click => new CommandDelegate(parameter =>
        {
            if (SelectedTable == null) return;
            EditWindow newWindow = new EditWindow();
            EditViewModel vmEdit = new EditViewModel();
            newWindow.DataContext = vmEdit;
            vmEdit.dataGrid = newWindow.dataGrid;
            vmEdit.DataTable = SelectedTable;
            vmEdit.TableName = SelectedTable.TableName;
            foreach (var pair in schemeTablePairs)
            {
                if (pair.Key.Name == SelectedTable.TableName) 
                { 
                    vmEdit.scheme = pair.Key; 
                    vmEdit.table = pair.Value; 
                    break;
                }
            }
            vmEdit.ColumnNames = CreateColumnNamesList(vmEdit.scheme);
            vmEdit.oldFileName = SelectedTable.TableName;
            vmEdit.folderPath = folderPath;
            newWindow.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
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
