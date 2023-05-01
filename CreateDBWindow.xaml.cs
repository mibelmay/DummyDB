using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DummyDB_5
{
    /// <summary>
    /// Interaction logic for CreateDBWindow.xaml
    /// </summary>
    public partial class CreateDBWindow : Window
    {
        public CreateDBWindow()
        {
            InitializeComponent();
        }

        private void Choose_Place(object sender, RoutedEventArgs e)
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
            DB_path.Text = $"{folderPath}";

        }
        private void CreateDB_Click(object sender, RoutedEventArgs e)
        {
            string name = DB_name.Text;
            string path = DB_path.Text + "\\" + name;
            Directory.CreateDirectory(path);
            ((CreateDBWindow)System.Windows.Application.Current.MainWindow).Close();
        }
    }
}
