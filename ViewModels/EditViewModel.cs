using DummyDB_5.Model;
using System.Text.Json;
using System.Data;
using System.Windows.Input;
using System.Windows;

namespace DummyDB_5.ViewModel
{
    public class EditViewModel : ViewModel
    {
        private DataTable _dataTable;
        public DataTable DataTable
        {
            get { return _dataTable; }
            set { _dataTable = value; OnPropertyChanged(); }
        }
        private string _tableName;
        public string TableName
        {
            get { return _tableName; }
            set 
            { 
                if (value == _tableName) return; 
                _tableName = value; 
                OnPropertyChanged();
            }
        }
        private TableScheme scheme;
        public EditViewModel() 
        {
            DataTable = MainViewModel.selectedTable;
            TableName = MainViewModel.selectedTable.TableName;
            foreach (var pair in MainViewModel.schemeTablePairs)
            {
                if (pair.Key.Name == TableName) { scheme = pair.Key; break; }
            }
        }

        public ICommand Save => new CommandDelegate(parameter =>
        {
            if (_tableName != scheme.Name)
            {
                scheme.Name = _tableName;
            }
            string json = JsonSerializer.Serialize(scheme);
            MessageBox.Show(json);
        });

    }
}
