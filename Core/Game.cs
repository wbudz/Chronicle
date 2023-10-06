using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    [Serializable]
    /// <summary>
    /// Defines a game supported by the program.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Short version of the game token, e.g. "eu3".
        /// </summary>
        public string Token = "";

        /// <summary>
        /// Full name of the game, e.g. "Europa Universalis III"
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Full official name of the game, e.g. "Europa Universalis III"
        /// </summary>
        public string OfficialName = "";

        /// <summary>
        /// Short name of the game, e.g. "EU3"
        /// </summary>
        public string ShortName = "";

        /// <summary>
        /// Name of the EXE file, e.g. "eu3game.exe", used for finding root folder of the game
        /// </summary>
        public string EXEName = "";

        /// <summary>
        /// Defines name of the folder used for holding mods, e.g. "mods".
        /// </summary>
        public string ModFolderName = "";

        /// <summary>
        /// Defines name of the folder used for holding mods, e.g. "save games".
        /// </summary>
        public string SavegameFolderName = "";

        /// <summary>
        /// If true, the game allows to use only one mod at time (CE 1.0 style) and puts file in the mod folder; otherwise usage of combination of mods is allows (CE 2.0 style), savegames are put in the default savegame folder.
        /// </summary>
        public bool SingleMod = false;

        /// <summary>
        /// Defines extension used for savegames, e.g. "eu3". Should be given without preceding dot nor asterisk.
        /// </summary>
        public string SavegameExtension = "";

        /// <summary>
        /// Lists special tags (e.g. rebels, pirates, etc.) which will not be shown in statistics.
        /// </summary>
        public string[] SpecialCountryTags = new string[0];

        /// <summary>
        /// If true, map bitmaps will not be flipped upside down when being loaded, otherwise false.
        /// </summary>
        public bool FlipBitmaps = false;

        /// <summary>
        /// If true, the game will be hidden from game management selection.
        /// </summary>
        public bool Hidden = false;

        /// <summary>
        /// If not empty, it will point to subfolder in the main game folder from which files should be read, e.g. 'tfh' within 'Hearts of Iron III' folder.
        /// </summary>
        public string GameFilesSubfolder = "";

        /// <summary>
        /// Not used currently, if true, endline character for savegame files is /r/n, otherwise /n.
        /// </summary>
        public bool SavegameLineFeed = true;

        /// <summary>
        /// Not used currently, if true, endline character for savegame files is /r/n, otherwise /n.
        /// </summary>
        public bool GamedefsLineFeed = true;

        /// <summary>
        /// Used for overriding terrain definitions. High byte contains original terrain bytecode, lower byte its intended replacement.
        /// </summary>
        public ushort[] TerrainConverter = new ushort[0];

        /// <summary>
        /// Specifies how many bytes does palette take in indexed graphic files.
        /// </summary>
        public int PaletteSize = 1024;

        /// <summary>
        /// Defines Clausewitz Engine version of a game.
        /// </summary>
        public double ClausewitzEngineVersion = 1;

        /// <summary>
        /// Defines what nodes should be ignored when reading a game. Is given in 'x;y;z' format where x is a name of a node to be ignored (wildcards allowed at the beginning and end of the string), y is a node's parent ('*' to ignore this requirement), z is node's depth ('*' to ignore this requirement).
        /// </summary>
        public List<string> IgnoredNodeRules = new List<string>();

        /// <summary>
        /// Specifies which large localization files do not contain anything useful from the point of view of the program and can be ommitted.
        /// </summary>
        public List<string> IgnoredLocalizationFiles = new List<string>();

        /// <summary>
        /// Defines which data table defines borders (in practice, either controllership or ownership).
        /// </summary>
        public string ProvinceMasterTable = "";

        /// <summary>
        /// Open dialog file filter for a savegame file for the given game.
        /// </summary>
        public string OpenDialogFileFilter = "";

        /// <summary>
        /// Defines Steam cache folder.
        /// </summary>
        public string SteamCloudCache = "";

        /// <summary>
        /// If true, Chronicle supports reading binary savegame for this game.
        /// </summary>
        public bool SupportsBinarySaves = false;

        /// <summary>
        /// Specifies which is the minimum interval between autosaves to register, to prevent too frequent reading of Ironman savegames.
        /// This is independent of game settings although it's impossible to read savegame data more frequently than it is saved by the game.
        /// 0 - as frequent as possible (not recommended)
        /// 1 - one day
        /// 2 - one month
        /// 3 - three months
        /// 4 - half a year
        /// 5 - one year
        /// 6 - three years
        /// 7 - five years
        /// 8 - ten years
        /// 9 - twenty-five years
        /// 10 - hundred years        /// 
        /// </summary>
        public AutosaveFrequency DefaultMinAutosaveFrequency;

        public List<TableScript> Scripts;

        /// <summary>
        /// Creates a new game
        /// </summary>
        /// <param name="token">Token, e.g. "eu3"</param>
        public Game(string token)
        {
            Token = token.ToLowerInvariant();
        }

        public static Game AddGameSupport(string token)
        {
            Game g = new Game(token);

            switch (token)
            {
                case "eu3":
                    g.Name = "Europa Universalis III";
                    g.OfficialName = "Europa Universalis III";
                    g.ShortName = "EU3";
                    g.EXEName = "eu3game.exe";
                    g.SavegameExtension = "eu3";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mods";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = true;
                    g.FlipBitmaps = true;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.ClausewitzEngineVersion = 1;
                    g.ProvinceMasterTable = "ProvinceOwnership";
                    g.OpenDialogFileFilter = "Europa Universalis III saved games (*.eu3)|*.eu3";
                    g.SpecialCountryTags = new string[] { "REB", "PIR", "NAT", "MIN" };
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.ThreeMonths;
                    break;

                case "hoi3":
                    g.Name = "Hearts of Iron III";
                    g.OfficialName = "Hearts of Iron III";
                    g.ShortName = "HoI3";
                    g.EXEName = "hoi3game.exe";
                    g.SavegameExtension = "hoi3";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = true;
                    g.FlipBitmaps = false;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.ClausewitzEngineVersion = 1;
                    g.ProvinceMasterTable = "ProvinceControllership";
                    g.OpenDialogFileFilter = "Hearts of Iron III saved games (*.hoi3)|*.hoi3";
                    g.SpecialCountryTags = new string[] { "REB" };
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.HalfMonth;
                    break;

                case "hoi3tfh":
                    g.Name = "Hearts of Iron III TFH";
                    g.OfficialName = "Hearts of Iron III";
                    g.ShortName = "HoI3TFH";
                    g.EXEName = "hoi3_tfh.exe";
                    g.SavegameExtension = "hoi3";
                    g.GameFilesSubfolder = "tfh";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = true;
                    g.FlipBitmaps = false;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.ClausewitzEngineVersion = 1;
                    g.ProvinceMasterTable = "ProvinceControllership";
                    g.OpenDialogFileFilter = "Hearts of Iron III saved games (*.hoi3)|*.hoi3";
                    g.SpecialCountryTags = new string[] { "REB" };
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.HalfMonth;
                    break;

                case "vic2":
                    g.Name = "Victoria II";
                    g.OfficialName = "Victoria II";
                    g.ShortName = "Vic2";
                    g.EXEName = "v2game.exe";
                    g.SavegameExtension = "v2";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = true;
                    g.FlipBitmaps = false;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.ClausewitzEngineVersion = 1;
                    g.ProvinceMasterTable = "ProvinceOwnership";
                    g.OpenDialogFileFilter = "Victoria II saved games (*.v2)|*.v2";
                    g.SpecialCountryTags = new string[] { "REB" };
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.ThreeMonths;
                    break;

                case "ck2":
                    g.Name = "Crusader Kings II";
                    g.OfficialName = "Crusader Kings II";
                    g.ShortName = "CK2";
                    g.EXEName = "ck2game.exe";
                    g.SavegameExtension = "ck2";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = true;
                    g.FlipBitmaps = true;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = false;
                    g.PaletteSize = 1020;
                    g.TerrainConverter = new ushort[] { (15 << 8) | 1 };
                    g.ClausewitzEngineVersion = 2;
                    g.ProvinceMasterTable = "ProvinceOwnership";
                    g.OpenDialogFileFilter = "Crusader Kings II saved games (*.ck2)|*.ck2";
                    g.SteamCloudCache = "203770\\remote\\save games";
                    g.SupportsBinarySaves = true;
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.ThreeMonths;
                    break;

                case "eu4":
                    g.Name = "Europa Universalis IV";
                    g.OfficialName = "Europa Universalis IV";
                    g.ShortName = "EU4";
                    g.EXEName = "eu4.exe";
                    g.SavegameExtension = "eu4";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = false;
                    g.FlipBitmaps = true;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.PaletteSize = 1020;
                    g.ClausewitzEngineVersion = 3;
                    g.ProvinceMasterTable = "ProvinceOwnership";
                    g.OpenDialogFileFilter = "Europa Universalis IV saved games (*.eu4)|*.eu4";
                    g.SteamCloudCache = "236850\\remote\\save games";
                    g.SupportsBinarySaves = true;
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.ThreeMonths;
                    break;

                case "hoi4":
                    g.Name = "Hearts of Iron IV";
                    g.OfficialName = "Hearts of Iron IV";
                    g.ShortName = "HoI4";
                    g.EXEName = "hoi4.exe";
                    g.SavegameExtension = "hoi4";
                    g.GameFilesSubfolder = "";
                    g.ModFolderName = "mod";
                    g.SavegameFolderName = "save games";
                    g.SingleMod = false;
                    g.FlipBitmaps = true;
                    g.GamedefsLineFeed = true;
                    g.SavegameLineFeed = true;
                    g.PaletteSize = 1020;
                    g.ClausewitzEngineVersion = 3;
                    g.ProvinceMasterTable = "ProvinceControllership";
                    g.OpenDialogFileFilter = "Hearts of Iron IV saved games (*.hoi4)|*.hoi4";
                    g.SteamCloudCache = "394360\\remote\\save games";
                    g.SupportsBinarySaves = true;
                    g.DefaultMinAutosaveFrequency = AutosaveFrequency.OneMonth;
                    break;

                default:
                    break;
            }

            return g;
        }

        public static string[] GetGamesList()
        {
            return Core.Games.Select(x => x.Name).ToArray();
        }

        public static string GetAutosaveFrequencyString(AutosaveFrequency freq)
        {
            switch (freq)
            {
                case AutosaveFrequency.Default: return "(default)";
                case AutosaveFrequency.Maximum: return "Maximum";
                case AutosaveFrequency.HalfMonth: return "Twice a month";
                case AutosaveFrequency.OneMonth: return "Monthly";
                case AutosaveFrequency.TwoMonths: return "Bimonthly";
                case AutosaveFrequency.ThreeMonths: return "Quarterly";
                case AutosaveFrequency.HalfYear: return "Twice a year";
                case AutosaveFrequency.OneYear: return "Yearly";
                case AutosaveFrequency.ThreeYears: return "Once per three years";
                case AutosaveFrequency.FiveYears: return "Once per five years";
                case AutosaveFrequency.TenYears: return "Once per ten years";
                default: return "";
            }
        }

        public static string[] GetAutosaveFrequenciesList()
        {
            string[] output = new string[11];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = GetAutosaveFrequencyString((AutosaveFrequency)i);
            }
            return output;
        }
    }

    public enum AutosaveFrequency
    {
        Default, Maximum, HalfMonth, OneMonth, TwoMonths, ThreeMonths, HalfYear, OneYear, ThreeYears, FiveYears, TenYears
    }
}
