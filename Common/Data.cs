using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CEParser;
using Ionic.Zip;
using System.IO;

namespace Chronicle
{
    public partial class Data
    {
        static string guid = Guid.NewGuid().ToString();

        /// <summary>
        /// Definitions of game-specific data, i.e. country and province definitions, etc.
        /// </summary>
        public Gamedefs Defs { get; private set; }

        /// <summary>
        /// Currently selected installed game.
        /// </summary>
        public InstalledGame Game { get; private set; }

        /// <summary>
        /// Data tables containing raw game data.
        /// </summary>
        public Tables Tables { get; private set; }

        /// <summary>
        /// Represents disk-based archive containing game definitions and table definitions and data.
        /// </summary>
        public Cache Cache { get; private set; }

        /// <summary>
        /// Colorscales that are default choice for given modes.
        /// </summary>
        public Dictionary<string, Colorscale> DefaultColorscales = new Dictionary<string, Colorscale>();

        /// <summary>
        /// Colorscales customized by the user.
        /// </summary>
        public Dictionary<string, Colorscale> CustomColorscales = new Dictionary<string, Colorscale>();

        /// <summary>
        /// Time when the AAR file was saved last time.
        /// </summary>
        DateTime lastSaveTime;

        /// <summary>
        /// True if there are any pending unsaved changes (like new savegame data imported) and there is an AAR file active.
        /// </summary>
        public bool UnsavedChanges = false;

        /// <summary>
        /// Path of the current AAR file.
        /// </summary>
        public string CurrentFile = "";

        /// <summary>
        /// Converts country numerical index to text tag.
        /// </summary>
        /// <param name="index">Country index</param>
        /// <returns>Tag</returns>
        public string IndexToTag(ushort index) { return (Defs?.Countries?.GetTag(index)) ?? ""; }

        /// <summary>
        /// Converts country text tag to numerical index.
        /// </summary>
        /// <param name="index">Tag</param>
        /// <returns>Country index</returns>
        public ushort TagToIndex(string tag) { return (Defs?.Countries?.GetIndex(tag)) ?? 0; }

        public Data()
        {
            Initialize();
        }

        ~Data()
        {
            Unload();
        }

        public void Initialize()
        {
            Cache = new Cache();
        }

        /// <summary>
        /// Loads defs and table data from AAR file.
        /// </summary>
        /// <param name="path">Path of AAR file</param>
        public void Load(string path)
        {
            Core.CacheCollectionPaused = true;
            lock (Core.LoadGameLock)
            {
                Core.Dispatch.DisplayProgress("Opening AAR file...");
                Cache = new Cache(path);

                Core.Dispatch.DisplayProgress("Initializing tables..");
                Game = InstalledGame.Load(this);
                Defs = new Gamedefs();
                Tables = new Tables();
                Tables.ReadTableDefinitions();
                Tables.RecreateTimepointData();
                Core.Dispatch.HideProgress();

                Core.Dispatch.Run(() =>
                {
                    Core.MainWindow.SetGameInformation(Game);

                    Core.UI_Tableview.RefreshAvailableTablesUI();
                    Core.UI_Graphview.RefreshAvailableGraphsUI();
                    Core.UI_Mapview.RefreshAvailableMapmodesUI();
                    Core.UI_Mapview.InitializeMap();
                    Core.UI_Mapview.ResetTimeline();
                    Core.UI_Mapview.SetCurrentMode();

                    Core.UI_GraphSettings.EnforcedListsRefresh();  // Refresh enforced items list
                    Core.UI_Debug.RefreshCacheStats(Tables.CacheCount, Tables.Count, Tables.CacheSize, 0, 0);
                });
            }
            Core.CacheCollectionPaused = false;
        }

        /// <summary>
        /// Saves current cache data to an AAR file
        /// </summary>
        /// <param name="path">AAR file path</param>
        public void SaveAs(string path)
        {
            Core.Data.Cache.SaveAs(path);
            CurrentFile = path;
            UnsavedChanges = false;
            lastSaveTime = DateTime.Now;
        }

        /// <summary>
        /// Gets a province master (owner or controller) at a given point of time for a specified province.
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        /// <param name="id">Province id</param>
        /// <returns>Country index of a province master</returns>
        public ushort GetProvinceMaster(GameDate timepoint, ushort id)
        {
            return Tables.GetMasterVector(timepoint)[id];
        }

        /// <summary>
        /// Gets province basekeys, i.e. province ids or country indices of province masters, if masters is true
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        /// <param name="masters">Specifies whether the function will return province masters, if true, or province ids, if false</param>
        /// <returns>Province ids or country indice</returns>
        public ushort[] GetProvinceBasekeys(GameDate timepoint, bool masters)
        {
            if (timepoint.IsEmpty && masters)
            {
                return new ushort[Defs.Provinces.Count];
            }
            else if (!masters)
            {
                ushort[] output = new ushort[Defs.Provinces.Count];
                for (ushort i = 0; i < output.Length; i++)
                {
                    output[i] = i;
                }
                return output;
            }
            else
            {
                return Tables.GetMasterVector(timepoint);
            }
        }

        public List<ModeInterfaceElement> GetTablesInterfaceElements(bool includeTimeless)
        {
            GameDate timepoint = Tables.GetTimepoint(Tables.GetTimepointsCount() - 1);

            var tables = Tables.SelectByDate(timepoint).FindAll(x => !x.Hidden);
            if (includeTimeless && !timepoint.IsEmpty) tables.AddRange(Tables.SelectByDate(GameDate.Empty)); // add special tables
            tables = tables.OrderBy(x => x.Category).ThenBy(x => x.Section).ThenBy(x => x.Caption).ToList();

            List<ModeInterfaceElement> output = new List<ModeInterfaceElement>();

            foreach (var table in tables)
            {
                output.Add(new ModeInterfaceElement(table));
            }

            return output;
        }

        public bool IsSpecialTag(ushort identifier)
        {
            return IsSpecialTag(IndexToTag(identifier));
        }

        public bool IsSpecialTag(string identifier)
        {
            if (!Core.IsGameLoaded() || Game == null) return false;

            return Game.IsSpecialTag(identifier);
        }
    }
}
