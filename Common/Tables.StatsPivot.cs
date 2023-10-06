using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public partial class Tables
    {
        public StatsPivot GetPivot(string name, bool removeEmptyRows)
        {
            // Pivot buildup: 1. column: timepoint, 2. column: ID; 3. column: key, 4. column: value
            StatsPivot output;

            var tables = SelectByName(name);
            if (tables == null || tables.Count < 1)
            {
                Core.Log.ReportError("Could not find compatible table(s) named: " + name + ".");
                return new StatsPivot();
            }
            else
            {
                if (tables[0] is SingleValueTable)
                {
                    var t = tables.Cast<SingleValueTable>().ToArray();
                    Array.Sort(t);
                    output = GetSingleValuePivot(t);
                }
                else if (tables[0] is SingleDatakeyTable)
                {
                    var t = tables.Cast<SingleDatakeyTable>().ToArray();
                    Array.Sort(t);
                    output = GetSingleDatakeyPivot(t);
                }
                else if (tables[0] is MultiValueTable)
                {
                    var t = tables.Cast<MultiValueTable>().ToArray();
                    Array.Sort(t);
                    output = GetMultiValuePivot(t);
                }
                else if (tables[0] is MultiDatakeyTable)
                {
                    var t = tables.Cast<MultiDatakeyTable>().ToArray();
                    Array.Sort(t);
                    output = GetMultiDatakeyPivot(t);
                }
                else
                {
                    Core.Log.ReportError("Tables not compatible with time-based stats: " + name + ".");
                    return new StatsPivot();
                }
            }

            bool countryBased = tables[0].Type == TableType.Country;

            if (removeEmptyRows)
            {
                output.RemoveEmptyRows();
            }

            return output;
        }

        StatsPivot GetSingleValuePivot(SingleValueTable[] tables)
        {
            StatsPivot output = new StatsPivot();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            output.BaseKeyHeader = countryBased ? "Country" : "Province";
            var entities = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();

            for (int i = 0; i < tables.Length; i++)
            {
                double[] values = tables[i].GetVector();
                for (int j = 0; j < values.Length && j < entities.Count; j++)
                {
                    output.AddRow(tables[i].Timepoint.GetString("yyyy-MM-dd"), entities[j], "", values[j]);
                }
            }

            return output;
        }

        StatsPivot GetSingleDatakeyPivot(SingleDatakeyTable[] tables)
        {
            StatsPivot output = new StatsPivot();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            output.BaseKeyHeader = countryBased ? "Country" : "Province";
            var entities = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();

            for (int i = 0; i < tables.Length; i++)
            {
                string[] values = tables[i].GetVector();
                for (int j = 0; j < values.Length && j < entities.Count; j++)
                {
                    output.AddRow(tables[i].Timepoint.GetString("yyyy-MM-dd"), entities[j], "", values[j]);
                }
            }

            return output;
        }

        StatsPivot GetMultiValuePivot(MultiValueTable[] tables)
        {
            StatsPivot output = new StatsPivot();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            output.BaseKeyHeader = countryBased ? "Country" : "Province";
            var entities = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();

            for (int i = 0; i < tables.Length; i++)
            {
                double[,] values = tables[i].Get();
                string[] datakeys = tables[i].GetAllDatakeys(true);

                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (values[2, j] != 0)
                    {
                        if ((int)values[0, j] >= entities.Count) continue;
                        if ((int)values[1, j] >= datakeys.Length) continue;
                        output.AddRow(tables[i].Timepoint.GetString("yyyy-MM-dd"), entities[(int)values[0, j]], datakeys[(int)values[1, j]], values[2, j]);
                    }
                }
            }

            return output;
        }

        StatsPivot GetMultiDatakeyPivot(MultiDatakeyTable[] tables)
        {
            StatsPivot output = new StatsPivot();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            output.BaseKeyHeader = countryBased ? "Country" : "Province";
            var entities = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();

            for (int i = 0; i < tables.Length; i++)
            {
                double[,] values = tables[i].Get();
                string[] datakeys = tables[i].GetAllDatakeys(true);
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if ((int)values[0, j] >= entities.Count) continue;
                    if ((int)values[1, j] >= datakeys.Length) continue;
                    output.AddRow(tables[i].Timepoint.GetString("yyyy-MM-dd"), entities[(int)values[0, j]], datakeys[(int)values[1, j]], values[2, j]);
                }
            }

            return output;
        }
    }
}
