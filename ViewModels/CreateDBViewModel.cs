using System;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;

namespace DummyDB.ViewModel
{
    public class CreateDBViewModel : ViewModel
    {
        public CreateDBWindow Window { get; set; }
        private string _path;
        public string Path
        {
            get 
            { 
                return _path;
            }
            set 
            { 
                _path = value;
                OnPropertyChanged();
            }
        }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set 
            { 
                _name = value; 
                OnPropertyChanged();
            }
        }
        public ICommand Choose_Place => new CommandDelegate(parameter =>
        {
            Path = "";
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            string path = "";

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
            if (Name == "" || Name == null || Path == "" || Path == null)
            {
                MessageBox.Show("Заполните поля");
                return;
            }
            string name = Name;
            string path = Path + "\\" + Name;
            if (Directory.Exists(path))
            {
                MessageBox.Show("Такая папка уже существует");
                return;
            }
            CreateFolder(path);
        });
        private void CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                MessageBox.Show($"Папка {Name} создана по пути {Path}");
                Window.Close();
            }
            catch
            {
                MessageBox.Show("Возможно вы ввели символы, которые нельзя использовать в названии папки");
                return;
            }
        }
    }
}
