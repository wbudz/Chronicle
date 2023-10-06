using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public class Mapmode
    {
        public string Name { get; private set; }
        public string Label { get; private set; }
        public string Category { get; set; }
        public int Section { get; set; }
        public bool RequiresSavegame { get; set; }
        public string Table { get; private set; }
        public bool CountryBased { get; private set; }
        public bool Special { get; private set; }
        public bool Multivalue { get; private set; }
        public bool RequiresRefreshUponClick { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public bool RenderEmptyProvinces { get; set; }

        public Mapmode(string name, string label, string category, int section, string table)
        {
            this.Name = name;
            this.Label = label;
            this.Category = category;
            this.Section = section;
            this.RequiresSavegame = table != null;
            this.Table = table;
            this.RequiresRefreshUponClick = false;
            this.Min = double.MinValue;
            this.Max = double.MaxValue;
        }

        //public double GetMinimum(Datamode mode, bool water, DateTime timestamp)
        //{
        //    if (this.AssociatedTable == "") return double.NaN;
        //    if (water && !Colorscales.HasWaterColorscale()) return double.NaN;
        //    if (!water && !Colorscales.HasLandColorscale()) return double.NaN;
        //    if (mode.Min != double.MinValue) return mode.Min;
        //    bool[] waterlist = Data.Defs.Provinces.WaterProvinces();

        //    double[] data = Data.Tables.GetDataVector("", this.AssociatedTable, timestamp);

        //    //if (water && data.Length < Data.Defs.Provinces.Count) return double.NaN;
        //    double[] values = data.Where((x, i) => waterlist[i % Data.Defs.Provinces.Count] == water).ToArray();
        //    return values.Length > 0 ? values.Min() : double.NaN;
        //}

        //public double GetMaximum(Datamode mode, bool water, DateTime timestamp)
        //{
        //    if (this.AssociatedTable == "") return double.NaN;
        //    if (water && !Colorscales.HasWaterColorscale()) return double.NaN;
        //    if (!water && !Colorscales.HasLandColorscale()) return double.NaN;
        //    if (mode.Max != double.MaxValue) return mode.Max;
        //    bool[] waterlist = Data.Defs.Provinces.WaterProvinces();
        //    double[] data = Data.Tables.GetDataVector("", this.AssociatedTable, timestamp);
        //    //if (water && data.Length < Data.Defs.Provinces.Count) return double.NaN;
        //    double[] values = data.Where((x, i) => waterlist[i % Data.Defs.Provinces.Count] == water).ToArray();
        //    return values.Length > 0 ? values.Max() : double.NaN;
        //}

        public int[] GetColor(GameDate timestamp, ushort selectedCountry)
        {
            int[] output = new int[Data.ProvincesCount];
            double[] data = Data.Tables.GetDataVector(Table, timestamp);
            ushort[] master = Data.GetProvinceMasterVector(timestamp);
            output[0] = Core.WaterColor;

            if (data == null)
            {

                for (int i = 1; i < output.Length; i++)
                {
                    output[i] = Data.IsWater(i) ? Core.WaterColor : Core.LandColor;
                }
                return output;
            }

            double min = Min;
            double max = Max;
            if (min == double.MinValue) min = data.Min();
            if (max == double.MaxValue) max = data.Max();

            Parallel.For(1, output.Length, (i) =>
            {
                if (master[i] != 0 || RenderEmptyProvinces)
                    output[i] = Colorscales.GetColor(Data.IsWater(i), data[CountryBased ? master[i] : i], min, max);
                else
                    output[i] = Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor;
            });

            return output;
        }

        public int[][] GetColorMultivalue(GameDate timestamp, ushort selectedCountry, int[] datakeysColors)
        {
            int[][] output = new int[Data.Defs.Provinces.Count][];
            ushort[] master = Data.GetProvinceMasterVector(timestamp);
            double[,] values;

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
            }

            switch (Data.Multivaluesetting)
            {

                case MultivalueSetting.Unspecified:
                    return output;

                case MultivalueSetting.TopKeyUniform:
                    values = Data.Tables.GetMultivalueTopKeyMatrix("", Table, timestamp);
                    for (int i = 0; i < output.Length; i++)
                    {
                        if (master[i] == 0 && !RenderEmptyProvinces) continue;
                        if (values[0, CountryBased ? master[i] : i] < 0)
                            output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                        else
                            output[i] = new int[] { datakeysColors[(int)values[0, CountryBased ? master[i] : i]] };
                    }
                    return output;

                case MultivalueSetting.TopKeyShaded:
                    values = Data.Tables.GetMultivalueTopKeyMatrix("", Table, timestamp);
                    for (int i = 0; i < output.Length; i++)
                    {
                        if (master[i] == 0 && !RenderEmptyProvinces) continue;
                        if (values[0, CountryBased ? master[i] : i] < 0)
                            output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                        else
                            output[i] = new int[] { Colorscales.CalculateMeanColor(new int[] {
                                datakeysColors[(int)values[0, CountryBased?master[i]:i]], Core.EmptyColor },
                                new double[] { values[1, CountryBased?master[i]:i], values[2, CountryBased?master[i]:i] - values[1, CountryBased?master[i]:i] }) };
                    }
                    return output;

                case MultivalueSetting.AllShaded:
                    values = Data.Tables.GetMultivalueMatrix("", Table, timestamp);
                    List<ushort>[] k = new List<ushort>[CountryBased ? Data.Defs.Countries.Count : Data.Defs.Provinces.Count];
                    List<double>[] v = new List<double>[CountryBased ? Data.Defs.Countries.Count : Data.Defs.Provinces.Count];
                    for (int i = 0; i < values.GetLength(1); i++)
                    {
                        ushort basekey = (ushort)values[0, i];
                        ushort datakey = (ushort)values[1, i];

                        if (k[basekey] == null || v[basekey] == null)
                        {
                            k[basekey] = new List<ushort>();
                            v[basekey] = new List<double>();
                        }

                        k[basekey].Add(datakey);
                        v[basekey].Add(values[2, i]);
                    }

                    for (int i = 0; i < output.Length; i++)
                    {
                        if (master[i] == 0 && !RenderEmptyProvinces) continue;
                        if (k[CountryBased ? master[i] : i] == null || v[CountryBased ? master[i] : i] == null || v[CountryBased ? master[i] : i].Sum() == 0)
                        {
                            output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                        }
                        else
                        {
                            int[] c = new int[k[CountryBased ? master[i] : i].Count];
                            for (int j = 0; j < c.Length; j++)
                                c[j] = datakeysColors[k[CountryBased ? master[i] : i][j]];
                            output[i] = new int[] { Colorscales.CalculateMeanColor(c, v[CountryBased ? master[i] : i].ToArray()) };
                        }
                    }
                    return output;


                case MultivalueSetting.SelectedKeyOnly:
                    if (Data.MultivalueDatakey < 0)
                    {
                        for (int i = 0; i < output.Length; i++)
                        {
                            output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                        }
                        return output;
                    }
                    values = Data.Tables.GetMultivalueSelectedKeyMatrix("", Table, timestamp, (ushort)Data.MultivalueDatakey);

                    double max = double.MinValue;
                    for (int i = 0; i < values.GetLength(1); i++)
                    {
                        if (values[1, i] > max) max = values[1, i];
                    }

                    for (int i = 0; i < output.Length; i++)
                    {
                        if (master[i] == 0 && !RenderEmptyProvinces) continue;
                        if (values[0, CountryBased ? master[i] : i] < 0)
                            output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                        else
                        {
                            if (Data.UseAbsoluteColoring)
                                output[i] = new int[] { Colorscales.CalculateMeanColor(
                                new int[] { datakeysColors[(ushort)Data.MultivalueDatakey], Core.EmptyColor },
                                new double[] { values[1, CountryBased?master[i]:i], max - values[1, CountryBased?master[i]:i] }) };
                            else
                                output[i] = new int[] { Colorscales.CalculateMeanColor(
                                new int[] { datakeysColors[(ushort)Data.MultivalueDatakey], Core.EmptyColor },
                                new double[] { values[1, CountryBased?master[i]:i], values[2, CountryBased?master[i]:i] - values[1, CountryBased?master[i]:i] }) };
                        }
                    }
                    return output;


                default:
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = new int[] { Data.IsWater(i) ? Core.WaterColor : Core.EmptyColor };
                    }
                    return output;
            }
        }

        public string GetValue(GameDate timestamp, ushort country, ushort id, ushort selectedCountry)
        {
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
