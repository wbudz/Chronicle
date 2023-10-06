using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Diagnostics;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using Ionic.Zip;
using System.Text.RegularExpressions;

namespace Chronicle
{
    public static partial class Core
    {
        public static readonly string GUID = Guid.NewGuid().ToString();

        public static Data Data = new Data();

        /// <summary>
        /// Random number generator.
        /// </summary>
        public static Random RNG = new Random();

        /// <summary>
        /// Standard ANSI encoding.
        /// </summary>
        public static Encoding ANSI = Encoding.GetEncoding(1250);

        /// <summary>
        /// Contains program settings.
        /// </summary>
        public static Settings Settings = new Settings();

        /// <summary>
        /// Events and errors logger.
        /// </summary>
        public static Log Log;

        /// <summary>
        /// List of supported games.
        /// </summary>
        public static List<Game> Games = new List<Game>();

        /// <summary>
        /// List of games on this computer.
        /// </summary>
        public static InstalledGames InstalledGames;

        public static object LoadGameLock = new object();

        public static bool CacheCollectionPaused = false;

        public static bool DesignMode { get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; } }

        public static bool FullyLoaded = false;

        public static bool IsClosingForUpdate { get; set; }

        public static bool IsGameLoaded() { return Data.Defs != null; }
        public static bool IsGameLoaded(string token) { return ((Data.Game?.Game?.Token) ?? "") == token; }
        public static bool IsGameLoaded(string token, IEnumerable<Mod> mods)
        {
            return ((Data.Game?.Game?.Token) ?? "") == token && ((Data.Defs?.Mods) ?? new Mod[0]).SequenceEqual(mods);
        }

        public static bool IsSavegameLoaded() { return Data.Tables != null && Data.Tables.Count > 0; }

        /// <summary>
        /// Defines savegames from which versions of Chronicle the program will accept. I.e. specifying 1.0.0 means that any older version than 1.0.0 won't be accepted by the program. The last portion of the versioning ("revision") is not used for comparisons.
        /// </summary>
        public static readonly Version MinimumSavegameCompatibility = new Version(3, 3, 0, 0);

        /// <summary>
        /// Extracts the assembly file version.
        /// </summary>
        public static Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        static BitmapImage iModeProvinces = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/mode-province-16.png"));
        public static BitmapImage ProvinceModeIcon { get { return iModeProvinces; } }
        static BitmapImage iModeCountries = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/mode-country-16.png"));
        public static BitmapImage CountryModeIcon { get { return iModeCountries; } }
        static BitmapImage iModeSpecial = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/mode-special-16.png"));
        public static BitmapImage SpecialModeIcon { get { return iModeSpecial; } }

        #region UI elements

        public static MainWindow MainWindow;

        public static UI_Mapview UI_Mapview;
        public static UI_Tableview UI_Tableview;
        public static UI_Graphview UI_Graphview;

        public static UI_Debug UI_Debug;
        public static UI_Log UI_Log;
        public static UI_Developer UI_Developer;

        public static UI_MapControl UI_MapControl;
        public static UI_MapStats UI_MapStats;
        public static UI_MapColors UI_MapColors;

        public static UI_TableSettings UI_TableSettings;

        public static UI_GraphSettings UI_GraphSettings;

        #endregion

        #region Dialogs

        /// <summary>
        /// Open dialog for loading AAR and savegame files.
        /// </summary>
        public static OpenFileDialog OpenDialog = new OpenFileDialog();

        /// <summary>
        /// Save dialog for saving AAR files.
        /// </summary>
        public static SaveFileDialog SaveDialog = new SaveFileDialog();

        /// <summary>
        /// Save dialog for saving graphics files (PNG, JPG, GIF).
        /// </summary>
        public static SaveFileDialog SaveGFXDialog = new SaveFileDialog();

        /// <summary>
        /// Save dialog for saving CSV files.
        /// </summary>
        public static SaveFileDialog SaveCSVDialog = new SaveFileDialog();

        #endregion

        #region Colors

        static public Dictionary<string, _Color> PresetColors { get; set; } = new Dictionary<string, _Color>();

