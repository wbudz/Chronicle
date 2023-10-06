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
    /// <summary>
    /// Represents a data table where one entry (province or country) can have one value.
    /// </summary>
    [Serializable]
    public class SingleDatakeyTable : Table, ISingleTable
    {
        public SingleDatakeyTable(string name, TableType type) : base(name, type)
        {
            DisplayOnlyForSelectedCountry = false;
            ColorByValue = false;
            AggregateValues = false;
        }

        public override void ReadData(byte[] raw)
        {
            datalist = new Datalist(AggregateValues ? ConflictResolution.AddValue : ConflictResolution.ReplaceValue, ItemCount);
            base.ReadData(raw);
        }

        #region Setters

        public void Set(ushort basekey, string datakey)
        {
            if (datakey == "" || datakey == null) return;
            datalist.Set(basekey, datakey, 1);
        }

        public void Set(string basekey, string datakey)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(basekey) : Ext.ParseUShort(basekey), datakey);
        }

        #endregion

        #region Getters

        public string Get(ushort basekey)
        {
            using (var clt = new CacheLoadToken(this))
            {
                if (datalist == null) return "";
                var result = datalist.Get(basekey);
                return result.GetLength(1) > 0 ? datalist.GetDatakey((ushort)result[0, 0]) : "";
            }
        }

        public string[] GetVector()
        {
            using (var clt = new CacheLoadToken(this))
            {
                if (datalist == null) return new string[ItemCount];
                var result = datalist.Get();
                var strings = datalist.GetAllDatakeys(true);

                string[] output = new string[ItemCount];
                for (int i = 0; i < output.Length; i++)
                {
                    output[i] = "";
                }

                for (int i = 0; i < result.GetLength(1); i++)
                {
                    output[(int)result[0, i]] = strings[(int)result[1, i]];
                }

                return output;
            }
        }

        #endregion

        public override int[][] GetColors(MultivalueSetting setting, string datakey, string selectedCountry, MultivalueColorSetting coloring, int[] colors, out double min, out double max, out double[] vals)
        {
            int[][] output = new int[Core.Data.Defs.Provinces.Count][];
            output[0] = new int[] { colors[2] };
            min = double.NaN;
            max = double.NaN;
            vals = null;

            ushort[] basekeys = Core.Data.GetProvinceBasekeys(Timepoint, Type == TableType.Country);

            string[] stringdata = GetVector();
            bool[] water = Core.Data.Defs.Provinces.WaterProvinces;

            var cs = GetColorscale(null);

            for (int i = 1; i < output.Length; i++)
            {
                output[i] = new int[] { water[i] ? colors[2] : colors[0] };
            }

            for (int i = 1; i < output.Length; i++)
            {
                if (water[i] && !RenderWater) continue;
                if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                output[i][0] = cs.GetColor(water[i], stringdata[basekeys[i]]);
            }

            return output;
        }

        public string GetValue(ushort id, ushort country)
        {
            if ((Type == TableType.Country && country == 0) || id == 0) return "";
            return this.Get(Type == TableType.Country ? country : id);
        }
    }

}
