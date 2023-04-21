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

namespace DummyDB_5.ViewModel
{
    public class MainViewModel : ViewModel
    {

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
                if (file.EndsWith(".json"))
                {
                    TableScheme scheme = TableScheme.ReadFile(file);
                }
                ((MainWindow)System.Windows.Application.Current.MainWindow).dataTree.Items.Add(file.Split("\\")[file.Split("\\").Length - 1]);
            }
        });
    }

}