        #endregion

        /// <summary>
        /// Initializes log.
        /// </summary>
        public static void InitializeLog()
        {
            Log = new Log(Path.Combine(Paths.AppData, "Log.txt"));
            Log.InitializeLog();
        }

        /// <summary>
        /// Contains reference to the original culture (Windows locale) before it is forced to US English by the application.
        /// </summary>
        public static CultureInfo OriginalCulture { get; private set; }

        /// <summary>
        /// Defines dark shadow under province labels.
        /// </summary>
        public static DropShadowEffect LabelsShadow
        {
            get
            {
                var e = new DropShadowEffect();
                e.BlurRadius = 1;
                e.ShadowDepth = 0;
                return e;
            }
        }

        /// <summary>
        /// Initializes basic program information and data structures.
        /// </summary>
        public static void InitializeCore()
        {
            // Clean up locations
            try
            {
                Core.TryDelete(Core.Paths.Temp);
                Core.TryDelete(Core.Paths.ModTemp);
                Core.TryDelete(Core.Paths.DynamicCompile);
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError cleaning up temporary file folders.", ex);
            }

            OriginalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            CEParser.Bytes.Initialize();
            Paths.SetSteamFolderPath(); // exceptions handled internally

            Games.Add(Game.AddGameSupport("eu3"));
            Games.Add(Game.AddGameSupport("hoi3tfh"));
            Games.Add(Game.AddGameSupport("hoi3"));
            Games.Add(Game.AddGameSupport("vic2"));
            Games.Add(Game.AddGameSupport("ck2"));
            Games.Add(Game.AddGameSupport("eu4"));
            Games.Add(Game.AddGameSupport("hoi4"));

            // Load configuration
            try
            {
                if (File.Exists(Path.Combine(Paths.AppData, "Configuration.xml")))
                {
                    Settings = Settings.LoadFromFile(Path.Combine(Paths.AppData, "Configuration.xml"));
                    Settings.EnsureMinimalIntervals();
                    Settings.EnsureUpdateLocations();
                }
                else
                {
                    // First run
                    Settings = new Settings();
                    System.Windows.Forms.MessageBox.Show("All settings are reverted to defaults. For quick start, use File -> Import to define your games (you can import Steam library) and import savegames.", "First run", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError initializing settings. Default values will be loaded.", ex);
                Settings = new Settings();
            }

            var cla = Environment.GetCommandLineArgs();
            Core.Settings.DebugMode = Array.Find(cla, x => x.ToLowerInvariant().Contains("/debug")) != null;

            // Load installed games
            try
            {
                InstalledGames = new InstalledGames();
                if (File.Exists(Path.Combine(Paths.AppData, "InstalledGames.xml")))
                    InstalledGames.Load(Path.Combine(Paths.AppData, "InstalledGames.xml"));
                InstalledGames.Initialize();
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError loading installed games.", ex);
                InstalledGames = new InstalledGames();
            }

            // Initialize graphing
            UI_Graphview.Initialize();

            Data.InitializeRecording();
            try
            {
                SetUpTablesSources();
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError setting up tables sources.", ex);
            }
            MainWindow.SetProgramStatus(ProgramStatus.Idle);

            // Initialize colors
            InitializeColors();

            // Initialize scripts
            try
            {
                TableScripts.Initialize();
                TableScripts.Compile();
                Core.UI_Developer.Refresh();
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError initializing scripts.", ex);
            }

            // Update
            try
            {
                Update.Initialize();
                if (Core.Settings.AutocheckForUpdates) Update.CheckForUpdates(Core.Settings.UpdateLocations);
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning(ex.Message + "\n\nError initializing updating component.", ex);
            }

            // Autorun
            var autorun = cla.Skip(1).FirstOrDefault(x => File.Exists(x));
            if (autorun != null)
            {
                if (Path.GetExtension(autorun).ToLowerInvariant().EndsWith("aar"))
                    Core.Data.Load(autorun);
                else
                    Core.Data.LoadSavegame(autorun, false, true);
            }
        }

        /// <summary>
        /// Finalizes core define upon program exit.
        /// </summary>
        public static void FinalizeCore()
        {
            Core.UI_MapStats.Finish();

            // Save installed games
            Core.InstalledGames.Save(Path.Combine(Paths.AppData, "InstalledGames.xml"));

            // Look for non-existing scripts
            for (int i = Settings.DisabledScriptSets.Count - 1; i >= 0; i--)
            {
                if (TableScripts.ScriptSets.FirstOrDefault(x => x.Name == Settings.DisabledScriptSets[i]) == null)
                {
                    Settings.DisabledScriptSets.RemoveAt(i);
                }
            }

            // Save game settings
            Settings.SaveToFile(Settings, Path.Combine(Paths.AppData, "Configuration.xml"));

            Log.FinalizeLog();

            // Check if no other instances are open
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() <= 1)
            {
                Core.TryDelete(Core.Paths.Cache);
                Core.TryDelete(Core.Paths.Temp);
                Core.TryDelete(Core.Paths.ModTemp);
                //Core.TryDelete(Core.Paths.DynamicCompile);
            }

            // Update
            if (!IsClosingForUpdate && Core.Settings.NotifyAboutUpdates && Core.Update.UpdateStatus == UpdateStatus.UpdateAvailable)
            {
                if (System.Windows.Forms.MessageBox.Show("There is a new version of Chronicle (" + Update.UpdateVersion + ") available.\r\nDo you want to update now?\r\n\r\n(Note that you can turn off these notifications in Update options)", "Update available", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(Core.Update.UpdateLink);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.ReportError("Cannot open download link in a web browser.", ex);
                    }
                    finally
                    {
                        Process.GetCurrentProcess().CloseMainWindow();
                    }
                }
            }
        }

        public static void InitializeColors()
        {
            string colorspath = Path.Combine(Paths.Application, "Colors.xml");
            PresetColors.Clear();
            XmlDocument xml = new XmlDocument();
            xml.Load(colorspath);
            var nodes = xml.DocumentElement.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                try
                {
                    PresetColors.Add(node.SelectNodes("Name")[0].InnerText.ToLowerInvariant(), Table.CreateColor(
                        Byte.Parse(node.SelectNodes("R")[0].InnerText),
                        Byte.Parse(node.SelectNodes("G")[0].InnerText),
                        Byte.Parse(node.SelectNodes("B")[0].InnerText)));
                }
                catch (Exception ex)
                {
                    Core.Log.WriteWarning("Error loading color preset.", ex);
                }
            }
        }

