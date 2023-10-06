using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Update.xaml
    /// </summary>
    public partial class UI_Update : UserControl
    {
        public UI_Update()
        {
            InitializeComponent();
        }

        public void RefreshUpdateInfo(UpdateStatus us, string version, string link, string changelog)
        {
            var bc = new BrushConverter();
            switch (us)
            {
                case UpdateStatus.Unspecified:
                    gUpdateBackground.Background = (Brush)bc.ConvertFrom("#00000000");
                    lUpdateInfo.Content = "No check for updates made.";
                    bDownload.IsEnabled = false;
                    tChangelog.Visibility = Visibility.Collapsed;
                    tChangelog.Text = "";
                    break;
                case UpdateStatus.Checking:
                    gUpdateBackground.Background = (Brush)bc.ConvertFrom("#00000000");
                    lUpdateInfo.Content = "Checking for updates...";
                    bDownload.IsEnabled = false;
                    tChangelog.Visibility = Visibility.Collapsed;
                    tChangelog.Text = "";
                    break;
                case UpdateStatus.Failure:
                    gUpdateBackground.Background = (Brush)bc.ConvertFrom("#3FFF0000");
                    lUpdateInfo.Content = "Checking for updates failed.\nCheck your internet connection or try to access the download page manually.";
                    bDownload.IsEnabled = false;
                    tChangelog.Visibility = Visibility.Collapsed;
                    tChangelog.Text = "";
                    break;
                case UpdateStatus.NoUpdates:
                    gUpdateBackground.Background = (Brush)bc.ConvertFrom("#3F00A000");
                    lUpdateInfo.Content = "No update found.";
                    bDownload.IsEnabled = false;
                    tChangelog.Visibility = Visibility.Collapsed;
                    tChangelog.Text = "";
                    break;
                case UpdateStatus.UpdateAvailable:
                    gUpdateBackground.Background = (Brush)bc.ConvertFrom("#3FFFFF00");
                    lUpdateInfo.Content = "There is a new version of Chronicle (" + version + ") available.\n\nThe program will close before beginning of the update process.";
                    bDownload.IsEnabled = true;
                    tChangelog.Text = changelog;
                    tChangelog.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void bCheckAgain_Click(object sender, RoutedEventArgs e)
        {
            Core.Update.CheckForUpdates(Core.Settings.UpdateLocations);
        }

        private void bDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.IsClosingForUpdate = true;
                Process.Start(Core.Update.UpdateLink);
                Process.GetCurrentProcess().CloseMainWindow();
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Cannot open download link in a web browser.", ex);
            }
        }

        private void UI_Update_Loaded(object sender, RoutedEventArgs e)
        {
            if (Core.Settings.NotifyAboutUpdates)
                rNotifyOfUpdates.IsChecked = true;
            else if (Core.Settings.AutocheckForUpdates)
                rAutocheckForUpdates.IsChecked = true;
            else
                rDontCheckForUpdates.IsChecked = true;
            RefreshUpdateInfo(UpdateStatus.NoUpdates, null, null, null);
        }

        private void rNotifyOfUpdates_Checked(object sender, RoutedEventArgs e)
        {
            if (rNotifyOfUpdates.IsChecked == true)
            {
                Core.Settings.NotifyAboutUpdates = true;
                Core.Settings.AutocheckForUpdates = true;
            }
            else if (rAutocheckForUpdates.IsChecked == true)
            {
                Core.Settings.NotifyAboutUpdates = false;
                Core.Settings.AutocheckForUpdates = true;
            }
            else
            {
                Core.Settings.NotifyAboutUpdates = false;
                Core.Settings.AutocheckForUpdates = false;
            }
        }
    }
}
