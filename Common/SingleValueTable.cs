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
    public class SingleValueTable : Table, ISingleTable
    {
        public SingleValueTable(string name, TableType type) : base(name, type)
        {
            DisplayOnlyForSelectedCountry = false;
            ColorByValue = true;
            AggregateValues = false;
        }

        public override void ReadData(byte[] raw)
        {
            datalist = new Datalist(AggregateValues ? ConflictResolution.AddValue : ConflictResolution.ReplaceValue, ItemCount);
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
            SingleValueTable t = SelectTable(name) as SingleValueTable;
            if (t == null) return;
            ushort[] master = GetProvinceMasters();
            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }

        #region Setters

        static double ParseDouble(string input)
        {
            double value;
            Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return value;
        }

        static ushort ParseUShort(string input)
        {
            ushort value;
            UInt16.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            return value;
        }

        public void Set(ushort identifier, string value)
        {
            Set(identifier, ParseDouble(value));
        }

        public void Set(ushort identifier, double value)
        {
            if (identifier == 0 || value == 0) return;
            if (Type == TableType.Country && Core.Data.IsSpecialTag(identifier)) return;
            datalist.Set(identifier, 0, value);
        }

        public void Set(string identifier, string value)
        {
            Set(identifier, ParseDouble(value));
        }

        public void Set(string identifier, double value)
        {
            Set(Type == TableType.Country ? Core.Data.TagToIndex(identifier) : ParseUShort(identifier), value);
        }

        #endregion

        #region Getters

        public double Get(ushort identifier)
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.Get(identifier, 0)) ?? 0;
        }

        public double[] GetVector()
        {
            using (var clt = new CacheLoadToken(this))
                return (datalist?.GetVector()) ?? new double[ItemCount];
        }

        #endregion

        public override int[][] GetColors(MultivalueSetting setting, string datakey, string selectedCountry, MultivalueColorSetting coloring, int[] colors, out double min, out double max, out double[] vals)
        {
            int[][] output = new int[Core.Data.Defs.Provinces.Count][];
            output[0] = new int[] { colors[2] };

            ushort[] basekeys = Core.Data.GetProvinceBasekeys(Timepoint, Type == TableType.Country);

            double[] numdata = GetVector();
            bool[] water = Core.Data.Defs.Provinces.WaterProvinces;
            min = double.IsNaN(ForcedMin) ? numdata.Min() : ForcedMin;
            max = double.IsNaN(ForcedMax) ? numdata.Max() : ForcedMax;
            vals = new double[Core.Data.Defs.Provinces.Count];
            Array.Copy(numdata, vals, Math.Min(numdata.Length, vals.Length));

            var cs = GetColorscale(null);

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = new int[] { water[i] ? colors[2] : colors[0] };
            }

            for (int i = 0; i < output.Length; i++)
            {
                if (water[i] && !RenderWater) continue;
                if (basekeys[i] == 0 && !RenderEmptyProvinces) continue;
                output[i][0] = cs.GetColor(water[i], numdata[basekeys[i]], min, max);
            }

            return output;
        }

        public string GetValue(ushort id, ushort country)
        {
            if ((Type == TableType.Country && country == 0) || id == 0) return "";
            double data = Get(Type == TableType.Country ? country : id);
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return data.ToString("#,0.##", nfi);
        }

    }

}
