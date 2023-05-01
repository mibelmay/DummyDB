using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;

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

        public ICommand Choose_Place => new CommandDelegate(parameter =>
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            string? folderPath = "";

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = openFolderDialog.SelectedPath;
            }
            if (folderPath == "")
            {
                return;
            }
            Path = $"{folderPath}";

        });

        public ICommand CreateDB_Click => new CommandDelegate(parameter =>
        {
            //string name = DB_name.Text;
            //string path = DB_path.Text + "\\" + name;
            //Directory.CreateDirectory(path);
        });
    }
}
