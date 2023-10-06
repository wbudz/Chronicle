using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public interface IStatsSource
    {
        string[] Columns { get; }
        string GetText(string delimiter);
    }

    public class StatsData : IStatsSource
    {
        public string[] Columns { get; set; }
        public ObservableCollection<StatsRow> Rows { get; set; } = new ObservableCollection<StatsRow>();

        public void AddRow(string header, double[] data)
        {
            Rows.Add(new StatsRowNumValue(header, data));
        }

        public void AddRow(string header, string[] data)
        {
            Rows.Add(new StatsRowTextValue(header, data));
        }

        public void FillColumn(int column, string[] headers, double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (i > Rows.Count - 1)
                {
                    double[] row = new double[Columns.Length - 1];
                    row[column] = data[i];
                    if (i < headers.Length)
                        AddRow(headers[i], row);
                }
                else
                {
                    Rows[i].Header = headers[i];
                    (Rows[i] as StatsRowNumValue).Data[column] = data[i];
                }
            }
        }

        public void FillColumn(int column, string[] headers, string[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (i > Rows.Count - 1)
                {
                    string[] row = new string[Columns.Length - 1];
                    row[column] = data[i];
                    if (i < headers.Length)
                        AddRow(headers[i], row);
                }
                else
                {
                    Rows[i].Header = headers[i];
                    (Rows[i] as StatsRowTextValue).Data[column] = data[i];
                }
            }
        }

        public string GetText(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Columns.Length; i++)
            {
                sb.Append(Columns[i]);
                sb.Append(delimiter);
            }
            sb.AppendLine();

            for (int i = 0; i < Rows.Count; i++)
            {
                sb.AppendLine(Rows[i].GetText(delimiter));
            }

            return sb.ToString();
        }

        public void RemoveEmptyRows()
        {
            for (int i = Rows.Count - 1; i > 0; i--)
            {
                if (Rows[i].IsEmpty()) Rows.RemoveAt(i);
            }
        }

        public List<KeyValuePair<ushort, double>> GetBest(int count, int column)
        {
            List<KeyValuePair<ushort, double>> output = new List<KeyValuePair<ushort, double>>();
            if (Rows.Count == 0 || Rows[0] is StatsRowTextValue) return output;

            if (column < 0 || column >= Columns.Length)
            {
                for (ushort i = 0; i < Rows.Count; i++)
                {
                    output.Add(new KeyValuePair<ushort, double>(i, (Rows[i] as StatsRowNumValue).Data.Sum() / (float)Columns.Length));
                }
            }
            else
            {
                for (ushort i = 0; i < Rows.Count; i++)
                {
                    output.Add(new KeyValuePair<ushort, double>(i, (Rows[i] as StatsRowNumValue).Data[column]));
                }
            }

            output.Sort((a, b) => b.Value.CompareTo(a.Value));

            return count > 0 ? output.Take(count).ToList() : output;
        }
    }

    public abstract class StatsRow
    {
        public string Header { get; set; }

        public abstract string GetText(string delimiter);
        public abstract bool IsEmpty();
    }

    public class StatsRowNumValue : StatsRow
    {
        public double[] Data { get; set; }

        public StatsRowNumValue(string header, double[] data)
        {
            Header = header;
            Data = data;
        }

        public override string GetText(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Header);
            sb.Append(delimiter);
            for (int i = 0; i < Data.Length; i++)
            {
                sb.Append(Data[i].ToString("F" + Core.Settings.TableDisplayPrecision, Core.OriginalCulture));
                sb.Append(delimiter);
            }
            return sb.ToString();
        }

        public override bool IsEmpty()
        {
            return Data.All(x => x == 0);
        }
    }

    public class StatsRowTextValue : StatsRow
    {
        public string[] Data { get; set; }

        public StatsRowTextValue(string header, string[] data)
        {
            Header = header;
            Data = data;
        }

        public override string GetText(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Header);
            sb.Append(delimiter);
            for (int i = 0; i < Data.Length; i++)
            {
                sb.Append(Data[i]);
                sb.Append(delimiter);
            }
            return sb.ToString();
        }

        public override bool IsEmpty()
        {
            return Data.All(x => x == "");
        }
    }

    public class StatsPivot : IStatsSource
    {
        public string BaseKeyHeader { get; set; }
        public string DataKeyHeader { get; set; }
        public ObservableCollection<PivotRow> Rows { get; set; } = new ObservableCollection<PivotRow>();

        public string[] Columns
        {
            get
            {
                return new string[] { "Date", BaseKeyHeader, DataKeyHeader, "Value" };
            }
        }

        public StatsPivot()
        {
            BaseKeyHeader = "Basekey";
            DataKeyHeader = "Datakey";
        }

        public void AddRow(string date, string basekey, string datakey, double value)
        {
            Rows.Add(new PivotRowNumValue
            {
                Date = date,
                Basekey = basekey,
                Datakey = datakey,
                Value = value
            });
        }

        public void AddRow(string date, string basekey, string dataKey, string value)
        {
            Rows.Add(new PivotRowTextValue
            {
                Date = date,
                Basekey = basekey,
                Datakey = dataKey,
                Value = value
            });
        }

        public string GetText(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Columns.Length; i++)
            {
                sb.Append(Columns[i]);
                sb.Append(delimiter);
            }
            sb.AppendLine();

            for (int i = 0; i < Rows.Count; i++)
            {
                sb.AppendLine(Rows[i].GetText(delimiter));
            }

            return sb.ToString();
        }

        public void RemoveEmptyRows()
        {
            for (int i = Rows.Count - 1; i > 0; i--)
            {
                if (Rows[i].IsEmpty()) Rows.RemoveAt(i);
            }
        }
    }

    public abstract class PivotRow
    {
        public string Date { get; set; }
        public string Basekey { get; set; }
        public string Datakey { get; set; }

        public abstract string GetText(string delimiter);
        public abstract bool IsEmpty();
    }

    public class PivotRowNumValue : PivotRow
    {
        public double Value { get; set; }

        public override string GetText(string delimiter)
        {
            return Date + delimiter + Basekey + delimiter + Datakey + delimiter + Value.ToString("F" + Core.Settings.TableDisplayPrecision, Core.OriginalCulture) + delimiter;
        }

        public override bool IsEmpty()
        {
            return Value == 0;
        }
    }

    public class PivotRowTextValue : PivotRow
    {
        public string Value { get; set; }

        public override string GetText(string delimiter)
        {
            return Date + delimiter + Basekey + delimiter + Datakey + delimiter + Value + delimiter;
        }

        public override bool IsEmpty()
        {
            return Value == "";
        }
    }
}
