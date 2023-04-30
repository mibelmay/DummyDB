using DummyDB_5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Data;
using System.Drawing.Drawing2D;

namespace DummyDB_5.ViewModel
{
    public class MainViewModel : ViewModel
    {
        private Dictionary<TableScheme, Table> schemeTablePairs = new Dictionary<TableScheme, Table>();
        private List<TableScheme> schemes = new List<TableScheme>();
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
        public ICommand OpenSourceClick => new CommandDelegate(parameter =>
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            string folderPath = "";
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = openFolderDialog.SelectedPath;
            }
            string[] splits = folderPath.Split('\\');
            string folderName = splits[splits.Length - 1];
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Header = folderName;
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                if (file.Contains(".json"))
                {
                    string pathTable = file.Replace("json", "csv");
                    TableScheme scheme = TableScheme.ReadFile(file);
                    schemes.Add(scheme);
                    Table table = ReadTable.Read(scheme, pathTable);
                    schemeTablePairs.Add(scheme, table);
                    TreeViewItem treeItem = new TreeViewItem();
                    string[] line = file.Split("\\");
                    treeItem.Header = (line[line.Length - 1]).Substring(0, line[line.Length - 1].Length-5);
                    treeItem.Selected += TableTreeSelected;
                    //treeItem.Unselected += TableTreeUnselected;

                    foreach (Column key in scheme.Columns)
                    {
                        treeItem.Items.Add(key.Name + " | " + key.Type);
                    }
                    ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(treeItem);
                    
                }
                
            }
            Message = "Все таблицы успешно загружены";
        });


        private void TableTreeSelected(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataGrid.Columns.Clear();
            DataTable dataTable = new DataTable();
            string tableName = ((TreeViewItem)sender).Header.ToString();
            foreach(var pair in schemeTablePairs)
            {
                if(pair.Key.Name == tableName)
                {
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
        }

        //private void TableTreeUnselected(object sender, RoutedEventArgs e)
        //{
        //    ((MainWindow)System.Windows.Application.Current.MainWindow).dataGrid.Columns.Clear();
        //}
    }

}
