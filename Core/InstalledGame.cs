using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chronicle
{
    [Serializable]
    /// <summary>
    /// Contains information about a game that has been installed on the computer (with user-specific information as game directory, etc.)
    /// </summary>
    public class InstalledGame
    {
        Game game;
        /// <summary>
        /// Relates to general game information.
        /// </summary>
        public Game Game
        {
            get
            {
                if (game != null) return game;
                game = Core.Games.First(x => x.Token == Token);
                return game;
            }
        }

        /// <summary>
        /// Game token, to identify corresponding Game information
        /// </summary>
        public string Token = "";

        /// <summary>
        /// Name of the game, by default the same as Game object specifies, can be edited.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Location of the game folder on the disk.
        /// </summary>
        public string Directory = "";

        /// <summary>
        /// User-specified interval between saves
        /// </summary>
        public AutosaveFrequency IntervalBetweenSaves = AutosaveFrequency.Default;

        /// <summary>
        /// Specifies how the game is prioritized on the list (0-lower, 1-normal, 2-higher).
        /// </summary>
        public int ListPriority = 1;

        public bool AutomaticallySelectMods = true;

        public List<string> ManuallyEnabledMods = new List<string>();

        [NonSerialized]
        [XmlIgnore]
        public List<Mod> Mods = new List<Mod>();

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public InstalledGame()
        {
        }

        /// <summary>
        /// Initializes an installed game.
        /// </summary>
        /// <param name="token">Game token</param>
        /// <param name="path">Path of the game folder</param>
        public InstalledGame(string token, string name, string path)
        {
            Token = token;
            Name = name;
            Directory = path;
            //GenerateSavegamePaths();
        }

        public InstalledGame(InstalledGame other)
        {
            Token = other.Token;
            Name = other.Name;
            Directory = other.Directory;
            IntervalBetweenSaves = other.IntervalBetweenSaves;
            ListPriority = other.ListPriority;
        }

        public static InstalledGame Load(Data data)
        {
            try
            {
                Core.Dispatch.DisplayProgress("Loading installed game definition...");
                using (MemoryStream ms = new MemoryStream(Core.Data.Cache.ReadGameDefinitions("game")))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter bin = new BinaryFormatter();
                    return (InstalledGame)bin.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error loading data from cache.", ex);
                return null;
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        public void Save(Data data)
        {
            Core.Dispatch.DisplayProgress("Saving installed game definition...");
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(ms, this);
                    Core.Data.Cache.WriteGameDefinitions("game", ms.ToArray());
                }
            }
            finally
            {
                Core.Dispatch.HideProgress();
            }
        }

        /// <summary>
        /// Generates all possible paths where saved games may be located.
        /// </summary>
        public string[] GenerateSavegamePaths()
        {
            List<string> output = new List<string>();

            output.Add(Path.Combine(Directory, Game.SavegameFolderName)); // Program files
            output.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore", output[0].Remove(0, output[0].IndexOf('\\') + 1))); // Virtual store
            output.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", Game.OfficialName, Game.SavegameFolderName)); // Documents
            if (Game.SteamCloudCache != "") output.Add(Path.Combine(Core.Paths.Steam, Game.SteamCloudCache));

            // A very special case for Victoria II/2
            if (output[2].IndexOf("Victoria 2") > 0)
            {
                output[2] = output[2].Replace("Victoria 2", "Victoria II");
            }

            // Add mod folders
            for (int i = 0; i < Mods.Count; i++)
            {
                output.Add(Mods[i].SaveDir);
            }

            return output.Distinct().ToArray();
        }

        public bool IsSpecialTag(string tag)
        {
            return Game.SpecialCountryTags.Contains(tag);
        }

        /// <summary>
        /// Gets list of all the savegames present.
        /// </summary>
        /// <returns>Array of savegame locations.</returns>
        public string[] GetSavegames()
        {
            List<string> savegames = new List<string>();
            var dir = GenerateSavegamePaths();
            for (int i = 0; i < dir.Length; i++)
            {
                if (dir == null || !System.IO.Directory.Exists(dir[i])) continue;
                savegames.AddRange(System.IO.Directory.GetFiles(dir[i], "*." + Game.SavegameExtension));
            }
            return savegames.ToArray();
        }

        public void RefreshModList()
        {
            Mods = new List<Mod>();

            // First try program files folder
            string moddir = Path.Combine(this.Directory, this.Game.ModFolderName);
            if (System.IO.Directory.Exists(moddir))
            {
                var moddeffiles = System.IO.Directory.GetFiles(moddir, "*.mod");
                foreach (var mdf in moddeffiles)
                {
                    var mod = Mod.CreateMod(game, moddir, mdf);
                    if (mod.DirName != "") Mods.Add(mod);
                }
            }

            // Then try documents folder
            moddir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", Game.OfficialName, Game.ModFolderName);
            moddir = moddir.Replace("Victoria 2", "Victoria II"); // A very special case for Victoria II/2
            if (System.IO.Directory.Exists(moddir))
            {
                var moddeffiles = System.IO.Directory.GetFiles(moddir, "*.mod");
                foreach (var mdf in moddeffiles)
                {
                    var mod = Mod.CreateMod(game, moddir, mdf);
                    if (mod.DirName != "") Mods.Add(mod);
                }
            }

            // At last try Steam folder (there shouldn't be any mods there really) 
            if (Game.SteamCloudCache != "")
            {
                moddir = Path.Combine(Core.Paths.Steam, Game.OfficialName, Game.ModFolderName);
                if (System.IO.Directory.Exists(moddir))
                {
                    var moddeffiles = System.IO.Directory.GetFiles(moddir, "*.mod");
                    foreach (var mdf in moddeffiles)
                    {
                        var mod = Mod.CreateMod(game, moddir, mdf);
                        if (mod.DirName != "") Mods.Add(mod);
                    }
                }
            }
        }
    }
}
