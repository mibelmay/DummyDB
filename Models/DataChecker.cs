using DummyDB.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DummyDB_5.Models
{
    public class DataChecker
    {
        DataTable DataTable { get; set; }
        public DataChecker(DataTable dataTable) 
        { 
            DataTable= dataTable;
        }

        public object CheckData(DataRow row, Column column)
        {
            string value = row[column.Name].ToString();
            switch (column.Type)
            {
                case ("int"):
                    {
                        return IntCase(value, row, column);
                    }
                case ("uint"):
                    {
                        return UintCase(value, row, column);
                    }
                case ("datetime"):
                    {
                        return DateTimeCase(value, row, column);
                    }
                case ("double"):
                    {
                        return DoubleCase(value, row, column);
                    }
                default:
                    break;
            }
            return value;
        }
        public object IntCase(string value, DataRow row, Column column)
        {
            if (int.TryParse(value, out int data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1} в столбце {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object UintCase(string value, DataRow row, Column column)
        {
            if (uint.TryParse(value, out uint data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1} в столбце {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object DateTimeCase(string value, DataRow row, Column column)
        {
            if (DateTime.TryParse(value, out DateTime data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1}  в столбце  {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public object DoubleCase(string value, DataRow row, Column column)
        {
            if (double.TryParse(value, out double data))
            {
                return data;
            }
            else
            {
                ShowMessage($"В сроке {DataTable.Rows.IndexOf(row) + 1}  в столбце  {column.Name} ({column.Type}) указаны некорректные данные");
                return null;
            }
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
