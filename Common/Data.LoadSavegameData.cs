using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public partial class Data
    {
        public void LoadSavegame(string path, bool import, bool manual)
        {
            byte[] raw = null;
            bool binary = false;
            bool zipped = false;
            CEParser.File file = null;

            Core.CacheCollectionPaused = true;
            lock (Core.LoadGameLock)
            {
                Core.Log.Write("Loading a savegame: " + Path.GetFileName(path) + "...");

                InstalledGame game = Core.FindGameForSavegame(path);
                string name = Path.GetFileName(path);
                binary = Core.IsBinarySavegame(path);
                zipped = Core.IsCompressedSavegame(path);
                GameDate gameDate = Core.GetGameDate(game, path);

                if (binary && !game.Game.SupportsBinarySaves)
                {
                    Core.Log.ReportError("Ironman savegames for this game are not supported in the current version of the program. The savegame file will not be imported.");
                    return;
                }
                if (import && !manual && Core.IsSavegameIntervalTooSmall(gameDate, Tables.GetTimepoint(-1), game.IntervalBetweenSaves))
                {
                    Core.Log.Write("Game not loaded because of savegame interval check (between " + gameDate + " and " + Tables.GetTimepoint(-1) + ".");
                    return;
                }

                if (zipped)
                {
                    Core.Dispatch.DisplayProgress("Uncompressing savegame file...");
                    raw = Core.ExtractFile(path, "*" + Path.GetExtension(name));
                }
                else
                {
                    Core.Dispatch.DisplayProgress("Reading uncompressed savegame file...");
                    raw = Core.ReadRawBytes(path);
                }

                if (binary)
                {
                    file = new CEParser.BinaryFile(new MemoryStream(raw), game.Token, Encoding.GetEncoding("1250"));
                    if (game.Game.Token == "eu4") (file as CEParser.BinaryFile).Decoder.EnforceDateDatatype = true;
                }
                else
                {
                    file = new CEParser.TextFile(new MemoryStream(raw));
                }

                if (zipped && game.Game.Token == "eu4" && Core.ZipContainsEntry(path, "rnw.zip"))
                {
                    Core.Dispatch.DisplayMessageBox("Random New World savegames are not currently supported.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    //Core.Data.Defs.LoadRandomNewWorld(game, path);
                }

                Core.Dispatch.DisplayProgress("Parsing savegame file...");

                Core.Log.Write("Parsing savegame file...");
                file.FileParseProgress += file_FileParseProgress;
                file.Parse();
                file.FileParseProgress -= file_FileParseProgress;
                Core.Log.Write("Processing savegame file...");

                Core.Dispatch.DisplayProgress("Initializing savegame data...");

                var mods = GetActiveMods(game, file, path);

                // Now initialize data
                if (!Core.IsGameLoaded(game.Token, mods))
                {
                    Cache.Clear();
                    Game = game;
                    Defs = new Gamedefs(Game, mods);

                    Defs.Save(this);
                    Game.Save(this);
                }

                Defs.Countries.AddDynamicTags(game.Token, file);
                Tables.PrepareNodesets(game, file);

                if (!import || Tables == null)
                {
                    Cache.ClearTables();
                    Tables = new Tables();
                    Core.UI_Developer.Reset();
                    Core.UI_Tableview.Reset();
                    Core.UI_Graphview.Reset();
                }

                TableScripts.RefreshCurrentGameScripts(game.Token);

                // Tables that do not require a savegame and are timepoint independent are created - probably they exist already.
                // Status: Unspecified
                // Cached: no
                // Tables that require a savegame may exist at his point.
                // Status: Unspecified
                // Cached: no
                Tables.CreateTables(game.Token);
                // Tables are now parsed.
                // Status: Parsed
                // Cached: yes
                Tables.ParseTables();

                // Tables that do not require a savegame exist at this point.
                // Status: Unspecified
                // Cached: no
                // Tables that require a savegame are created at this point
                // Status: Unspecified
                // Cached: no
                Tables.CreateTables(gameDate, game.Token);
                // Tables are now parsed.
                // Status: Parsed
                // Cached: yes
                Tables.ParseTables(file, gameDate);

                TableScripts.RefreshStats();

                Core.Dispatch.DisplayProgress("Saving tables to cache...");
                Tables.SaveToCache();
                Core.Dispatch.HideProgress();

                // Tables now can be made available.
                Tables.AddTimepoint(gameDate);

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

        static IEnumerable<Mod> GetActiveMods(InstalledGame game, CEParser.File file, string path)
        {
            if (game.AutomaticallySelectMods)
            {
                if (game.Game.SingleMod || game.Game.ClausewitzEngineVersion < 3)
                {
                    string folder = Path.GetDirectoryName(path); // get "save games" folder
                    folder = Path.GetDirectoryName(folder); // get mod root folder
                    folder = Path.GetFileName(folder); // get folder name

                    // Check if "root" folder is among moddirs
                    for (int i = 0; i < game.Mods.Count; i++)
                    {
                        if (Path.GetFileName(Path.GetDirectoryName(game.Mods[i].SaveDir)) == folder)
                        {
                            yield return game.Mods[i];
                        }
                    }
                }
                else
                {
                    // Get mods list
                    List<string> modlist = new List<string>();
                    if (file.HasAContainer(file.Root, "mods_enabled"))
                    {
                        modlist.AddRange(file.GetEntries(file.GetSubnode(file.Root, "mods_enabled")));
                    }
                    if (file.HasAContainer(file.Root, "mod_enabled"))
                    {
                        modlist.AddRange(file.GetEntries(file.GetSubnode(file.Root, "mod_enabled")));
                    }
                    foreach (var m in modlist)
                    {
                        if (game.Mods.Find(x => x.DirName == m) != null)
                        {
                            yield return game.Mods.Find(x => x.DirName == m);
                        }
                    }
                }
            }
            else
            {
                foreach (var m in game.ManuallyEnabledMods)
                {
                    yield return game.Mods.Find(x => x.Name == m);
                }
            }
        }

        static void file_FileParseProgress(object source, CEParser.FileParseEventArgs e)
        {
            Core.Dispatch.DisplayProgress("Parsing savegame file...", e.Progress);
        }
    }

}
