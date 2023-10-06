using Ionic.Zip;
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
    /// Represents a table of data, to be pulled out depending on the current data mode.
    /// </summary>
    [Serializable]
    public abstract class Table : IEquatable<Table>, IComparable<Table>
    {
        /// <summary>
        /// Raw data for the table.
        /// </summary>
        [NonSerialized]
        protected Datalist datalist;

        /// <summary>
        /// Timestamp of the data table.
        /// </summary>
        public GameDate Timepoint { get; set; }

        /// <summary>
        /// Name (identifier) of the table.
        /// </summary>
        public string Name { get; protected set; } = "";

        /// <summary>
        /// Name (identifier) of the set.
        /// </summary>
        public string ScriptSet { get; set; } = "";

        /// <summary>
        /// Name (identifier) of the table.
        /// </summary>
        public string ScriptName { get; set; } = "";

        /// <summary>
        /// Amount of items (basekeys) in the table.
        /// </summary>
        public ushort ItemCount { get; private set; } = 0;

        /// <summary>
        /// If true, the table will not be shown in interface.
        /// </summary>
        public bool Hidden { get; set; } = false;

        /// <summary>
        /// Table category.
        /// </summary>
        public string Category { get; protected set; } = "(not set)";

        /// <summary>
        /// Section of a given category, regulates usage of separators in menus.
        /// </summary>
        public int Section { get; protected set; } = 0;

        public TableStatus Status { get; set; } = TableStatus.Unspecified;

        /// <summary>
        /// Name of the table appearing in UX.
        /// </summary>
        public string Caption { get; protected set; } = "(not set)";

        /// <summary>
        /// Useful for setting the parsing order so that some tables may be run only when others are complete. Used for aggregating tables. Must stay in [-10;10] range (inclusive).
        /// </summary>
        public short ParsingOrder { get; set; } = 0;

        /// <summary>
        /// If true, the table will be loaded to cache as soon as possible and will be kept there as long as the flag is true.
        /// </summary>
        public bool ForceCache { get; protected set; } = false;

        /// <summary>
        /// If true, basekeys are country tags, otherwise province indices.
        /// </summary>
        public TableType Type { get; protected set; }

        /// <summary>
        /// If true, the table pulls data from parsed savegame, otherwise from game definitions only.
        /// </summary>
        public bool RequiresSavegame { get; protected set; }

        private bool aggregateValues = false;
        public bool AggregateValues
        {
            get
            {
                return aggregateValues;
            }
            protected set
            {
                aggregateValues = value;
                if (datalist != null)
                {
                    if (this is IMultiTable)
                        datalist.SetConflictResolution(aggregateValues ? ConflictResolution.AddValue : ConflictResolution.AddEntry);
                    else
                        datalist.SetConflictResolution(aggregateValues ? ConflictResolution.AddValue : ConflictResolution.ReplaceValue);
                }
            }
        }

        public double ForcedMin { get; protected set; } = double.NaN;

        public double ForcedMax { get; protected set; } = double.NaN;

        public bool RenderEmptyProvinces { get; set; }

        protected ValueEncoding DatakeyEncoding { get; set; }

        protected ValueEncoding ValueEncoding { get; set; }

        public bool DisplayOnlyForSelectedCountry { get; set; }

        public bool MarkSelectedCountry { get; set; }

        public bool ColorByValue { get; set; }

        public bool RenderWater { get; set; }

        /// <summary>
        /// Path of a file that contains all the table data.
        /// </summary>
        public string CacheFilename
        {
            get
            {
                return Timepoint.GetString("yyyyMMddhh") + "_" + Name;
            }
        }

        public bool Cached
        {
            get
            {
                return datalist != null && datalist.IsCached;
            }
        }

        public long Size
        {
            get
            {
                return datalist == null ? -1 : datalist.Size;
            }
        }

        public Table(string name, TableType type)
        {
            Name = name;
            Hidden = false;
            ParsingOrder = 0;
            ForceCache = false;
            Status = TableStatus.Unspecified;
            Type = type;
            RenderWater = false;
            switch (type)
            {
                case TableType.Country: ItemCount = (ushort)Core.Data.Defs.Countries.Count; break;
                case TableType.Province: ItemCount = (ushort)Core.Data.Defs.Provinces.Count; break;
                case TableType.Special: ItemCount = 1; break;
                default: break;
            }
            RequiresSavegame = true; //default
            Timepoint = GameDate.Empty;
        }

        public static Table CreateFromCache(byte[] raw)
        {
            int len;
            int index = 0;
            string str;

            Table t;
            int type = BitConverter.ToInt32(raw, index); index += 4;

            switch (type)
            {
                case 0: t = new SingleValueTable("", TableType.Special); break;
                case 1: t = new SingleDatakeyTable("", TableType.Special); break;
                case 2: t = new MultiValueTable("", TableType.Special); break;
                case 3: t = new MultiDatakeyTable("", TableType.Special); break;
                default: return null;
            }

            GameDate timepoint = GameDate.FromBytes(raw, index); index += 16;
            t.Timepoint = timepoint;

            len = BitConverter.ToInt32(raw, index); index += 4;
            str = len == 0 ? "" : Encoding.UTF8.GetString(raw, index, len); index += len;
            t.Name = str;

            t.ItemCount = BitConverter.ToUInt16(raw, index); index += 2;
            t.Hidden = BitConverter.ToBoolean(raw, index); index += 1;

            len = BitConverter.ToInt32(raw, index); index += 4;
            str = len == 0 ? "" : Encoding.UTF8.GetString(raw, index, len); index += len;
            t.Category = str;

            t.Section = BitConverter.ToInt32(raw, index); index += 4;

            len = BitConverter.ToInt32(raw, index); index += 4;
            str = len == 0 ? "" : Encoding.UTF8.GetString(raw, index, len); index += len;
            t.Caption = str;

            t.ForceCache = BitConverter.ToBoolean(raw, index); index += 1;
            t.Type = (TableType)BitConverter.ToInt32(raw, index); index += 4;
            t.RequiresSavegame = BitConverter.ToBoolean(raw, index); index += 1;
            t.ForcedMin = BitConverter.ToDouble(raw, index); index += 8;
            t.ForcedMax = BitConverter.ToDouble(raw, index); index += 8;
            t.RenderEmptyProvinces = BitConverter.ToBoolean(raw, index); index += 1;
            t.DatakeyEncoding = (ValueEncoding)BitConverter.ToInt32(raw, index); index += 4;
            t.ValueEncoding = (ValueEncoding)BitConverter.ToInt32(raw, index); index += 4;

            t.DisplayOnlyForSelectedCountry = BitConverter.ToBoolean(raw, index); index += 1;
            t.ColorByValue = BitConverter.ToBoolean(raw, index); index += 1;

            t.Status = TableStatus.Uncached;

            return t;
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (this is SingleValueTable) ms.Write(BitConverter.GetBytes(0), 0, 4);
                else if (this is SingleDatakeyTable) ms.Write(BitConverter.GetBytes(1), 0, 4);
                else if (this is MultiValueTable) ms.Write(BitConverter.GetBytes(2), 0, 4);
                else if (this is MultiDatakeyTable) ms.Write(BitConverter.GetBytes(3), 0, 4);
                else return null;

                byte[] buffer;

                ms.Write(GameDate.ToBytes(Timepoint), 0, 16);

                buffer = Encoding.UTF8.GetBytes(Name);
                ms.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                ms.Write(buffer, 0, buffer.Length);

                ms.Write(BitConverter.GetBytes(ItemCount), 0, 2);
                ms.Write(BitConverter.GetBytes(Hidden), 0, 1);

                buffer = Encoding.UTF8.GetBytes(Category);
                ms.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                ms.Write(buffer, 0, buffer.Length);

                ms.Write(BitConverter.GetBytes(Section), 0, 4);

                buffer = Encoding.UTF8.GetBytes(Caption);
                ms.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                ms.Write(buffer, 0, buffer.Length);

                ms.Write(BitConverter.GetBytes(ForceCache), 0, 1);
                ms.Write(BitConverter.GetBytes((int)Type), 0, 4);
                ms.Write(BitConverter.GetBytes(RequiresSavegame), 0, 1);
                ms.Write(BitConverter.GetBytes(ForcedMin), 0, 8);
                ms.Write(BitConverter.GetBytes(ForcedMax), 0, 8);
                ms.Write(BitConverter.GetBytes(RenderEmptyProvinces), 0, 1);
                ms.Write(BitConverter.GetBytes((int)DatakeyEncoding), 0, 4);
                ms.Write(BitConverter.GetBytes((int)ValueEncoding), 0, 4);
                ms.Write(BitConverter.GetBytes(DisplayOnlyForSelectedCountry), 0, 1);
                ms.Write(BitConverter.GetBytes(ColorByValue), 0, 1);

                return ms.ToArray();
            }
        }

        public void ParseTable(CEParser.File f)
        {
            if (this is IMultiTable)
                datalist = new Datalist(AggregateValues ? ConflictResolution.AddValue : ConflictResolution.AddEntry, ItemCount);
            else
                datalist = new Datalist(AggregateValues ? ConflictResolution.AddValue : ConflictResolution.ReplaceValue, ItemCount);

            try
            {
                Parse(f);
            }
            catch (Exception ex)
            {
                TableScripts.SaveExceptionData(ScriptSet, ScriptName, ex);
            }
            finally
            {
                Status = TableStatus.Parsed;
            }
        }

        public virtual void Parse(CEParser.File f)
        {

        }

        /// <summary>
        /// Unloads the table from RAM.
        /// </summary>
        public void ClearData()
        {
            if (datalist != null)
                datalist.Clear();
            Status = TableStatus.Uncached;
        }

        public void ReadData()
        {
            ReadData(Core.Data.Cache.ReadTableData(CacheFilename));
        }

        public virtual void ReadData(byte[] raw)
        {
            datalist.Read(raw);
            Status = TableStatus.Cached;
        }

        public byte[] WriteData()
        {
            if (!Cached) { throw new Exception("An attempt was made to write table data to disk but the table was not present in RAM."); }

            return datalist.Write();
        }

        public byte[] WriteDefinition()
        {
            return Save();
        }

        #region Colorscales

        public Colorscale GetColorscale(bool? custom)
        {
            if (custom == true)
            {
                if (Core.Data.Tables.CustomColorscales.ContainsKey(Name))
                {
                    return Core.Data.Tables.CustomColorscales[Name];
                }
                else if (Core.Data.Tables.Colorscales.ContainsKey(Name))
                {
                    var cs = Core.Data.Tables.Colorscales[Name];
                    Core.Data.Tables.CustomColorscales.TryAdd(Name, new Colorscale(cs));
                    return cs;
                }
                else
                {
                    var cs = new Colorscale();
                    Core.Data.Tables.Colorscales.TryAdd(Name, cs);
                    Core.Data.Tables.CustomColorscales.TryAdd(Name, cs);
                    return cs;
                }
            }
            else if (custom == null)
            {
                if (Core.Data.Tables.CustomColorscales.ContainsKey(Name))
                {
                    return Core.Data.Tables.CustomColorscales[Name];
                }
                else if (Core.Data.Tables.Colorscales.ContainsKey(Name))
                {
                    var cs = Core.Data.Tables.Colorscales[Name];
                    return cs;
                }
                else
                {
                    var cs = new Colorscale();
                    Core.Data.Tables.Colorscales.TryAdd(Name, cs);
                    return cs;
                }
            }
            else if (custom == false)
            {
                if (Core.Data.Tables.CustomColorscales.ContainsKey(Name))
                {
                    Colorscale c;
                    Core.Data.Tables.CustomColorscales.TryRemove(Name, out c);
                }
                if (Core.Data.Tables.Colorscales.ContainsKey(Name))
                {
                    var cs = Core.Data.Tables.Colorscales[Name];
                    return cs;
                }
                else
                {
                    var cs = new Colorscale();
                    Core.Data.Tables.Colorscales.TryAdd(Name, cs);
                    return cs;
                }
            }
            return null;
        }

        public void ResetColorscale(Data data)
        {
            if (data.Tables.CustomColorscales.ContainsKey(Name))
                data.Tables.CustomColorscales[Name] = new Colorscale(data.Tables.Colorscales[Name]);
            else
                data.Tables.CustomColorscales.TryAdd(Name, data.Tables.Colorscales[Name]);
        }

        void SetColorscale(Colorscale cs)
        {
            if (!Core.Data.Tables.Colorscales.ContainsKey(Name))
                Core.Data.Tables.Colorscales.TryAdd(Name, cs);
            if (Core.Data.Tables.CustomColorscales.ContainsKey(Name))
                Core.Data.Tables.CustomColorscales[Name] = new Colorscale(cs);
        }

        public void SetColorscale(_Color low, _Color mid, _Color high, float exp)
        {
            SetColorscale(new Colorscale(low, mid, high, exp));
        }

        public void SetColorscale(string low, string mid, string high, float exp)
        {
            SetColorscale(GetColor(low), GetColor(mid), GetColor(high), exp);
        }

        public void SetColorscale(_Color low, _Color mid, _Color high)
        {
            SetColorscale(new Colorscale(low, mid, high));
        }

        public void SetColorscale(string low, string mid, string high)
        {
            SetColorscale(GetColor(low), GetColor(mid), GetColor(high));
        }

        public void SetColorscale(_Color low, _Color high, float exp)
        {
            SetColorscale(new Colorscale(low, high, exp));
        }

        public void SetColorscale(string low, string high, float exp)
        {
            SetColorscale(GetColor(low), GetColor(high), exp);
        }

        public void SetColorscale(_Color low, _Color high)
        {
            SetColorscale(new Colorscale(low, high));
        }

        public void SetColorscale(string low, string high)
        {
            SetColorscale(GetColor(low), GetColor(high));
        }

        public void SetColorscale(_Color color, float exp)
        {
            SetColorscale(new Colorscale(color, exp));
        }

        public void SetColorscale(string color, float exp)
        {
            SetColorscale(GetColor(color), exp);
        }

        public void SetColorscale(_Color color)
        {
            SetColorscale(new Colorscale(color));
        }

        public void SetColorscale(string identifier)
        {
            if (Core.Data.Defs.Dynamic.HasDatakeyColors(identifier))
                SetColorscale(new Colorscale(Core.Data.Defs.Dynamic, identifier));
            else
                SetColorscale(GetColor(identifier));
        }

        public void SetColorscale(_Color low, _Color mid, _Color high, float exp, _Color lowW, _Color midW, _Color highW, float expW)
        {
            SetColorscale(new Colorscale(low, mid, high, exp, lowW, midW, highW, expW));
            RenderWater = true;
        }

        public void SetColorscale(string low, string mid, string high, float exp, string lowW, string midW, string highW, float expW)
        {
            SetColorscale(GetColor(low), GetColor(mid), GetColor(high), exp, GetColor(lowW), GetColor(midW), GetColor(highW), expW);
        }

        public void SetColorscale(_Color low, _Color mid, _Color high, _Color lowW, _Color midW, _Color highW)
        {
            SetColorscale(new Colorscale(low, mid, high, lowW, midW, highW));
            RenderWater = true;
        }

        public void SetColorscale(string low, string mid, string high, string lowW, string midW, string highW)
        {
            SetColorscale(GetColor(low), GetColor(mid), GetColor(high), GetColor(lowW), GetColor(midW), GetColor(highW));
        }

        public void SetColorscale(_Color low, _Color high, float exp, _Color lowW, _Color highW, float expW)
        {
            SetColorscale(new Colorscale(low, high, exp, lowW, highW, expW));
            RenderWater = true;
        }

        public void SetColorscale(string low, string high, float exp, string lowW, string highW, float expW)
        {
            SetColorscale(GetColor(low), GetColor(high), exp, GetColor(lowW), GetColor(highW), expW);
        }

        public void SetColorscale(_Color low, _Color high, _Color lowW, _Color highW)
        {
            SetColorscale(new Colorscale(low, high, lowW, highW));
            RenderWater = true;
        }

        public void SetColorscale(string low, string high, string lowW, string highW)
        {
            SetColorscale(GetColor(low), GetColor(high), GetColor(lowW), GetColor(highW));
        }

        public void SetColorscale()
        {
            SetColorscale(new Colorscale());
        }

        public static _Color CreateColor()
        {
            return new _Color(0, 0, 0, 0);
        }

        public static _Color CreateColor(byte r, byte g, byte b)
        {
            return new _Color(255, r, g, b);
        }

        public static _Color CreateColor(Color c)
        {
            return new _Color(255, c.R, c.G, c.B);
        }

        public static Color CreateColor(_Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static _Color GetColor(string id)
        {
            id = id.ToLowerInvariant();
            if (Core.PresetColors.ContainsKey(id))
                return Core.PresetColors[id];
            else
                return CreateColor();
        }

        protected void AddColor(string id, _Color color)
        {
            GetColorscale(false).AddColor(id, color);
        }

        #endregion

        #region Helpers

        protected Table SelectTable(string name)
        {
            return Core.Data.Tables.Select(Timepoint, name);
        }

        protected ushort[] GetProvinceMasters()
        {
            return Core.Data.GetProvinceBasekeys(Timepoint, true);
        }

        protected DynamicDefs DynamicDefs
        {
            get
            {
                return Core.Data.Defs.Dynamic;
            }
        }

        protected ushort TagToIndex(string tag)
        {
            return Core.Data.TagToIndex(tag);
        }

        #endregion

        public abstract int[][] GetColors(MultivalueSetting setting, string datakey, string selectedCountry, MultivalueColorSetting coloring, int[] colors, out double min, out double max, out double[] vals);

        /// <summary>
        /// Returns name of the cache file of the table.
        /// </summary>
        /// <returns>Name of the cache file</returns>
        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(CacheFilename ?? "");
        }

        public bool Equals(Table other)
        {
            return this.CacheFilename == other.CacheFilename;
        }

        public int CompareTo(Table other)
        {
            return this.CacheFilename.CompareTo(other.CacheFilename);
        }
    }

    public class _Color
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public _Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public _Color()
        {
            A = 0;
            R = 0;
            G = 0;
            B = 0;
        }

        public Color GetColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

    public class CacheLoadToken : IDisposable
    {
        Table table;
        TableStatus previousStatus;

        public CacheLoadToken(Table table)
        {
            this.table = table;
            this.previousStatus = table.Status;
            table.Status = TableStatus.InUse;
            if (!table.Cached && previousStatus == TableStatus.Uncached)
            {
                table.ReadData();
            }
        }

        public void Dispose()
        {
            table.Status = previousStatus == TableStatus.Parsed ? TableStatus.Parsed : TableStatus.Cached;
        }
    }

    public interface ISingleTable
    {
        string GetValue(ushort id, ushort country);
    }

    public interface IMultiTable
    {
        IEnumerable<ValueInfo> GetValue(ushort id, ushort country);
    }

    public enum TableType { Special, Province, Country }

    public enum ValueEncoding { Unspecified, Number, Text, Province, Country }

    public enum TableStatus { Unspecified, Parsed, Cached, Uncached, InUse }
}
