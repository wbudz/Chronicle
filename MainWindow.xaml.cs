using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        DispatcherTimer CacheTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            if (Core.DesignMode) return;

            Core.MainWindow = this;

            Core.UI_Mapview = UI_Mapview;
            Core.UI_Tableview = UI_Tableview;
            Core.UI_Graphview = UI_Graphview;

            Core.UI_Debug = UI_Debug;
            Core.UI_Log = UI_Log;
            Core.UI_Developer = UI_Developer;

            Core.UI_MapControl = UI_MapControl;
            Core.UI_MapStats = UI_MapStats;
            Core.UI_MapColors = UI_MapColors;

            Core.UI_TableSettings = UI_TableSettings;

            Core.UI_GraphSettings = UI_GraphSettings;
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.InitializeLog();
                Core.UI_Log.InitializeLog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n\nError initializing log. The program will now exit.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            try
            {
                Core.InitializeCore();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nInitialization error. The program will now exit.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            CacheTimer.Interval = Core.Settings.CacheInterval;
            CacheTimer.Tick += CacheTimer_Tick;
            CacheTimer.Start();

            if (Core.UI_Mapview.iMap.Source == null)
            {
                try
                { 
                Core.UI_Mapview.InitializeMap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\nError initializing map. The program will now exit.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }

            }
            SetPaneVisibility();
        }

        public void SetPaneVisibility()
        {
            LocalDock.Visibility = Core.Settings.LeftPaneVisible ? Visibility.Visible : Visibility.Collapsed;
            LocalDockSplitter.Visibility = Core.Settings.LeftPaneVisible ? Visibility.Visible : Visibility.Collapsed;
            GlobalDock.Visibility = Core.Settings.RightPaneVisible ? Visibility.Visible : Visibility.Collapsed;
            GlobalDockSplitter.Visibility = Core.Settings.LeftPaneVisible ? Visibility.Visible : Visibility.Collapsed;

            if (Core.Settings.LeftPaneVisible)
            {
                MainGrid.ColumnDefinitions[0].MinWidth = 160;
                MainGrid.ColumnDefinitions[0].Width = new GridLength(360);
                MainGrid.ColumnDefinitions[0].MaxWidth = 500;
                MainGrid.ColumnDefinitions[1].Width = new GridLength(5);
            }
            else
            {
                MainGrid.ColumnDefinitions[0].MinWidth = 0;
                MainGrid.ColumnDefinitions[0].Width = new GridLength(0);
                MainGrid.ColumnDefinitions[0].MaxWidth = 0;
                MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }

            if (Core.Settings.RightPaneVisible)
            {
                MainGrid.ColumnDefinitions[4].MinWidth = 160;
                MainGrid.ColumnDefinitions[4].Width = new GridLength(360);
                MainGrid.ColumnDefinitions[4].MaxWidth = 500;
                MainGrid.ColumnDefinitions[3].Width = new GridLength(5);
            }
            else
            {
                MainGrid.ColumnDefinitions[4].MinWidth = 0;
                MainGrid.ColumnDefinitions[4].Width = new GridLength(0);
                MainGrid.ColumnDefinitions[4].MaxWidth = 0;
                MainGrid.ColumnDefinitions[3].Width = new GridLength(0);
            }

            Core.UI_MapControl.RefreshMinimap();
        }

        private void CacheTimer_Tick(object sender, EventArgs e)
        {
            if (Core.Data.Tables != null && !Core.CacheCollectionPaused)
            {
                Task.Run(() => Core.Data.Tables.CollectCache());
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Ribbon.Backstage.IsOpen) Ribbon.CloseBackstage();
        }

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Core.UI_Mapview.RefreshViewpoint();
        }

        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
            Core.FinalizeCore();
        }

        private void RibbonWindow_ContentRendered(object sender, EventArgs e)
        {
            if (!Core.FullyLoaded)
            {
                Core.FullyLoaded = true;
                Ribbon.SelectTab(Ribbon.MapRibbonTab);
            }
        }

        public void SetProgramStatus(ProgramStatus status)
        {
            switch (status)
            {
                case ProgramStatus.Idle:
                    iStatusIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/stop-16.png"));
                    lStatusCaption.Content = "Not recording";
                    break;
                case ProgramStatus.Recording:
                    iStatusIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/record-16.png"));
                    lStatusCaption.Content = "Recording";
                    break;
            }
        }

        public void SetGameInformation(InstalledGame game)
        {
            try
            {
                Core.MainWindow.iGameIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/GameIcons/" + game.Token + "-16.png"));
                Core.MainWindow.lGameName.Content = game.Game.Name;
                Core.MainWindow.iGameIcon.Visibility = System.Windows.Visibility.Visible;
                Core.MainWindow.lGameName.Visibility = System.Windows.Visibility.Visible;

                if (Core.Data.Defs.Mods == null || Core.Data.Defs.Mods.Length < 1)
                {
                    Core.MainWindow.lModName.Content = "(no mods)";
                    Core.MainWindow.lModName.ToolTip = null;
                }
                else if (Core.Data.Defs.Mods.Length == 1)
                {
                    Core.MainWindow.lModName.Content = Core.Data.Defs.Mods[0].Name;
                    Core.MainWindow.lModName.ToolTip = null;
                }
                else
                {
                    var primaryMod = Core.Data.Defs.GetPrimaryMod();
                    if (primaryMod != null && game.AutomaticallySelectMods)
                    {
                        Core.MainWindow.lModName.Content = primaryMod.Name + " (and others)";
                    }
                    else
                    {
                        Core.MainWindow.lModName.Content = "(multiple mods)";
                    }
                    string modsTooltip = "Active mods:\n";
                    for (int i = 0; i < Core.Data.Defs.Mods.Length; i++)
                    {
                        modsTooltip += Core.Data.Defs.Mods[i].Name + "\n";
                    }
                    Core.MainWindow.lModName.ToolTip = modsTooltip;
                }
                Core.MainWindow.lModName.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                Core.Log.WriteWarning("Error refreshing game data in the statusbar.", ex);
                Core.MainWindow.iGameIcon.Visibility = System.Windows.Visibility.Collapsed;
                Core.MainWindow.lGameName.Visibility = System.Windows.Visibility.Collapsed;
                Core.MainWindow.lModName.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void pRecordingStatus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Core.Data.Game == null) return;
            if (Core.Data.IsRecording)
            {
                if (MessageBox.Show("Do you want to stop the current recording session?", "Recording", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Core.Data.StopRecording();
            }
            else
            {
                if (MessageBox.Show("Do you want to start recording savegames?", "Recording", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Core.Data.StartRecording();
            }
        }
    }
}
