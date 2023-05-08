﻿using DummyDB_5.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data;

namespace DummyDB_5.ViewModel
{
    public class MainViewModel : ViewModel
    {
        
        public static Dictionary<TableScheme, Table> schemeTablePairs = new Dictionary<TableScheme, Table>();
       
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

        public static DataTable selectedTable { get; set; }
        public static string folderPath { get; set; }
        public ICommand CreateDB_Click => new CommandDelegate(parameter =>
        {
            CreateDBWindow CreateDB = new CreateDBWindow();
            CreateDB.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            CreateDB.ShowDialog();
        });

        public ICommand OpenSourceClick => new CommandDelegate(parameter =>
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Clear();
            schemeTablePairs.Clear();
            List<TableScheme> schemes = new List<TableScheme>();
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            folderPath = "";
            Message = "";

            if ( openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = openFolderDialog.SelectedPath;
            }
            if (folderPath == "")
            {
                Message = "Вы не выбрали папку";
                return;
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
                }
            }
            foreach(string file in Directory.EnumerateFiles(folderPath))
            {
                if (file.Contains(".csv"))
                {
                    Table table = new Table();
                    foreach (TableScheme scheme in schemes) 
                    {
                        try 
                        {
                            table = ReadTable.Read(scheme, file);
                            schemeTablePairs.Add(scheme, table);
                            TreeViewItem treeItem = new TreeViewItem();
                            string[] line = file.Split("\\");
                            treeItem.Header = (line[line.Length - 1]).Substring(0, line[line.Length - 1].Length - 4);
                            treeItem.Selected += TableTreeSelected;
                          

                            foreach (Column key in scheme.Columns)
                            {
                                treeItem.Items.Add(key.Name + " - " + key.Type);
                            }
                            ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(treeItem);
                            break;
                        }
                        catch (Exception ex) { continue; }
                        
                    }
                    if (table.Rows == null)
                    {
                        string[] line = file.Split("\\");
                        Message = $"Не найдена схема для таблицы {line[line.Length - 1].Replace(".csv", "")} или в таблице некорректные данные";
                    }
                    
                }
            }
           
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
            selectedTable = dataTable;
        }

        public ICommand CreateTable_Click => new CommandDelegate(parameter =>
        {
            CreateTableWindow CreateTable = new CreateTableWindow();
            CreateTable.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            CreateTable.ShowDialog();
        });

        public ICommand Edit_Click => new CommandDelegate(parameter =>
        {
            if (selectedTable == null) return;
            EditWindow EditWindow = new EditWindow();
            EditWindow.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            EditWindow.ShowDialog();
        });

    }

}
