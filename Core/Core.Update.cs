using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public static partial class Core
    {
        public static class Update
        {
            static WebClient client;

            public static UpdateStatus UpdateStatus { get; set; }
            public static string UpdateVersion { get; set; }
            public static string UpdateLink { get; set; }
            public static string UpdateChangelog { get; set; }

            public static void CheckForUpdates(List<string> locations)
            {
                Task.Run(() =>
                {
                    string us = null;

                    foreach (var url in locations)
                    {
                        UpdateStatus = UpdateStatus.Checking;
                        Core.Dispatch.Run(() => MainWindow.Ribbon.UI_Update.RefreshUpdateInfo(UpdateStatus, null, null, null));
                        try
                        {
                            us = client.DownloadString(new Uri(url));
                            //autoUpdate = auto;
                            //WebClient w = new WebClient();
                            //w.DownloadStringCompleted += W_DownloadStringCompleted;
                            //w.DownloadStringAsync(new Uri("http://codeofwar.wbudziszewski.pl/ironmelt/"), auto);
                            InterpretUpdateString(us);
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus = UpdateStatus.Failure;
                            Core.Log.WriteWarning("Error checking for updates at URL: <" + url + ">.", ex);
                        }
                        if (UpdateVersion != null && UpdateLink != null) break;
                    }

                    try
                    {
                        if (int.Parse(UpdateVersion.Split('.')[3]) > Core.Version.Revision)
                            UpdateStatus = UpdateStatus.UpdateAvailable;
                        else
                            UpdateStatus = UpdateStatus.NoUpdates;
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus = UpdateStatus.Failure;
                        Core.Log.WriteWarning("Error checking for updates.", ex);
                    }

                    Core.Dispatch.Run(() => MainWindow.Ribbon.UI_Update.RefreshUpdateInfo(UpdateStatus, UpdateVersion, UpdateLink, UpdateChangelog));
                });
            }

            static void InterpretUpdateString(string us)
            {
                try
                {
                    var win = MainWindow.Ribbon.UI_Update;

                    if (us == "" || us == null) throw new Exception("Checking for updates failed. Update string is empty.");

                    // Update string format:
                    // One line for version number
                    // One line for update link
                    // Rest of the file: changelog
                    
                    string version = us.Substring(0, us.IndexOf("\n")).Trim();
                    us = us.Substring(version.Length + 1).Trim();
                    string link = us.Substring(0, us.IndexOf("\n")).Trim();
                    us = us.Substring(link.Length + 1).Trim();

                    UpdateVersion = version;
                    UpdateLink = link;
                    UpdateChangelog = us;
                }
                catch (Exception ex)
                {
                    Core.Log.WriteWarning("Checking for updates failed.", ex);
                    UpdateStatus = UpdateStatus.Failure;
                    UpdateVersion = null;
                    UpdateLink = null;
                    UpdateChangelog = null;
                }
            }

            public static void Initialize()
            {
                client = new WebClient();
                client.Headers.Add("user-agent", "Chronicle");
                UpdateStatus = UpdateStatus.Unspecified;
            }
        }
    }

    public enum UpdateStatus { Unspecified, Checking, Failure, NoUpdates, UpdateAvailable }
}
