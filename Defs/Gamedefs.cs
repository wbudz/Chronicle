using Ionic.Zip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public class Gamedefs
    {
        public CEBitmap.Editable32bppBitmap Map { get; private set; }
        public CEBitmap.Traced32bppBitmap TMap { get; private set; }

        public Provincedefs Provinces { get; private set; }
        public Terraindefs Terrain { get; private set; }
        public Countrydefs Countries { get; private set; }

        Borderdefs borders;
        Riverdefs rivers;

        public DynamicDefs Dynamic { get; private set; }
        public Mod[] Mods { get; private set; }

        string gameDir;

        /// <summary>
        /// Initializes game data, using game installed on the computer as a source.
        /// </summary>
        /// <param name="game">Installed game that serves as a source of the game data</param>
        public Gamedefs(InstalledGame game, IEnumerable<Mod> mods)
        {
            gameDir = game.Directory;
            Mods = mods?.ToArray();

            LoadFromGameFiles(game, mods);
            RebuildBorders();
        }

        /// <summary>
        /// Initializes game data, using files unpacked from an AAR data as a source.
        /// </summary>
        public Gamedefs()
        {
            // AAR files are used as source.            
            Load();
            RebuildBorders();
        }

        /// <summary>
        /// Loads game definitions from game files. Is used when loading game files without specific saved game specified.
        /// </summary>
        /// <param name="game">Installed game definition</param>
        public void LoadFromGameFiles(InstalledGame game)
        {
            LoadFromGameFiles(game, null);
        }

        /// <summary>
        /// Loads game definitions from game files. Is used when loading a saved game.
        /// </summary>
        /// <param name="game">Installed game definition</param>
        public void LoadFromGameFiles(InstalledGame game, IEnumerable<Mod> mods)
        {
            try
            {
                Parallel.Invoke(
                    () => InitializeMap(game, mods),
                    () => InitializeRivers(game, mods),
                    () => InitializeTerrain(game, mods));

                //InitializeMap(game, mods);
                //InitializeRivers(game, mods);
                //InitializeTerrain(game, mods);

                InitializeProvinces(game, mods);
                InitializeCountries(game, mods);
                InitializeLabelPositions(game, mods);
                InitializeLocalization(game, mods);

                // Finally count how much of each terrain each province has and is it water-based one or a land one.
                Provinces.SetTerrainTypes(Terrain.Count);
                CalculateProvinceTerrain(Core.GetPath(game, "map\\terrain.bmp", mods), game.Game.FlipBitmaps, game.Game.TerrainConverter, game.Game.PaletteSize);
                Provinces.SetOverriddenTerrain(Core.GetPaths(game, "map\\terrain.txt", mods), Terrain);
                Core.Log.Write("Terrain provincial data is calculated.");

                Provinces.SetWaterTerrains(Terrain.GetWaterTerrains());
                Provinces.SetMissingPositions(Map.Width, Map.Height);

                // Initialize extensions
                switch (game.Game.Token)
                {
                    case "eu3": Dynamic = new EU3DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "hoi3": Dynamic = new HoI3DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "hoi3tfh": Dynamic = new HoI3DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "vic2": Dynamic = new Vic2DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "ck2": Dynamic = new CK2DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "eu4": Dynamic = new EU4DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    case "hoi4": Dynamic = new HoI4DynamicDefs(this, game, Path.Combine(game.Directory, game.Game.GameFilesSubfolder), mods); break;
                    default: break;
                }

                this.Mods = mods?.ToArray();

            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error loading data from game files.", ex);
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        public Mod GetPrimaryMod()
        {
            if (Mods != null && Mods.Length > 0) return Mods[0];
            return null;
        }

        #region Initializers

        void InitializeMap(InstalledGame game, IEnumerable<Mod> mods)
        {
            // map\provinces.bmp and map\definition.csv
            Map = new CEBitmap.Editable32bppBitmap(
                Core.GetPath(game, "map\\provinces.bmp", mods),
                Core.GetPath(game, "map\\definition.csv", mods),
                game.Game.FlipBitmaps);
            Core.Log.Write("Provinces map finished loading.");
        }

        void InitializeRivers(InstalledGame game, IEnumerable<Mod> mods)
        {
            // map\\rivers.bmp
            rivers = new Riverdefs(Core.GetPath(game, "map\\rivers.bmp", mods), game.Game.FlipBitmaps);
            Core.Log.Write("Rivers data is initialized.");
        }

        void InitializeTerrain(InstalledGame game, IEnumerable<Mod> mods)
        {
            if (game.Game.Token == "hoi4")
            {
                // common\\terrain\\00_terrain.txt
                Terrain = new Terraindefs(game, Core.GetPaths(game, "common\\terrain\\00_terrain.txt", mods));
            }
            else
            {
                // map\\terrain.txt
                Terrain = new Terraindefs(game, Core.GetPaths(game, "map\\terrain.txt", mods));
            }
            Core.Log.Write("Terrain definitions are initialized.");
        }

        void InitializeProvinces(InstalledGame game, IEnumerable<Mod> mods)
        {
            // map\\definition.csv
            Provinces = new Provincedefs(game, Core.GetPaths(game, "map\\definition.csv", mods));
            if (game.Game.Token == "hoi4")
            {
                Provinces.SetOverriddenTerrain(Core.GetPaths(game, "map\\definition.csv", mods), Terrain);
            }
            Core.Log.Write("Province information data is initialized.");
        }

        void InitializeCountries(InstalledGame game, IEnumerable<Mod> mods)
        {
            string[] paths;
            // Load countries
            switch ((int)game.Game.ClausewitzEngineVersion)
            {
                case 1: paths = Core.GetPaths(game, "common\\countries.txt", mods); break;
                case 2: paths = Core.GetPaths(game, "common\\landed_titles\\landed_titles.txt", mods); break;
                case 3: paths = Core.GetPaths(game, "common\\country_tags\\*.txt", mods); break;
                default: return;
            }
            Countries = new Countrydefs(game, paths, mods);
            Core.Log.Write("Countries information data is initialized.");
        }

        #endregion

        #region 2nd pass initializers

        void InitializeLabelPositions(InstalledGame game, IEnumerable<Mod> mods)
        {
            // map\\positions.txt
            Provinces.LoadLabelPositions(game, Core.GetPaths(game, "map\\positions.txt", mods), Map.Width, Map.Height);
            Core.Log.Write("Province information data is initialized.");
        }

        void InitializeLocalization(InstalledGame game, IEnumerable<Mod> mods)
        {
            // Load localization
            var locale = new Core.Localization(game, Core.GetPaths(game, "localisation\\*.*", mods));
            Provinces.LoadNames(game, locale.Strings);
            Countries.LoadNames(game, locale.Strings);
            locale = null;
            Core.Log.Write("Localization data is initialized.");
        }

        #endregion

        public void RebuildBorders()
        {
            // Load borders
            borders = new Borderdefs(Map, Provinces);
            Core.Log.Write("Borders data rebuilt.");
        }

        public ushort GetProvinceByCoords(int x, int y)
        {
            int location = y * Map.Width + x;

            try
            {
                int high, low, mid;
                high = Map.PosMap.Length - 1;
                low = 0;
                if (Map.PosMap[0] == location)
                    return Map.IDMap[0];
                else if (Map.PosMap[high] == location)
                    return Map.IDMap[high];
                else
                {
                    while (low <= high)
                    {
                        mid = (high + low) / 2;
                        if (mid + 1 >= Map.PosMap.Length || (Map.PosMap[mid + 1] > location && Map.PosMap[mid] <= location))
                            return Map.IDMap[mid];
                        else if (Map.PosMap[mid] > location)
                            high = mid - 1;
                        else
                            low = mid + 1;
                    }
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Loads game definition files from BIN files.
        /// </summary>
        public void Load()
        {
            try
            {
                Core.Dispatch.DisplayProgress("Loading game definitions...", 0);
                Countries = new Countrydefs(Core.Data.Cache.ReadGameDefinitions("countries"));
                Core.Log.Write("Countries definitions loaded from cache.");

                Core.Dispatch.DisplayProgress("Loading game definitions...", 0.167f);
                Provinces = new Provincedefs(Core.Data.Cache.ReadGameDefinitions("provinces"));
                Core.Log.Write("Provinces definitions loaded from cache.");

                Core.Dispatch.DisplayProgress("Loading game definitions...", 0.333f);
                Map = new CEBitmap.Editable32bppBitmap(Core.Data.Cache.ReadGameDefinitions("map"));
                Core.Log.Write("Map data loaded from cache.");

                Core.Dispatch.DisplayProgress("Loading game definitions...", 0.5f);
                rivers = new Riverdefs(Core.Data.Cache.ReadGameDefinitions("rivers"));
                Core.Log.Write("Rivers data loaded from cache.");

                Core.Dispatch.DisplayProgress("Loading game definitions...", 0.667f);
                Terrain = new Terraindefs(Core.Data.Cache.ReadGameDefinitions("terrain"));
                Core.Log.Write("Terrain types data loaded from cache.");

                Core.Dispatch.DisplayProgress("Loading game definitions...", 0.833f);
                using (MemoryStream ms = new MemoryStream(Core.Data.Cache.ReadGameDefinitions("extensions")))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter bin = new BinaryFormatter();
                    Dynamic = (DynamicDefs)bin.Deserialize(ms);
                }
                Core.Log.Write("Extensions data loaded from cache.");

                Provinces.SetWaterTerrains(Terrain.GetWaterTerrains());
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error loading data from cache.", ex);
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        public void Save(Data data)
        {
            try
            {
                Core.Dispatch.DisplayProgress("Saving game definitions...", 0);
                Core.Data.Cache.WriteGameDefinitions("countries", Countries.Save());

                Core.Dispatch.DisplayProgress("Saving game definitions...", 0.167f);
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(ms, Dynamic);
                    Core.Data.Cache.WriteGameDefinitions("extensions", ms.ToArray());
                }

                Core.Dispatch.DisplayProgress("Saving game definitions...", 0.333f);
                Core.Data.Cache.WriteGameDefinitions("map", Map.Save());

                Core.Dispatch.DisplayProgress("Saving game definitions ...", 0.5f);
                Core.Data.Cache.WriteGameDefinitions("provinces", Provinces.Save());

                Core.Dispatch.DisplayProgress("Saving game definitions...", 0.667f);
                Core.Data.Cache.WriteGameDefinitions("rivers", rivers.Save());

                Core.Dispatch.DisplayProgress("Saving game definitions...", 0.833f);
                Core.Data.Cache.WriteGameDefinitions("terrain", Terrain.Save());
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error occured when writing game data definition files.", ex);
                return;
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        /// <summary>
        /// Generates terrain information data for all provinces.
        /// </summary>
        public void CalculateProvinceTerrain(string path, bool flip, ushort[] converter, int enforcedPaletteSize)
        {
            Dictionary<byte, byte> dict = null;
            if (converter != null)
            {
                dict = new Dictionary<byte, byte>();
                for (int i = 0; i < converter.Length; i++)
                {
                    dict.Add((byte)(converter[i] >> 8), (byte)converter[i]);
                }
            }

            var terrainMap = new CEBitmap.Static8bppBitmap(path, flip, enforcedPaletteSize, Terrain.GetColorIndexAggregator(dict));

            int mapIndex = 1;
            int terrainIndex = 1;
            int start = 0;
            ushort currentID = Map.IDMap[0];
            int currentTerrain = terrainMap.IDMap[0];

            try
            {

                while (mapIndex < Map.PosMap.Length && terrainIndex < terrainMap.PosMap.Count)
                {
                    if (Map.PosMap[mapIndex] == terrainMap.PosMap[terrainIndex])
                    {
                        Provinces.AddTerrainPixels(currentID, currentTerrain, Map.PosMap[mapIndex] - start, Map.PosMap[mapIndex]);
                        start = Map.PosMap[mapIndex];
                        currentID = Map.IDMap[mapIndex];
                        currentTerrain = terrainMap.IDMap[terrainIndex];

                        mapIndex++;
                        terrainIndex++;
                    }
                    else if (Map.PosMap[mapIndex] < terrainMap.PosMap[terrainIndex])
                    {
                        Provinces.AddTerrainPixels(currentID, currentTerrain, Map.PosMap[mapIndex] - start, Map.PosMap[mapIndex]);
                        start = Map.PosMap[mapIndex];
                        currentID = Map.IDMap[mapIndex];

                        mapIndex++;
                    }
                    else if (Map.PosMap[mapIndex] > terrainMap.PosMap[terrainIndex])
                    {
                        Provinces.AddTerrainPixels(currentID, currentTerrain, terrainMap.PosMap[terrainIndex] - start, terrainMap.PosMap[terrainIndex]);
                        start = terrainMap.PosMap[terrainIndex];
                        currentTerrain = terrainMap.IDMap[terrainIndex];

                        terrainIndex++;
                    }
                }

            }
            catch (Exception ex)
            {
                Core.Log.WriteWarning("Error calculating province terrain.", ex);
            }
        }

        public void CalculateProvinceTerrain()
        {

        }

        public void EditBitmap(BitmapContext bc, int[][] colors)
        {
            unsafe
            {
                CEBitmap.Editable32bppBitmap map = Map;
                int* p = bc.Pixels;

                Parallel.For(0, map.Height, (row) =>
                {
                    int color;
                    int pos = map.RowsOrigins[row] + 1;
                    int rowend = map.RowsOrigins[row + 1] + 1;
                    int end;
                    int idx;
                    while (pos < rowend)
                    {
                        end = map.PosMap[pos];
                        idx = map.PosMap[pos - 1] - 1;
                        if (colors[map.IDMap[pos - 1]].Length > 1)
                        {
                            while (++idx < end)
                            {
                                *(p + idx) = colors[map.IDMap[pos - 1]][((idx + idx / map.Width) / 2) % colors[map.IDMap[pos - 1]].Length];
                            }
                        }
                        else
                        {
                            color = colors[map.IDMap[pos - 1]][0];
                            while (++idx < end)
                            {
                                *(p + idx) = color;
                            }
                        }
                        ++pos;
                    }
                });
            }
        }

        public void DrawBorders(BitmapContext bc, bool[] visibility)
        {
            ushort[] provOwnership = Core.Data.GetProvinceBasekeys(Core.UI_Mapview.CurrentTimepoint, true);
            borders.Draw(bc,
                (visibility[0] ? BorderDisplayOptions.Country : 0) |
                (visibility[1] ? BorderDisplayOptions.Land : 0) |
                (visibility[2] ? BorderDisplayOptions.Sea : 0) |
                (visibility[3] ? BorderDisplayOptions.Shore : 0),
                provOwnership);
        }

        public void DrawBorders(int[] array, int row)
        {
            Color cLB = Core.Settings.DisplayLandBorders ? CEBitmap.Bitmap.Int32ToColor(Core.Settings.LandBorderColor) : Color.FromArgb(0, 0, 0, 0);
            Color cSB = Core.Settings.DisplaySeaBorders ? CEBitmap.Bitmap.Int32ToColor(Core.Settings.SeaBorderColor) : Color.FromArgb(0, 0, 0, 0);
            Color cSS = Core.Settings.DisplayShoreBorders ? CEBitmap.Bitmap.Int32ToColor(Core.Settings.ShoreBorderColor) : Color.FromArgb(0, 0, 0, 0);
            Color cCB = Core.Settings.DisplayCountryBorders ? CEBitmap.Bitmap.Int32ToColor(Core.Settings.CountryBorderColor) : Color.FromArgb(0, 0, 0, 0);
            ushort[] provOwnership = Core.Data.GetProvinceBasekeys(Core.UI_Mapview.CurrentTimepoint, true);
            borders.Overlay(array, cLB, cSB, cSS, cCB, provOwnership, row);
        }

        public void DrawBorders(BitmapContext bc)
        {
            ushort[] provOwnership = Core.Data.GetProvinceBasekeys(Core.UI_Mapview.CurrentTimepoint, true);
            borders.Draw(bc, (BorderDisplayOptions.Country | BorderDisplayOptions.Land | BorderDisplayOptions.Sea | BorderDisplayOptions.Shore), provOwnership);
        }

        public void DrawRivers(int[] array, int row)
        {
            rivers.Overlay(array, Core.Settings.RiverColor, row);
        }

        public void DrawRivers(BitmapContext bc)
        {
            rivers.Draw(bc, Core.Settings.RiverColor);
        }

        public unsafe void DrawAreaShading(BitmapContext bc)
        {
            byte* b = (byte*)bc.Pixels;

            bool[] water = Provinces.WaterProvinces;
            float[] heights = Provinces.GetProvinceHeights(Map.Width);
            int[] baserows = Provinces.GetProvinceBaseRows(Map.Width);

            Parallel.For(0, Map.Height, (row) =>
            {
                int idx;
                int rowstart = Map.RowsOrigins[row] + 1;
                int rowend = Map.RowsOrigins[row + 1] + 1;
                for (int i = rowstart; i < rowend; i++)
                {
                    ushort id = Map.IDMap[i - 1];
                    int rowindex = baserows[id] - row + 1;
                    if (rowindex < heights[id] / 2f)
                    {
                        float factor = water[id] ? 0.9f + rowindex * 2 / heights[id] * 0.1f : 0.9f + rowindex * 2 / heights[id] * 0.1f;
                        idx = Map.PosMap[i - 1] * 4;
                        int end = Map.PosMap[i] * 4;
                        while (idx < end)
                        {
                            *(b + idx) = (byte)(*(b + idx) * factor);
                            *(b + idx + 1) = (byte)(*(b + idx + 1) * factor);
                            *(b + idx + 2) = (byte)(*(b + idx + 2) * factor);
                            idx += 4;
                        }

                    }
                }
            });
        }

        public void LoadRandomNewWorld(InstalledGame game, string path)
        {
            Core.AttemptDelete(Core.Paths.RandomNewWorldTemp, true);

            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(path))
                {
                    var rnw = zip.FirstOrDefault(x => x.FileName == "rnw.zip");
                    if (rnw == null) return;
                    rnw.Extract(Core.Paths.RandomNewWorldTemp, ExtractExistingFileAction.OverwriteSilently);
                }

                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Path.Combine(Core.Paths.RandomNewWorldTemp, "rnw.zip")))
                {
                    zip.ExtractAll(Core.Paths.RandomNewWorldTemp);
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error extracting Random New World data from the savegame file.", ex);
                return;
            }

            Map = new CEBitmap.Editable32bppBitmap(
                Path.Combine(Core.Paths.RandomNewWorldTemp, "provinces.bmp"),
                Core.GetPath(game, "map\\definition.csv", Mods),
                game.Game.FlipBitmaps);
            Core.Log.Write("Provinces map loaded from RNW data.");

            // map\\rivers.bmp
            rivers = new Riverdefs(Path.Combine(Core.Paths.RandomNewWorldTemp, "rivers.bmp"), game.Game.FlipBitmaps);
            Core.Log.Write("Rivers map loaded from RNW data.");

            CalculateRandomNewWorldProvinceTerrain(Path.Combine(Core.Paths.RandomNewWorldTemp, "data.txt"));
            RebuildBorders();

            Core.AttemptDelete(Core.Paths.RandomNewWorldTemp, true);
        }

        public void CalculateRandomNewWorldProvinceTerrain(string path)
        {
            // not implemented
        }
    }
}
