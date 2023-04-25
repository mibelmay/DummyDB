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

namespace DummyDB_5.ViewModel
{
    public class MainViewModel : ViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }


        private Dictionary<TableScheme, Table> schemesAndTablesDict = new Dictionary<TableScheme, Table>();

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
                    Table table = ReadTable.Read(scheme, pathTable);
                    schemesAndTablesDict.Add(scheme, table);
                    TreeViewItem treeItem = new TreeViewItem();
                    string[] line = file.Split("\\");
                    treeItem.Header = (line[line.Length - 1]).Substring(0, line[line.Length - 1].Length-5);
                    treeItem.Selected += TableTreeSelected;
                    treeItem.Unselected += TableTreeUnselected;

                    foreach (Column key in scheme.Columns)
                    {
                        treeItem.Items.Add(key.Name + "---" + key.Type);
                    }
                    ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(treeItem);
                }
                
            }
        });


        private class RowAdapter
        {
            public List<Object> Data { get; set; }
        }

        private void TableTreeSelected(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).DataTable.Columns.Clear();
            string tableName = ((TreeViewItem)sender).Header.ToString();

            foreach (var schemeAndTable in schemesAndTablesDict)
            {
                if (schemeAndTable.Key.Name == tableName)
                {

                    List<RowAdapter> rowsData = new List<RowAdapter>();

                    foreach (Row row in schemeAndTable.Value.Rows)
                    {
                        List<object> rowData = new List<object>();
                        foreach (object cell in row.Data.Values)
                        {
                            rowData.Add(cell);
                        }
                        rowsData.Add(new RowAdapter() { Data = rowData });
                    }

                    ((MainWindow)System.Windows.Application.Current.MainWindow).DataTable.ItemsSource = rowsData;

                    for (int i = 0; i < schemeAndTable.Key.Columns.Count; i++)
                    {
                        DataGridTextColumn tableTextColumn = new DataGridTextColumn()
                        {
                            Header = schemeAndTable.Key.Columns[i].Name,
                            Binding = new System.Windows.Data.Binding($"Data[{i}]")
                        };

                        ((MainWindow)System.Windows.Application.Current.MainWindow).DataTable.Columns.Add(tableTextColumn);
                    }
                    break;
                }
            }
        }

        private void TableTreeUnselected(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).DataTable.Columns.Clear();
        }
    }

}
