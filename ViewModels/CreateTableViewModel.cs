using DummyDB_5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace DummyDB_5.ViewModel
{
    public class CreateTableViewModel : ViewModel
    {
        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set
            {
                if (value == _columnName) return;
                _columnName = value;
                OnPropertyChanged();
            }
        }

        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Column> _columns = new ObservableCollection<Column>();
        public IEnumerable<Column> Columns => _columns;

        //public CreateTableViewModel()
        //{
        //    _columns = new ObservableCollection<Column>()
        //    {
        //        new Column{Name = "ID", Type = "string"},
        //    };
        //}


        public ICommand AddColumn => new CommandDelegate(patameter =>
        {
            _columns.Add(new Column { Name = $"{ColumnName}", Type = $"{Type}"});
        });
    }
}
