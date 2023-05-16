using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DummyDB.ViewModel;
using System.Windows.Shapes;

namespace DummyDB
{
    /// <summary>
    /// Interaction logic for CreateTableWindow.xaml
    /// </summary>
    public partial class CreateTableWindow : Window
    {
        public CreateTableWindow()
        {
            InitializeComponent();
            DataContext = new CreateTableViewModel();
        }
    }
}
