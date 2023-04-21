using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyDB_5.Model
{
    class Row
    {
        public Dictionary<Column, object> Data { get; set; }
        public Row()
        {
            Data = new Dictionary<Column, object>();
        }
    }
}