        public static GameDate GetGameDate(InstalledGame game, string path)
        {
            bool compressed = IsCompressedSavegame(path);
            bool binary = IsBinarySavegame(path);
            string text = "";
            byte[] raw;

            if (compressed)
            {
                raw = ExtractFile(path, "meta");
                if (raw == null) // no meta file
                    raw = ExtractFile(path, "*" + Path.GetExtension(path), 1024);
            }
            else
            {
                raw = ReadRawBytes(path, 1024);
            }

            if (binary)
            {
                CEParser.BinaryFile file = new CEParser.BinaryFile(new MemoryStream(raw), game.Token, Encoding.GetEncoding("windows-1250"));
                if (game.Game.Token == "eu4") file.Decoder.EnforceDateDatatype = true;
                file.Parse();
                text = file.Export();
            }
            else
            {
                text = GetANSIString(raw);
            }

            return new GameDate(Core.ExtractDate(text));
        }

        private static string ExtractDate(string text)
        {
            // Find date="" construct
            int start = text.IndexOf("date");
            int length = 1;

            bool quoted = false;

            while (text[start + length] != '=')
            {
                length++;
            }

            while (true)
            {
                if (length > 20) break;
                if (quoted && text[start + length] == '\"') { length++; break; }
                if (!quoted && Char.IsLetter(text[start + length])) break;

                if (text[start + length] == '\"') quoted = true;
                length++;
            }

            return text.Substring(start, length).Trim();
        }

