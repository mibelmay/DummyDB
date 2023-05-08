using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyDB_5.Model
{
    public class Table
    {
        public TableScheme Scheme { get; set; }
        public List<Row> Rows { get; set; }
    }
}
