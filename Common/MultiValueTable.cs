using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace Chronicle
{
    [Serializable]
    public class MultiValueTable : Table, IMultiTable
    {
        public MultiValueTable(string name, TableType type) : base(name, type)
        {
            DisplayOnlyForSelectedCountry = false;
            ColorByValue = false;
            AggregateValues = false;
        }

        public override void ReadData(byte[] raw)
        {
            datalist = new Datalist(AggregateValues ? ConflictResolution.AddValue : ConflictResolution.AddEntry, ItemCount);
            base.ReadData(raw);
        }

        protected void Aggregate(string name)
        {
            if (ParsingOrder < 3)
            {
                ParsingOrder = 3;
                return;
            }
            AggregateValues = true;
            Table table = SelectTable(name);
            if (table is MultiValueTable)
            {
                MultiValueTable t = SelectTable(name) as MultiValueTable;
                if (t == null) return;
                ushort[] master = GetProvinceMasters();
                double[,] data = t.Get();
                string[] strings = t.GetAllDatakeys(true);
                for (int i = 0; i < data.GetLength(1); i++)
                {
                    Set(master[(int)data[0, i]], strings[(ushort)data[1, i]], data[2, i]);
                }
            }
            else if (table is MultiDatakeyTable)
            {
                MultiDatakeyTable t = SelectTable(name) as MultiDatakeyTable;
                if (t == null) return;
                ushort[] master = GetProvinceMasters();
                double[,] data = t.Get();
                string[] strings = t.GetAllDatakeys(true);
                for (int i = 0; i < data.GetLength(1); i++)
                {
                    Set(master[(int)data[0, i]], strings[(ushort)data[1, i]], 1);
                }
            }
            else if (table is SingleDatakeyTable)
            {
                SingleDatakeyTable t = SelectTable(name) as SingleDatakeyTable;
                if (t == null) return;
                ushort[] master = GetProvinceMasters();
                string[] data = t.GetVector();
                for (int i = 0; i < data.Length; i++)
                {
                    Set(master[i], data[i], 1);
                }
            }
        }

        #region Setters

        public void Set(ushort basekey, ushort datakey, string value)
        {
            Set(basekey, datakey, Ext.ParseDouble(value));
        }

        public void Set(ushort basekey, ushort datakey, double value)
        {
            if (value == 0) return;
            datalist.Set(basekey, datakey, value);
        }

        public void Set(ushort basekey, string text, string value)
        {
            Set(basekey, text, Ext.ParseDouble(value));
        }

        public void Set(ushort basekey, string text, double value)
        {
            if (text == "" || text == null || value == 0) return;
            datalist.Set(basekey, text, value);
        }

        public void Set(ushort basekey, string text)
        {
            Set(basekey, text, 1);
        }

        public void Set(string basekey, ushort datakey, string value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey, Ext.ParseDouble(value));
        }

        public void Set(string basekey, ushort datakey, double value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey, value);
        }

        public void Set(string identifier, string text, string value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(identifier) : Ext.ParseUShort(identifier), text, Ext.ParseDouble(value));
        }

        public void Set(string basekey, string text, double value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), text, value);
        }

        #endregion

        #region Getters

        public double Get(ushort basekey, ushort datakey)
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.Get(basekey, datakey)) ?? 0;
        }

        public double Get(ushort basekey, string datakey)
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.Get(basekey, datakey)) ?? 0;
        }

        public double[,] Get(ushort basekey)
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.Get(basekey)) ?? new double[ItemCount, 0];
        }

        public double[,] Get()
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.Get()) ?? new double[ItemCount, 0];
        }

        public double[] GetVector()
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.GetVector()) ?? new double[ItemCount];
        }

        public string[] GetAllDatakeys(bool includeEmpty)
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.GetAllDatakeys(includeEmpty)) ?? new string[ItemCount];
        }

        #endregion

        public override int[][] GetColors(MultivalueSetting setting, string datakey, string selectedCountry, MultivalueColorSetting coloring, int[] colors, out double min, out double max, out double[] vals)
        {
            // Initialize variables
            int[][] output = new int[Core.Data.Defs.Provinces.Count][];
            min = double.NaN;
            max = double.NaN;
            vals = null;

            // Initialize additional data
            ushort[] basekeys = Core.Data.GetProvinceBasekeys(Timepoint, Type == TableType.Country);
            bool[] water = Core.Data.Defs.Provinces.WaterProvinces;
            string[] datakeys = GetAllDatakeys(true);
            var cs = GetColorscale(null);
            double[,] values = null;

            // Set initial values
            output[0] = new int[] { colors[2] };
            for (int i = 1; i < output.Length; i++) { output[i] = new int[] { water[i] ? colors[2] : colors[0] }; }

            switch (setting)
            {
                case MultivalueSetting.TopKeyUniform:
                    values = GetTopKeyMatrix(); // 0 - top datakey, 1 - top datakey value, 2 - all datakeys values sum
                    for (int i = 1; i < output.Length; i++)
                    {
                        if (water[i] && !RenderWater) continue;
                        if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                        datakey = datakeys[(int)Math.Max(values[0, basekeys[i]], 0)];
                        output[i][0] = cs.GetColor(water[i], 1, 0, 1, datakey);
                    }
                    break;

                case MultivalueSetting.TopKeyShaded:
                    values = GetTopKeyMatrix(); // 0 - top datakey, 1 - top datakey value, 2 - all datakeys values sum
                    for (int i = 1; i < output.Length; i++)
                    {
                        if (water[i] && !RenderWater) continue;
                        if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                        datakey = datakeys[(int)Math.Max(values[0, basekeys[i]], 0)];
                        output[i][0] = Colorscale.CalculateMeanColor(
                                cs.GetColor(water[i], 1, 0, 1, datakey),
                                values[1, basekeys[i]],
                                colors[0],
                                values[2, basekeys[i]] - values[1, basekeys[i]]);
                    }
                    break;

                case MultivalueSetting.AllShaded:
                    values = Get(); // 0 - basekey, 1 - datakey, 2 - value
                    List<ushort>[] k = new List<ushort>[output.Length];
                    List<double>[] v = new List<double>[output.Length];

                    for (int i = 0; i < output.Length; i++)
                    {
                        k[i] = new List<ushort>();
                        v[i] = new List<double>();
                    }

                    for (int i = 0; i < values.GetLength(1); i++)
                    {
                        k[(int)values[0, i]].Add((ushort)values[1, i]);
                        v[(int)values[0, i]].Add(values[2, i]);
                    }

                    for (int i = 1; i < output.Length; i++)
                    {
                        if (water[i] && !RenderWater) continue;
                        if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                        
                        if (v[basekeys[i]].Count < 1) continue;

                        int[] c = new int[k[basekeys[i]].Count];
                        for (int j = 0; j < c.Length; j++)
                        {
                            c[j] = cs.GetColor(water[i], 1, 0, 1, datakeys[k[basekeys[i]][j]]);
                        }
                        output[i][0] = Colorscale.CalculateMeanColor(c, v[basekeys[i]].ToArray());
                    }
                    break;

                case MultivalueSetting.SelectedKeyOnly:

                    var selkeyColor = colors[4] == 0 ? cs.GetColor(false, 1, 0, 1, datakey) : colors[4];
                    values = Get();
                    int datakeyidx = Array.FindIndex(datakeys, x => x == datakey);

                    // 3 options:
                    // Absolute world: given datakey amount vs the biggest amount in the world of any datakey
                    // Absolute key: given datakey amount vs the biggest amount in the world of this datakey
                    // Relative province: given datakey amount vs sum of amounts in this province

                    if (coloring == MultivalueColorSetting.AbsoluteWorld)
                    {
                        double[] keyvals = new double[output.Length];
                        double totval = 0;

                        for (int i = 0; i < values.GetLength(1); i++)
                        {
                            if (values[1, i] == datakeyidx) keyvals[(int)values[0, i]] = values[2, i];
                            totval = Math.Max(totval, values[2, i]);
                        }

                        for (int i = 1; i < output.Length; i++)
                        {
                            if (water[i] && !RenderWater) continue;
                            if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                            output[i][0] = Colorscale.CalculateMeanColor(selkeyColor, keyvals[basekeys[i]], colors[0], totval - keyvals[basekeys[i]]);
                        }
                    }
                    else if (coloring == MultivalueColorSetting.AbsoluteKey)
                    {
                        double[] keyvals = new double[output.Length];
                        double totval = 0;

                        for (int i = 0; i < values.GetLength(1); i++)
                        {
                            if (values[1, i] == datakeyidx) keyvals[(int)values[0, i]] = values[2, i];
                            if (values[1, i] == datakeyidx) totval = Math.Max(totval, values[2, i]);
                        }

                        for (int i = 1; i < output.Length; i++)
                        {
                            if (water[i] && !RenderWater) continue;
                            if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                            output[i][0] = Colorscale.CalculateMeanColor(selkeyColor, keyvals[basekeys[i]], colors[0], totval - keyvals[basekeys[i]]);
                        }
                    }
                    if (coloring == MultivalueColorSetting.RelativeProvince)
                    {

                        double[] keyvals = new double[output.Length];
                        double[] totvals = new double[output.Length];

                        for (int i = 0; i < values.GetLength(1); i++)
                        {
                            if (values[1, i] == datakeyidx) keyvals[(int)values[0, i]] = values[2, i];
                            totvals[(int)values[0, i]] += values[2, i];
                        }

                        for (int i = 1; i < output.Length; i++)
                        {
                            if (water[i] && !RenderWater) continue;
                            if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                            output[i][0] = Colorscale.CalculateMeanColor(selkeyColor, keyvals[basekeys[i]], colors[0], totvals[basekeys[i]] - keyvals[basekeys[i]]);
                        }
                    }

                    break;
            }

            return output.ToArray();
        }

        public IEnumerable<ValueInfo> GetValue(ushort id, ushort country)
        {
            double[,] values;
            string[] datakeyNames = GetAllDatakeys(true);

            if ((Type == TableType.Country && country == 0) || id == 0) yield break;
            values = Get(Type == TableType.Country ? country : id);
            for (int i = 0; i < values.GetLength(1); i++)
            {
                string text = values[1, i].ToString("N" + Core.Settings.TableDisplayPrecision);
                if ((int)values[0, i] >= datakeyNames.Length) continue;
                yield return new ValueInfo(datakeyNames[(int)values[0, i]], text);
            }
        }

        double[,] GetTopKeyMatrix()
        {
            double[,] data = Get();

            double[,] output = new double[3, ItemCount];
            for (int i = 0; i < output.GetLength(1); i++)
            {
                output[0, i] = -1; // datakey
                output[1, i] = 0; // value
                output[2, i] = 0; // sum
            }
            for (int i = 0; i < data.GetLength(1); i++)
            {
                int index = (int)data[0, i];
                if (data[2, i] > output[1, index])
                {
                    output[0, index] = data[1, i]; // datakey
                    output[1, index] = data[2, i]; // value
                }
                output[2, index] += data[2, i];
            }
            return output;
        }

        double[,] GetSelectedKeyMatrix(string datakey)
        {
            int datakeyidx = Array.FindIndex(GetAllDatakeys(true), x => x == datakey);
            double[,] data = Get();

            double[,] output = new double[3, ItemCount];
            for (int i = 0; i < output.GetLength(1); i++)
            {
                output[0, i] = -1; // datakey
                output[1, i] = 0; // value
                output[2, i] = 0; // sum
            }
            for (int i = 0; i < data.GetLength(1); i++)
            {
                int index = (int)data[0, i];
                if (data[1, i] == datakeyidx)
                {
                    output[0, index] = datakeyidx; // datakey                    
                    output[1, index] = data[2, i]; // value
                }
                output[2, index] += data[2, i];
            }
            return output;
        }
    }

    public enum MultivalueSetting { Unspecified, AllShaded, TopKeyUniform, TopKeyShaded, SelectedKeyOnly };
    public enum MultivalueColorSetting { Unspecified, AbsoluteWorld, AbsoluteKey, RelativeProvince };

    public class ValueInfo
    {
        public string Datakey { get; set; }
        public string Value { get; set; }

        public ValueInfo(string datakey, string value)
        {
            Datakey = datakey;
            Value = value;
        }
    }
}