        public static bool IsCompressedSavegame(string path)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    byte[] code = br.ReadBytes(2);
                    return (code[0] == 80 && code[1] == 75);
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportError(string.Format(CultureInfo.InvariantCulture, "Error reading file: <" + path + ">."), ex);
                return false;
            }
        }

        public static bool IsBinarySavegame(string path)
        {
            if (IsCompressedSavegame(path))
            {
                try
                {
                    byte[] code = ExtractFile(path, "meta", 6);
                    if (code == null) // no meta file found
                    {
                        code = ExtractFile(path, "*" + Path.GetExtension(path), 6);
                        if (code == null) throw new Exception("Cannot extract file.");

                    }
                    return (code[3] == 98 && code[4] == 105 && code[5] == 110);
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError(string.Format(CultureInfo.InvariantCulture, "Error reading file: <" + path + ">."), ex);
                    return false;
                }
            }
            else
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        byte[] code = br.ReadBytes(6);
                        return (code[3] == 98 && code[4] == 105 && code[5] == 110);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError(string.Format(CultureInfo.InvariantCulture, "Error reading file: <" + path + ">."), ex);
                    return false;
                }
            }
        }

        public static byte[] ExtractFile(string path, string entry, int length)
        {
            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(path))
            {
                Ionic.Zip.ZipEntry e = zip.FirstOrDefault(x => Like(Path.GetFileName(x.FileName), entry));
                if (e == null) return null;
                MemoryStream ms = new MemoryStream();

                int failsafe = 3;
                while (true)
                {
                    try
                    {
                        e.Extract(ms);
                        break; // success!
                    }
                    catch (Exception ex)
                    {
                        Core.Log.WriteWarning("Error extracting from a compressed file: <" + path + ">.", ex);
                        if (--failsafe == 0) return new byte[0]; //failure
                        else Thread.Sleep(100);
                    }
                }
                byte[] output = new byte[length < 0 ? ms.Length : length];

                ms.Seek(0, SeekOrigin.Begin);
                using (BinaryReader br = new BinaryReader(ms))
                {
                    ms.Read(output, 0, output.Length);
                }
                return output;
            }
        }

        public static byte[] ExtractFile(string path, string entry)
        {
            return ExtractFile(path, entry, -1);
        }

        public static bool ZipContainsEntry(string path, string entry)
        {
            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(path))
            {
                Ionic.Zip.ZipEntry e = zip.FirstOrDefault(x => Like(Path.GetFileName(x.FileName), entry));
                return e != null;
            }
        }

        static bool Like(string expression, string mask)
        {
            var rx = new Regex("^" + Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", "."));
            return rx.IsMatch(expression);
        }

        public static byte[] ReadRawBytes(string path, int count)
        {
            using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                return count < 0 ? br.ReadBytes((int)br.BaseStream.Length) : br.ReadBytes(count);
            }
        }

        public static byte[] ReadRawBytes(string path)
        {
            return ReadRawBytes(path, -1);
        }

        public static string GetANSIString(byte[] raw)
        {
            return ANSI.GetString(raw);
        }

        public static bool TryDelete(string path)
        {
            return AttemptDelete(path, false);
        }

        public static bool TryCreateDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    return false;
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteWarning("Attempted creation of the following directory failed: <" + path + ">. Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Looks for a compatible installed game, based on savegame file location.
        /// </summary>
        /// <param name="path">Location of the savegame file</param>
        /// <returns>Installed game that is deemed compatible with the savegame.</returns>
        public static InstalledGame FindGameForSavegame(string path)
        {
            try
            {
                string ext = Path.GetExtension(path).Replace(".", "").ToLowerInvariant();
                string dir = Path.GetDirectoryName(path);

                if (Data.Game.Game.SavegameExtension == ext)
                    return Data.Game;

                return InstalledGames.GetBySavegameExtension(ext);
            }
            catch (Exception ex)
            {
                Core.Log.WriteError("Error locating compatible installed game.", ex);
                return null;
            }
        }

        public static bool AttemptDelete(string path, bool suppressErrors)
        {
            try
            {
                if (path.Contains("*"))
                {
                    bool deleted = false;
                    foreach (var file in new DirectoryInfo(Path.GetDirectoryName(path)).EnumerateFiles(Path.GetFileName(path)))
                    {
                        deleted = true;
                        file.Delete();
                    }
                    return deleted;
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (!suppressErrors)
                    Log.WriteWarning("Attempted removal of the following path failed: <" + path + ">. Exception: " + ex.Message);
                return false;
            }
        }

        public static void Debug(string name, object value)
        {
            if (!Core.Settings.DebugMode) return;
            if (UI_Debug == null) return;
            UI_Debug.SetProperty(name, value);
        }

        #region File list creation

        /// <summary>
        /// Generates a proper full path for a given definitions asset, taking into account that it may be modified by the mods.
        /// </summary>
        public static string GetPath(InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> paths = new List<string>();

            var p = Path.Combine(game.Directory, path);
            if (File.Exists(p)) paths.Add(p);
            p = Path.Combine(game.Directory, game.Game.GameFilesSubfolder, path);
            if (File.Exists(p)) paths.Add(p);
            if (mods != null)
            {
                foreach (var m in mods)
                {
                    if (m.Zipped)
                    {
                        ZipFile z = new ZipFile(m.ModDir);
                        ZipEntry e = z.FirstOrDefault(x => x.FileName == path);
                        if (e != null)
                        {
                            e.Extract(Path.Combine(Core.Paths.ModTemp, m.Name), ExtractExistingFileAction.OverwriteSilently);
                            p = Path.Combine(Core.Paths.ModTemp, m.Name, e.FileName);
                            if (File.Exists(p)) paths.Add(p);
                        }
                    }
                    else
                    {
                        p = Path.Combine(m.ModDir, path);
                        if (File.Exists(p)) paths.Add(p);
                    }
                }
            }

            return (paths.Count == 0) ? "" : paths.Last();
        }

        /// <summary>
        /// Generates full paths for a given definitions asset, taking into account that it may be modified by the mods.
        /// </summary>
        public static string[] GetPaths(InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> paths = new List<string>();

            if (Path.GetFileName(path).IndexOf("*") > -1)
            {
                var filter = Path.GetFileName(path);

                var p = Path.Combine(game.Directory, Path.GetDirectoryName(path));
                if (Directory.Exists(p)) paths.AddRange(Directory.GetFiles(p, filter));

                p = Path.Combine(game.Directory, game.Game.GameFilesSubfolder, Path.GetDirectoryName(path));
                if (Directory.Exists(p)) paths.AddRange(Directory.GetFiles(p, filter));

                if (mods != null)
                {
                    foreach (var m in mods)
                    {
                        p = Path.Combine(m.ModDir, Path.GetDirectoryName(path));
                        if (Directory.Exists(p)) paths.AddRange(Directory.GetFiles(p, filter));
                    }
                }
            }
            else
            {
                var p = Path.Combine(game.Directory, path);
                if (File.Exists(p)) paths.Add(p);

                p = Path.Combine(game.Directory, game.Game.GameFilesSubfolder, path);
                if (File.Exists(p)) paths.Add(p);

                if (mods != null)
                {
                    foreach (var m in mods)
                    {
                        p = Path.Combine(m.ModDir, path);
                        if (File.Exists(p)) paths.Add(p);
                        if (Directory.Exists(p)) paths.AddRange(Directory.GetFiles(p));
                    }
                }
            }

            // Look for conflicts
            for (int i = 0; i < paths.Count; i++)
            {
                var name = Path.GetFileName(paths[i]);
                for (int j = i + 1; j < paths.Count; j++)
                {
                    if (name == Path.GetFileName(paths[j]))
                    {
                        paths.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            return paths.ToArray();
        }

        #endregion

        public static bool IsSavegameIntervalTooSmall(GameDate timestamp, GameDate lastTimepoint, AutosaveFrequency intervalBetweenSaves)
        {
            if (lastTimepoint.IsEmpty) return false;

            if (intervalBetweenSaves == AutosaveFrequency.Default) intervalBetweenSaves = Data.Game.Game.DefaultMinAutosaveFrequency;
            TimeSpan span = timestamp - lastTimepoint;
            switch (intervalBetweenSaves)
            {
                case AutosaveFrequency.Default: return false;
                case AutosaveFrequency.Maximum: return false;
                case AutosaveFrequency.HalfMonth: return (span.TotalDays < 15);
                case AutosaveFrequency.OneMonth: return (span.TotalDays < 28);
                case AutosaveFrequency.TwoMonths: return (span.TotalDays < 59);
                case AutosaveFrequency.ThreeMonths: return (span.TotalDays < 90);
                case AutosaveFrequency.HalfYear: return (span.TotalDays < 181);
                case AutosaveFrequency.OneYear: return (span.TotalDays < 365);
                case AutosaveFrequency.ThreeYears: return (span.TotalDays < 365 * 3);
                case AutosaveFrequency.FiveYears: return (span.TotalDays < 365 * 5);
                case AutosaveFrequency.TenYears: return (span.TotalDays < 365 * 10);
            }
            return false;
        }

        public static void SetUpTablesSources()
        {
            if (Core.Settings.SupportedTableSources == null)
                Core.Settings.SupportedTableSources = new List<string>();
            if (!Directory.Exists("Script")) Directory.CreateDirectory("Script");
            var dirs = Directory.GetDirectories("Script");
            foreach (var dir in dirs)
            {
                if (Path.GetFileName(dir) == "Chronicle.Default") continue;
                if (!Core.Settings.SupportedTableSources.Contains(Path.GetFileName(dir))) Core.Settings.SupportedTableSources.Add(Path.GetFileName(dir));
            }
            if (Core.Settings.SupportedTableSources.Count > 0)
            {
                for (int i = Core.Settings.SupportedTableSources.Count - 1; i >= 0; i--)
                {
                    if (!Directory.Exists("Script\\" + Core.Settings.SupportedTableSources[i]))
                        Core.Settings.SupportedTableSources.RemoveAt(i);
                }
            }
            if (Core.Settings.SupportedTableSources.Count > 0)
            {
                if (!Core.Settings.SupportedTableSources.Contains("Chronicle.Default"))
                    Core.Settings.SupportedTableSources.Insert(Core.Settings.SupportedTableSources.Count - 1, "Chronicle.Default");
            }
            else
            {
                Core.Settings.SupportedTableSources.Add("Chronicle.Default");
            }
        }

        public static void SaveImage(ImageSource src)
        {
            // add the RenderTargetBitmap to a Bitmapencoder
            Core.Dispatch.DisplayProgress("Encoding bitmap...");
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)src));

            // save file to disk
            Core.Dispatch.DisplayProgress("Saving screenshot...");
            using (FileStream fs = File.Open(Core.SaveGFXDialog.FileName, FileMode.OpenOrCreate))
            {
                encoder.Save(fs);
            }
            Core.Log.Write("Map export to file: <" + Core.SaveGFXDialog.FileName + "> finished.");
        }

        public class Localization
        {
            public List<string> Strings { get; private set; } = new List<string>();

            public Localization(InstalledGame game, string[] paths)
            {
                List<string> consumedFilenames = new List<string>();
                Core.Log.Write("Reading localization strings...");

                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                    if (!File.Exists(path) || game.Game.IgnoredLocalizationFiles.Contains(Path.GetFileNameWithoutExtension(path))) continue;

                    if (game.Game.ClausewitzEngineVersion <= 2)
                    {
                        try
                        {
                            Strings.AddRange(File.ReadAllLines(path, Core.ANSI));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.WriteWarning("Could not load localization strings from file: <" + path + ">.", ex);
                            return;
                        }
                    }
                    else if (game.Game.ClausewitzEngineVersion >= 3)
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(path, System.Text.Encoding.GetEncoding(1250));
                            // We expect first line of file to define language
                            if (!lines[0].Contains("l_english")) continue;
                            Array.ForEach(lines, x => Strings.Add(x.Replace("\"", "")));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.WriteWarning("Could not load localization strings from file: <" + path + ">. ", ex);
                            return;
                        }
                    }

                    consumedFilenames.Add(Path.GetFileName(path));
                }
            }
        }

    }

    public enum ProgramStatus { Idle, Recording }
}
