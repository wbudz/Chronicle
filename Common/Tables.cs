using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEParser;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;

namespace Chronicle
{
    /// <summary>
    /// Represent set of data tables that may be managed together.
    /// </summary>
    [Serializable]
    public partial class Tables : MarshalByRefObject
    {
        /// <summary>
        /// Contains all the tables.
        /// </summary>
        protected List<Table> tables = new List<Table>();

        List<GameDate> timepoints = new List<GameDate>();

        Dictionary<GameDate, ushort[]> masterVectors = new Dictionary<GameDate, ushort[]>();

        public ConcurrentDictionary<string, Colorscale> Colorscales
        {
            get;
            set;
        }

        public ConcurrentDictionary<string, Colorscale> CustomColorscales
        {
            get;
            set;
        }

        /// <summary>
        /// Returns total amount of tables.
        /// </summary>
        public int Count
        {
            get
            {
                return tables.Count;
            }
        }

        public int CacheCount
        {
            get
            {
                return tables.Count(x => x.Status == TableStatus.Cached || x.Status == TableStatus.InUse || x.Status == TableStatus.Parsed);
            }
        }

        public float CacheSize
        {
            get
            {
                return tables.FindAll(x => x.Status == TableStatus.Cached || x.Status == TableStatus.InUse || x.Status == TableStatus.Parsed).Sum(x => x.Size) / 1000000f;
            }
        }

        public Tables()
        {
            Colorscales = new ConcurrentDictionary<string, Colorscale>();
            CustomColorscales = new ConcurrentDictionary<string, Colorscale>();
        }

        public Colorscale GetColorscale(string name)
        {
            if (Core.Data.Tables.CustomColorscales.ContainsKey(name))
                return Core.Data.Tables.CustomColorscales[name];

            var cs = new Colorscale(Core.Data.Tables.Colorscales[name]);
            Core.Data.Tables.CustomColorscales.TryAdd(name, cs);
            return cs;
        }

        public Colorscale ResetColorscale(string name)
        {
            if (Core.Data.Tables.Colorscales[name].IsRandom)
                Core.Data.Tables.CustomColorscales[name] = new Colorscale();
            else
                Core.Data.Tables.CustomColorscales[name] = new Colorscale(Core.Data.Tables.Colorscales[name]);
            return Core.Data.Tables.CustomColorscales[name];
        }

        /// <summary>
        /// Select all the tables satisfying the given condition regarding timepoint.
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        /// <returns>Array of tables</returns>
        public List<Table> SelectByDate(GameDate timepoint)
        {
            List<Table> output = tables.FindAll(x => x.Timepoint == timepoint);
            return (output == null) ? (new List<Table>()) : (output.ToList());
        }

        /// <summary>
        /// Select all the tables satisfying the given condition regarding label.
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>Array of tables</returns>
        public List<Table> SelectByName(string name)
        {
            List<Table> output = tables.FindAll(x => x.Name == name);
            return (output == null) ? (new List<Table>()) : (output.ToList());
        }

        /// <summary>
        /// Select the table (first occurence) satisfying the given condition regarding timepoint and label.
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        /// <param name="name">Table name</param>
        /// <returns>Table</returns>
        public Table Select(GameDate timepoint, string name)
        {
            return tables.Find(x => (x.Timepoint == timepoint || x.Timepoint.IsEmpty) && x.Name == name);
        }

