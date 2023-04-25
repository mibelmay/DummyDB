using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DummyDB_5.Model
{
    class ReadTable
    {
        public static Table Read(TableScheme scheme, string path)
        {
            string[] data = File.ReadAllLines(path);
            Table table = new Table();
            table.Rows = new List<Row>();
            table.Scheme = scheme;

            for (int i = 0; i < data.Length; i++)
            {
                string[] line = data[i].Split(';');

                if (line.Length != scheme.Columns.Count)
                {
                    throw new Exception($"В строке {i + 1} неверное количество столбцов");
                }

                table.Rows.Add(AddRow(scheme, line, i));
            }

            return table;
        }

        public static Row AddRow(TableScheme scheme, string[] line, int i)
        {
            Row row = new Row();
            for (int j = 0; j < line.Length; j++)
            {
                switch (scheme.Columns[j].Type)
                {
                    case ("int"):
                        {
                            if (int.TryParse(line[j], out int data))
                            {
                                row.Data.Add(scheme.Columns[j], data);
                            }
                            else
                            {
                                throw new Exception($"В сроке {i + 1} в столбце {j + 1} указаны некорректные данные");
                            }
                            break;
                        }
                    case ("uint"):
                        {
                            if (uint.TryParse(line[j], out uint data))
                            {
                                row.Data.Add(scheme.Columns[j], data);
                            }
                            else
                            {
                                throw new Exception($"В сроке {i + 1} в столбце {j + 1} указаны некорректные данные");
                            }
                            break;
                        }
                    case ("datetime"):
                        {
                            if (DateTime.TryParse(line[j], out DateTime data))
                            {
                                row.Data.Add(scheme.Columns[j], data);
                            }
                            else
                            {
                                throw new Exception($"В сроке {i + 1} в столбце {j + 1} указаны некорректные данные");
                            }
                            break;
                        }
                    case ("double"):
                        {
                            if (double.TryParse(line[j], out double data))
                            {
                                row.Data.Add(scheme.Columns[j], data);
                            }
                            else
                            {
                                throw new Exception($"В сроке {i + 1} в столбце {j + 1} указаны некорректные данные");
                            }
                            break;
                        }
                    default:
                        row.Data.Add(scheme.Columns[j], line[j]);
                        break;
                }

            }

            return row;
        }
    }
}
