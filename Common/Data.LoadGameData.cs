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
        // Loads a new game after clicking on a backstage game button.
        public void LoadGame(InstalledGame game, UI_Mapview mapview)
        {
            Core.CacheCollectionPaused = true;
            lock (Core.LoadGameLock)
            {
                Unload();
                Core.Log.Write("New game: \"" + game.Game.Name + "\" is being initialized.");

                Game = game;
                Defs = new Gamedefs(game, null);

                Defs.Save(this);
                Game.Save(this);

                TableScripts.RefreshCurrentGameScripts(game.Token);
                // Tables that do not require a savegame and are timepoint independent are created.
                // Status: Unspecified
                // Cached: no
                // Tables that require a savegame don't exist at this point.
                Tables.CreateTables(game.Token);
                // Tables are now parsed.
                // Status: Parsed
                // Cached: yes
                Tables.ParseTables();
                TableScripts.RefreshStats();

                Core.Dispatch.Run(() =>
                {
                    Core.MainWindow.SetGameInformation(game);

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

        void Unload()
        {
            Defs = null;
            Game = null;
            Tables = new Tables();
            Cache.Clear();

            Core.UI_Developer.Reset();
            Core.UI_Tableview.Reset();
            Core.UI_Graphview.Reset();
        }
    }
}
