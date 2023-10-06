using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public partial class Tables
    {
        public StatsData GetStats(string name, bool removeEmptyRows, bool removeNonExistentCountries, bool removeWaterProvinces)
        {
            StatsData output;

            var tables = SelectByName(name);
            if (tables == null || tables.Count < 1)
            {
                Core.Log.ReportError("Could not find compatible table(s) named: " + name + ".");
                return new StatsData();
            }
            else
            {
                if (tables[0] is SingleValueTable)
                {
                    var svt = tables.Cast<SingleValueTable>().ToList();
                    svt.Sort((x, y) => x.Timepoint.CompareTo(y.Timepoint));
                    output = GetSingleValueStats(svt.ToArray());
                }
                else if (tables[0] is SingleDatakeyTable)
                {
                    var sdt = tables.Cast<SingleDatakeyTable>().ToList();
                    sdt.Sort((x, y) => x.Timepoint.CompareTo(y.Timepoint));
                    output = GetSingleDatakeyStats(sdt.ToArray());
                }
                else
                {
                    Core.Log.ReportError("Tables not compatible with time-based stats: " + name + ".");
                    return new StatsData();
                }
            }

            bool countryBased = tables[0].Type == TableType.Country;

            // Compact data
            List<ushort> removedRows = new List<ushort>();
            if (removeWaterProvinces && !countryBased)
            {
                List<ushort> waterProvs = Core.Data.Defs.Provinces.ListWaterProvinces().ToList();
                removedRows.AddRange(waterProvs);
            }
            if (removeNonExistentCountries && countryBased)
            {
                List<ushort> nonExistentCountries = GetNonExistentCountriesIDs().ToList();
                nonExistentCountries.Remove((ushort)0);
                removedRows.AddRange(nonExistentCountries);
            }
            removedRows = removedRows.Distinct().ToList();
            for (int i = (output.Rows.Count - 1); i >= 0; i--)
            {
                if (removedRows.Contains((ushort)i)) { output.Rows.RemoveAt(i); }
            }

            if (removeEmptyRows)
            {
                output.RemoveEmptyRows();
            }

            return output;
        }

        StatsData GetSingleValueStats(SingleValueTable[] tables)
        {
            StatsData output = new StatsData();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            List<string> xAxis = tables.Select(x => x.Timepoint).Select(x => x.GetString("yyy-MM-dd")).ToList();
            xAxis.Insert(0, countryBased ? "Country" : "Province");
            List<string> yAxis = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();
            output.Columns = xAxis.ToArray();

            // Fill in the table
            for (int i = 0; i < tables.Length; i++)
            {
                double[] data = tables[i].GetVector();
                output.FillColumn(i, yAxis.ToArray(), data);
            }
            
            return output;
        }

        StatsData GetSingleDatakeyStats(SingleDatakeyTable[] tables)
        {
            StatsData output = new StatsData();

            bool countryBased = tables[0].Type == TableType.Country;

            // Prepare axes descriptions
            List<string> xAxis = tables.Select(x => x.Timepoint).Select(x => x.GetString("yyy-MM-dd")).ToList();
            xAxis.Insert(0, countryBased ? "Country" : "Province");
            List<string> yAxis = (countryBased ? Core.Data.Defs.Countries.List("({0}) {1}") : Core.Data.Defs.Provinces.List("({0}) {1}")).ToList();
            output.Columns = xAxis.ToArray();

            // Fill in the table
            for (int i = 0; i < tables.Length; i++)
            {
                string[] data = tables[i].GetVector();
                output.FillColumn(i, yAxis.ToArray(), data);
            }

            return output;
        }
    }
}
