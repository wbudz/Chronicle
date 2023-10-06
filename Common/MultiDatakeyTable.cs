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
    public class MultiDatakeyTable : Table, IMultiTable
    {
        public MultiDatakeyTable(string name, TableType type) : base(name, type)
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

        #region Setters

        public void Set(ushort basekey, string datakey, double value)
        {
            if (datakey == "" || datakey == null || value == 0) return;
            datalist.Set(basekey, datakey, value);
        }

        public void Set(ushort basekey, ushort datakey, double value)
        {
            if (value == 0) return;
            datalist.Set(basekey, datakey, value);
        }

        public void Set(ushort basekey, string datakey)
        {
            if (datakey == "" || datakey == null) return;
            datalist.Set(basekey, datakey, 1);
        }

        public void Set(ushort basekey, ushort datakey)
        {
            datalist.Set(basekey, datakey, 1);
        }

        public void Set(ushort basekey, string datakey, string value)
        {
            Set(basekey, datakey, Ext.ParseDouble(value));
        }

        public void Set(ushort basekey, ushort datakey, string value)
        {
            Set(basekey, datakey, Ext.ParseDouble(value));
        }

        public void Set(string basekey, ushort datakey, double value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey, value);
        }

        public void Set(string basekey, string datakey, double value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey, value);
        }

        public void Set(string basekey, ushort datakey)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey);
        }

        public void Set(string basekey, string datakey)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey);
        }

        public void Set(string basekey, ushort datakey, string value)
        {
            switch (ValueEncoding)
            {
                case ValueEncoding.Country: Set(basekey, datakey, Core.Data.TagToIndex(value)); break;
                default: Set(basekey, datakey, Ext.ParseDouble(value)); break;
            }
        }

        public void Set(string basekey, string datakey, string value)
        {
            switch (ValueEncoding)
            {
                case ValueEncoding.Country: Set(basekey, datakey, Core.Data.TagToIndex(value)); break;
                default: Set(basekey, datakey, Ext.ParseDouble(value)); break;
            }
        }

        #endregion

        #region Getters

        public double Get(ushort basekey, ushort datakey)
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.Get(basekey, datakey);
        }

        public double[,] Get(ushort basekey)
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.Get(basekey);
        }

        public double[,] Get()
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.Get();
        }

        public double[] GetVector()
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.GetVector();
        }

        public string GetDatakey(ushort identifier)
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.GetDatakey(identifier);
        }

        public string[] GetAllDatakeys(bool includeEmpty)
        {
            using (var clt = new CacheLoadToken(this))
                return datalist.GetAllDatakeys(includeEmpty);
        }

        #endregion

        public override int[][] GetColors(MultivalueSetting setting, string datakey, string selectedCountry, MultivalueColorSetting coloring, int[] colors, out double min, out double max, out double[] vals)
        {
            int[][] output = new int[Core.Data.Defs.Provinces.Count][];
            output[0] = new int[] { colors[2] };

            ushort[] basekeys = Core.Data.GetProvinceBasekeys(Timepoint, Type == TableType.Country);

            double[,] values = Get();
            bool[] water = Core.Data.Defs.Provinces.WaterProvinces;
            double countryIndex = Core.Data.TagToIndex(selectedCountry);

            var cs = GetColorscale(null);

            for (int i = 1; i < output.Length; i++)
            {
                output[i] = new int[] { water[i] ? colors[2] : colors[0] };
            }

            if (Type == TableType.Country)
            {
                int[][] countriesOutput = new int[ItemCount][];

                List<string> keys = new List<string>();
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    if (!DisplayOnlyForSelectedCountry || values[2, i] == countryIndex)
                    {
                        if (ColorByValue)
                        {
                            if (ValueEncoding == ValueEncoding.Text)
                                keys.Add(GetDatakey((ushort)values[2, i]));
                            else if (ValueEncoding == ValueEncoding.Country)
                                keys.Add(Core.Data.Defs.Countries.GetTag((ushort)values[2, i]));
                            else
                                keys.Add(((ushort)values[0, i]).ToString());
                        }
                        else
                        {
                            keys.Add(GetDatakey((ushort)values[1, i]));
                        }
                    }
                    if (i == values.GetLength(1) - 1 || (int)values[0, i] != (int)values[0, i + 1])
                    {
                        var k = keys.Distinct().ToArray();
                        countriesOutput[(int)values[0, i]] = new int[k.Length];
                        for (int j = 0; j < k.Length; j++) { countriesOutput[(int)values[0, i]][j] = cs.GetColor(false, k[j]); }
                        keys.Clear();
                    }
                }

                // Ensure that selected country is properly marked
                if (MarkSelectedCountry && colors[3] != 0 && countryIndex != 0)
                {
                    int idx = (int)countryIndex;
                    if (countriesOutput[idx] == null)
                    {
                        countriesOutput[idx] = new int[1] { colors[3] };
                    }
                    else if (countriesOutput[idx].Length == 1 && countriesOutput[idx][0] == colors[0])
                    {
                        countriesOutput[idx][0] = colors[3];
                    }
                    else
                    {
                        int[] prevOutput = countriesOutput[idx];
                        countriesOutput[idx] = new int[prevOutput.Length + 1];
                        if (prevOutput.Length > 0)
                            Array.Copy(prevOutput, countriesOutput[idx], prevOutput.Length);
                        countriesOutput[idx][prevOutput.Length] = colors[3];
                    }
                }

                // Translate country-indexed colors to province-indexed standard
                for (int i = 1; i < output.Length; i++)
                {
                    if (countriesOutput[basekeys[i]] == null || countriesOutput[basekeys[i]].Length < 1) continue;
                    output[i] = countriesOutput[basekeys[i]];
                }
            }
            else
            {
                List<string> keys = new List<string>();
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    if (water[basekeys[(int)values[0, i]]] && !RenderWater) continue;
                    if (!DisplayOnlyForSelectedCountry || values[2, i] == countryIndex)
                    {
                        if (ColorByValue)
                        {
                            if (ValueEncoding == ValueEncoding.Text)
                                keys.Add(GetDatakey((ushort)values[2, i]));
                            else if (ValueEncoding == ValueEncoding.Country)
                                keys.Add(Core.Data.Defs.Countries.GetTag((ushort)values[2, i]));
                            else
                                keys.Add(((ushort)values[2, i]).ToString());
                        }
                        else
                        {
                            keys.Add(GetDatakey((ushort)values[1, i]));
                        }
                    }
                    if (keys.Count>0 && (i == values.GetLength(1) - 1 || (int)values[0, i] != (int)values[0, i + 1]))
                    {
                        var k = keys.Distinct().ToArray();
                        output[basekeys[(int)values[0, i]]] = new int[k.Length];
                        for (int j = 0; j < k.Length; j++) { output[basekeys[(int)values[0, i]]][j] = cs.GetColor(false, k[j]); }
                        keys.Clear();
                    }
                }
            }

            min = double.NaN;
            max = double.NaN;
            vals = null;
            return output;
        }

        public IEnumerable<ValueInfo> GetValue(ushort id, ushort country)
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            double[,] values;
            string[] datakeys = GetAllDatakeys(true);

            if ((Type == TableType.Country && country == 0) || id == 0) yield break;
            values = Get(Type == TableType.Country ? country : id);
            for (int i = 0; i < values.GetLength(1); i++)
            {
                switch (ValueEncoding)
                {
                    case ValueEncoding.Text: yield return new ValueInfo(datakeys[(int)values[0, i]], datakeys[(int)values[1, i]]); break;
                    case ValueEncoding.Province: yield return new ValueInfo(datakeys[(int)values[0, i]], Core.Data.Defs.Provinces.GetName((ushort)values[1, i], "({0}) {1}")); break;
                    case ValueEncoding.Country: yield return new ValueInfo(datakeys[(int)values[0, i]], Core.Data.Defs.Countries.GetName((ushort)values[1, i], "({0}) {1}")); break;
                    default: yield return new ValueInfo(datakeys[(int)values[0, i]], values[1, i].ToString("#,0.##", nfi)); break;
                }
            }
        }
    }
}