        /// <summary>
        /// Saves all the tables to the disk.
        /// </summary>
        public void SaveToCache()
        {
            Core.Dispatch.DisplayProgress("Saving tables to cache...");
            try
            {
                WriteTableDefinitions();
                WriteTableData();
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        public void PrepareNodesets(InstalledGame game, CEParser.File file)
        {
            file.AddNodeset("countries", Tables.CreateCountriesNodelist(file, game.Game.Token));
            file.AddNodeset("provinces", Tables.CreateProvincesNodelist(file, game.Game.Token));
            file.AddNodeset("titleholders", Tables.CreateTitleholdersNodelist(file, game.Game.Token));
            file.AddNodeset("baronies", Tables.CreateBaroniesNodelist(file, game.Game.Token));
        }

        public void ParseTables()
        {
            List<Table> allTables = SelectByDate(GameDate.Empty);

            int completedTables = 0;
            float totalTables = allTables.Count;

            try
            {
                // Traverse through parsing order
                for (int i = -3; i <= 3; i++)
                {
                    var tables = allTables.FindAll(x => x.ParsingOrder == i).ToList();
                    Parallel.ForEach(tables, x =>
                    {
                        Core.Dispatch.DisplayProgress("Parsing tables...", (float)completedTables / totalTables);
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        x.ParseTable(null);
                        sw.Stop();
                        TableScripts.SavePerformanceData(x.ScriptSet, x.ScriptName, sw.ElapsedMilliseconds);
                        Interlocked.Increment(ref completedTables);
                    });
                }

            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        /// <summary>
        /// Parses the given node through all the data tables of given timepoint.
        /// </summary>
        /// <param name="file">Parsed file which should be analyzed</param>
        /// <param name="timepoint">Specifies timepoint of which tables should be parsed</param>
        public void ParseTables(CEParser.File file, GameDate timepoint)
        {
            List<Table> allTables = SelectByDate(timepoint);

            int completedTables = 0;
            float totalTables = allTables.Count;

            // Parse master table first
            var masterTable = allTables.Find(x => x.Name == "Master") as SingleValueTable;
            if (masterTable != null)
            {
                masterTable.ParseTable(file);
                masterVectors.Add(timepoint, masterTable.GetVector().Select(x => (ushort)x).ToArray());
            }
            else
            {
                masterVectors.Add(timepoint, new ushort[Core.Data.Defs.Provinces.Count]);
            }

            // Traverse through parsing order
            for (int i = -3; i <= 3; i++)
            {
                var tables = allTables.FindAll(x => x.ParsingOrder == i).ToList();
                Parallel.ForEach(tables, x =>
                {
                    Core.Dispatch.DisplayProgress("Parsing tables...", (float)completedTables / totalTables);
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    x.ParseTable(file);
                    sw.Stop();
                    TableScripts.SavePerformanceData(x.ScriptSet, x.ScriptName, sw.ElapsedMilliseconds);
                    Interlocked.Increment(ref completedTables);
                });
            }
        }

        /// <summary>
        /// Removes the table contents from RAM (they will remain on disk).
        /// </summary>
        public void ClearData()
        {
            foreach (Table table in tables)
            {
                table.ClearData();
            }
        }

        private IEnumerable<ushort> GetNonExistentCountriesIDs()
        {
            List<ushort> countries = new List<ushort>();
            for (int i = 0; i < GetTimepointsCount(); i++)
            {
                countries.AddRange(Core.Data.GetProvinceBasekeys(GetTimepoint(i), true));
            }
            countries = countries.Distinct().ToList();

            for (ushort i = 0; i < Core.Data.Defs.Countries.Count; i++)
            {
                if (!countries.Contains(i)) yield return i;
            }
        }

        public List<string> GetTimepointStrings(string format)
        {
            return timepoints.Select(x => x.GetString(format)).ToList();
        }

        public GameDate GetTimepoint(int index)
        {
            if (timepoints == null || index >= timepoints.Count || timepoints.Count == 0) return GameDate.Empty;
            try
            {
                if (index < 0) return timepoints.Last();
                return timepoints[index];
            }
            catch
            {
                return GameDate.Empty;
            }
        }

        public List<GameDate> GetTimepoints()
        {
            return timepoints;
        }

        public int GetTimepointsCount()
        {
            return timepoints.Count;
        }

        /// <summary>
        /// Returns list of table captions of the given table type.
        /// </summary>
        /// <param name="tableType">Table type</param>
        /// <returns>List of table captions</returns>
        public List<string> GetTablesCaptions(Type tableType)
        {
            return tables.FindAll(x => x.GetType() == tableType).Select(x => x.Caption).Distinct().ToList();
        }

        public void CreateTables(string token)
        {
            if (!tables.Exists(x => x is DefaultTable))
                tables.Add(new DefaultTable());

            if (token == "hoi3tfh") token = "hoi3"; // important exception
            foreach (var s in TableScripts.Get(token))
            {
                try
                {
                    var t = s.CreateTable();
                    if (t.RequiresSavegame) continue;
                    if (!tables.Contains(t)) tables.Add(t);
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Dynamic table initialization error.", ex);
                }
            }
        }
        
        public void CreateTables(GameDate timepoint, string token)
        {
            // Create master table
            var masterTable = new MasterTable()
            {
                ParsingOrder = -3
            };
            masterTable.Timepoint = timepoint;
            if (!tables.Contains(masterTable))
                tables.Add(masterTable);

            // Import external tables
            
            if (token == "hoi3tfh") token = "hoi3"; // important exception
            foreach (var s in TableScripts.Get(token))
            {
                try
                {
                    var t = s.CreateTable();
                    if (!t.RequiresSavegame) continue;
                    t.Timepoint = timepoint;
                    if (!tables.Contains(t)) tables.Add(t);
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Dynamic table initialization error.", ex);
                }
            }
        }

        public void AddTimepoint(GameDate timepoint)
        {
            if (timepoint.IsEmpty) return;
            if (timepoints.Contains(timepoint)) return;
            timepoints.Add(timepoint);
            timepoints.Sort();
        }

        public bool HasTimepoint(GameDate? currentTimepoint)
        {
            if (currentTimepoint == null) return false;
            return timepoints.Contains(currentTimepoint.Value);
        }

        /// <summary>
        /// Remove all the tables pertaining to the given timepoint.
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        public void RemoveTables(GameDate timepoint)
        {
            tables.RemoveAll(x => x.Timepoint == timepoint);
            timepoints.Remove(timepoint);
            masterVectors.Remove(timepoint);
        }

        private void AddTable(Table table)
        {
            if (!table.Timepoint.IsEmpty && !timepoints.Contains(table.Timepoint))
            {
                AddTimepoint(table.Timepoint);
            }
            tables.Add(table);
        }

        public Table[] GetTables()
        {
            return tables.ToArray();
        }

        public ushort[] GetMasterVector(GameDate timepoint)
        {
            ushort[] output;
            if (timepoint.IsEmpty) return new ushort[Core.Data.Defs.Provinces.Count];
            masterVectors.TryGetValue(timepoint, out output);
            return output ?? new ushort[Core.Data.Defs.Provinces.Count];
        }

        static Dictionary<ushort, CEParser.Node> CreateCountriesNodelist(CEParser.File f, string token)
        {
            var nodelist = new Dictionary<ushort, CEParser.Node>();
            if (token == "eu4")
            {
                foreach (var n in f.GetSubnodes(f.Root, "countries", "*"))
                {
                    ushort id = Core.Data.TagToIndex(f.GetNodeName(n));
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            else if (token == "ck2")
            {
                foreach (var n in f.GetSubnodes(f.Root, "title", "*"))
                {
                    ushort id = Core.Data.TagToIndex(f.GetNodeName(n));
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            else
            {
                foreach (var n in f.GetSubnodes(f.Root))
                {
                    ushort id = Core.Data.TagToIndex(f.GetNodeName(n));
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            return nodelist;
        }

        static Dictionary<ushort, CEParser.Node> CreateProvincesNodelist(CEParser.File f, string token)
        {
            // Provinces
            var nodelist = new Dictionary<ushort, CEParser.Node>();
            if (token == "eu4")
            {
                foreach (var n in f.GetSubnodes(f.Root, "provinces", "*"))
                {
                    ushort id;
                    UInt16.TryParse(f.GetNodeName(n).Remove(0, 1), out id);
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            else if (token == "ck2")
            {
                foreach (var n in f.GetSubnodes(f.Root, "provinces", "*"))
                {
                    ushort id;
                    UInt16.TryParse(f.GetNodeName(n), out id);
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            else
            {
                foreach (var n in f.GetSubnodes(f.Root))
                {
                    ushort id;
                    UInt16.TryParse(f.GetNodeName(n), out id);
                    if (id > 0) nodelist.Add(id, n);
                }
            }
            return nodelist;
        }

        static Dictionary<ushort, CEParser.Node> CreateTitleholdersNodelist(CEParser.File f, string token)
        {
            var nodelist = new Dictionary<ushort, CEParser.Node>();
            if (token == "ck2")
            {
                foreach (var n in f.GetNodeset("countries"))
                {
                    string holderid = f.GetAttributeValue(n.Value, "holder");
                    nodelist.Add(n.Key, f.GetSubnode(f.Root, "character", holderid));
                }
            }
            else
            {
            }
            return nodelist;
        }

        static Dictionary<ushort, CEParser.Node> CreateBaroniesNodelist(CEParser.File f, string token)
        {
            var nodelist = new Dictionary<ushort, CEParser.Node>();
            if (token == "ck2")
            {
                foreach (var n in f.GetNodeset("provinces"))
                {
                    nodelist.Add(n.Key, f.GetSubnodes(n.Value).Find(x => f.GetNodeName(x).StartsWith("b_")));
                }
            }
            else
            {
            }
            return nodelist;
        }

        public void RecreateTimepointData()
        {
            timepoints.Clear();
            masterVectors.Clear();
            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].Timepoint.IsEmpty) continue;
                if (!timepoints.Contains(tables[i].Timepoint))
                    timepoints.Add(tables[i].Timepoint);
                if (tables[i].Name == "Master" && !masterVectors.ContainsKey(tables[i].Timepoint))
                {
                    var mastertable = (tables[i] as SingleValueTable).GetVector();
                    masterVectors.Add(tables[i].Timepoint, mastertable.Select(x => (ushort)x).ToArray());
                }
            }
            timepoints.Sort();
        }

        public List<KeyValuePair<string, string>> GetStatusReport()
        {
            List<KeyValuePair<string, string>> output = new List<KeyValuePair<string, string>>();
            foreach (var t in tables)
            {
                output.Add(new KeyValuePair<string, string>(t.CacheFilename, t.Cached + ", " + t.Status));
            }
            return output;
        }

        #region Cache management

        public void ReadTableDefinitions()
        {
            // Read tables definitions
            var tablesDefs = Core.Data.Cache.ReadTableDefinitions();
            foreach (var t in tablesDefs)
            {
                Table newTable = Table.CreateFromCache(t);
                if (Select(newTable.Timepoint, newTable.Name) != null) continue; // prevent duplicates
                newTable.Status = TableStatus.Uncached; // if we read definition from the cache, it is safe to set this status
                AddTable(newTable);
            }

            // Read colorscales
            using (MemoryStream stream = new MemoryStream(Core.Data.Cache.ReadColorscales()))
            {
                BinaryFormatter bin = new BinaryFormatter();
                Colorscales = (ConcurrentDictionary<string, Colorscale>)bin.Deserialize(stream);
            }
            CustomColorscales = new ConcurrentDictionary<string, Colorscale>();
        }

        public void WriteTableDefinitions()
        {
            // Write tables definitions
            Core.Data.Cache.WriteTableDefinitions(tables);

            // Write colorscales
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, Colorscales);
                Core.Data.Cache.WriteColorscales(stream.ToArray());
            }
        }

        public void ReadTableData()
        {
            ReadTableData(tables);
        }

        public void ReadTableData(IEnumerable<Table> tables)
        {
            foreach (var t in tables)
            {
                t.ReadData(Core.Data.Cache.ReadTableData(t.CacheFilename));
            }
        }

        public void WriteTableData()
        {
            WriteTableData(tables);
        }

        public void WriteTableData(IEnumerable<Table> tables)
        {
            Core.Data.Cache.WriteTableData(tables);
        }

        public void ClearTableData(IEnumerable<Table> tables)
        {
            foreach (var t in tables)
            {
                t.ClearData();
            }
        }

        public void CollectCache()
        {
            if (Core.UI_Mapview.CurrentTimepoint.IsEmpty || Core.UI_Mapview.CurrentTable == null) return;

            List<Table> cacheTasks = new List<Table>();
            List<Table> uncacheTasks = new List<Table>();

            for (int i = 0; i < tables.Count; i++)
            {
                switch (tables[i].Status)
                {
                    case TableStatus.Parsed:
                        continue;
                    case TableStatus.Cached:
                        if (!(tables[i].Timepoint == Core.UI_Mapview.CurrentTimepoint || Core.UI_Mapview.CurrentTable.Name == tables[i].Name || tables[i].ForceCache))
                            uncacheTasks.Add(tables[i]);
                        break;
                    case TableStatus.Uncached:
                        if (tables[i].Timepoint == Core.UI_Mapview.CurrentTimepoint || Core.UI_Mapview.CurrentTable.Name == tables[i].Name || tables[i].ForceCache)
                            cacheTasks.Add(tables[i]);
                        break;
                    case TableStatus.InUse:
                        continue;
                }
            }

            if (Core.CacheCollectionPaused) return;

            ClearTableData(uncacheTasks.ToArray());
            ReadTableData(cacheTasks.ToArray());

            // Refresh cache information
            if (Core.Data.Tables != null && (cacheTasks.Count != 0 || uncacheTasks.Count != 0))
            {
                Core.Dispatch.Run(() => Core.UI_Debug.RefreshCacheStats(Core.Data.Tables.CacheCount, Core.Data.Tables.Count, Core.Data.Tables.CacheSize, cacheTasks.Count, uncacheTasks.Count));
            }
        }

        #endregion
    }
}
