using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public static partial class Core
    {
        public static class Paths
        {
            /// <summary>
            /// Contains path to directory when program's EXE is located.
            /// </summary>
            public static string Application
            {
                get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
            }

            /// <summary>
            /// Defines program's %appdata% (Roaming) location.
            /// </summary>
            public static string AppData
            {
                get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chronicle"); }
            }

            /// <summary>
            /// Defines program's %appdata%\..\Local location.
            /// </summary>
            public static string LocalAppData
            {
                get { return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppData)), "Local", "Chronicle"); }
            }

            /// <summary>
            /// Defines main Steam folder path.
            /// </summary>
            public static string Steam { get; private set; }

            /// <summary>
            /// Defines all Steam folders.
            /// </summary>
            public static List<string> SteamFolders { get; private set; }

            /// <summary>
            /// Steam ID
            /// </summary>
            public static string SteamID { get; private set; }

            /// <summary>
            /// Defines folder for creating AAR files.
            /// </summary>
            public static string Temp
            {
                get { return Path.Combine(LocalAppData, "Temp"); }
            }

            public static string ModTemp
            {
                get { return Path.Combine(LocalAppData, "ModTemp"); }
            }

            public static string RandomNewWorldTemp
            {
                get { return Path.Combine(LocalAppData, "RNWTemp"); }
            }

            public static string Cache
            {
                get { return Path.Combine(LocalAppData, "Cache"); }
            }

            /// <summary>
            /// Defines folder for creating temporary dynamic script assemblies.
            /// </summary>
            public static string DynamicCompile
            {
                get { return Path.Combine(AppData, "Dynamic"); }
            }

            public static void SetSteamFolderPath()
            {
                try
                {
                    string userid = "";

                    RegistryKey key = Registry.CurrentUser;
                    key = key.OpenSubKey("Software");
                    key = key.OpenSubKey("Valve");
                    key = key.OpenSubKey("Steam");
                    Steam = key.GetValue("SteamPath", "").ToString().Replace('/', '\\');

                    SteamFolders = new List<string>();
                    SteamFolders.Add(Steam);

                    // Get alternative install folders
                    try
                    {
                        string[] steamSettingsLines = File.ReadAllLines(Path.Combine(Steam, "config", "config.vdf"));
                        var steamFolderSettings = steamSettingsLines.Where(x => x.Contains("BaseInstallFolder")).ToList();
                        foreach (var folder in steamFolderSettings)
                        {
                            var f = folder.Substring(folder.LastIndexOf("\t"));
                            f = f.Replace("\"", "");
                            f = f.Replace("\t", "");
                            f = f.Replace("\\\\", "\\");
                            if (Directory.Exists(f)) SteamFolders.Add(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.WriteWarning("Error getting Steam settings (Steam is possibly not installed): " + ex.Message);
                    }

                    // Add Steam ID
                    key = key.OpenSubKey("Users");
                    if (key != null && key.GetSubKeyNames().Length > 0)
                    {
                        userid = key.GetSubKeyNames()[0];
                    }
                    else
                    {
                        Core.Log.WriteWarning("Error getting Steam user from the Registry (Steam is possibly not installed).");
                        try
                        {
                            var ids = Directory.GetDirectories(Path.Combine(Steam, "userdata"));
                            userid = Path.GetFileName(ids[0]);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.WriteWarning("Error guessing Steam user name (Steam is possibly not installed): " + ex.Message);
                        }
                    }

                    if (Core.Settings.EnforcedSteamID != null && Core.Settings.EnforcedSteamID != "")
                    {
                        SteamID = Core.Settings.EnforcedSteamID;
                    }
                    else
                    {
                        SteamID = userid;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.WriteWarning("Error getting Steam settings (Steam is possibly not installed): " + ex.Message);
                }
            }
        }
    }
}
