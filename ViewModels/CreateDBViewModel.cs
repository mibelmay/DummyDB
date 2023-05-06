﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.Windows;

namespace DummyDB_5.ViewModel
{
    public class CreateDBViewModel : ViewModel
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged(); }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
        public ICommand Choose_Place => new CommandDelegate(parameter =>
        {
            Path = "";
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            string? path = "";

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFolderDialog.SelectedPath;
            }
            if (path == "")
            {
                return;
            }
            Path = path;

        });

        public ICommand CreateDB_Click => new CommandDelegate(parameter =>
        {
            string name = Name;
            string path = Path + "\\" + Name;
            Path = path;
            if (Directory.Exists(path))
            {
                Path = "Такая папка уже существует";
                return;
            }
            Directory.CreateDirectory(path);
        });
    }
}
