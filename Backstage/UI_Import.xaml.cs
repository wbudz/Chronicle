using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Import.xaml
    /// </summary>
    public partial class UI_Import : UserControl
    {
        InstalledGames games;
        int _gameSelectedIndex;
        int gameSelectedIndex
        {
            get
            {
                return _gameSelectedIndex;
            }
            set
            {
                _gameSelectedIndex = value;
                bEditGame.IsEnabled = _gameSelectedIndex > -1;
                bRemoveGame.IsEnabled = _gameSelectedIndex > -1;
            }
        }

        InstalledGame selectedGame
        {
            get
            {
                if (gameSelectedIndex < 0 || gameSelectedIndex >= pGames.Children.Count) return null;
                return (pGames.Children[gameSelectedIndex] as Button).Tag as InstalledGame;
            }
        }

        public UI_Import()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Core.DesignMode) return;

            gameSelectedIndex = -1;
            lSavegames.ItemsSource = null;
            InitializeGames();
            RefreshGames();
        }

        public void SetInterfaceAvailability()
        {
            bStartRecording.IsEnabled = (Core.Data.Game != null && !Core.Data.IsRecording);
            bStopRecording.IsEnabled = (Core.Data.IsRecording);
            lRecording.Content = Core.Data.IsRecording ? "Savegame folders are being monitored and savegame data is recorded." : "No savegame folders are currently being monitored.";

            bLoadSavegameFromDisk.IsEnabled = selectedGame != null;
            bImportSavegameFromDisk.IsEnabled = selectedGame != null;
        }

        public void Commit()
        {
            Core.InstalledGames = games;
        }

        public void InitializeGames()
        {
            games = new InstalledGames(Core.InstalledGames);
        }

        private void RefreshGames()
        {
            if (gameSelectedIndex > games.Count)
            {
                gameSelectedIndex = -1;
                lSavegames.ItemsSource = null;
            }
            var gamesControls = games.PrepareControls(this, gameSelectedIndex);
            pGames.Children.Clear();
            lNoGamesDefined.Visibility = gamesControls.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            Array.ForEach(gamesControls, i => pGames.Children.Add(i));
        }

        public async void GameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (gameSelectedIndex == (sender as Button).TabIndex) return;

            gameSelectedIndex = (sender as Button).TabIndex;

            for (int i = 0; i < pGames.Children.Count; i++)
            {
                games.SetSelectedStatus((pGames.Children[i] as Button), gameSelectedIndex == (pGames.Children[i] as Button).TabIndex);
            }

            // Load saved games
            InstalledGame game = selectedGame;
            await Task.Run(() => Core.Data.EnumerateSavegames(game, lSavegames));

            bLoadSavegameFromDisk.IsEnabled = selectedGame != null;
            bImportSavegameFromDisk.IsEnabled = selectedGame != null;

            // Load the map etc.
            await Task.Run(() =>
            {
                Core.Data.LoadGame(game, Core.UI_Mapview);
            });

            SetInterfaceAvailability();
        }

        private void bAddGame_Click(object sender, RoutedEventArgs e)
        {
            InstalledGame game = new InstalledGame();
            UI_GameWindow wnd = new UI_GameWindow();
            wnd.Initialize(game);
            if (wnd.ShowDialog() == true)
            {
                games.Add(game, false);
            }
            RefreshGames();
        }

        private void bEditGame_Click(object sender, RoutedEventArgs e)
        {
            if (gameSelectedIndex < 0) return;
            InstalledGame game = new InstalledGame(games[gameSelectedIndex]);
            UI_GameWindow wnd = new UI_GameWindow();
            wnd.Initialize(game);
            if (wnd.ShowDialog() == true)
            {
                games.Edit(game, gameSelectedIndex);
            }
            RefreshGames();
        }

        private void bRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if (gameSelectedIndex < 0) return;
            if (System.Windows.Forms.MessageBox.Show("Do you want to remove selected game: <" + games[gameSelectedIndex].Name + ">? No actual game files will be deleted.", "Question",
                System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                games.RemoveAt(gameSelectedIndex);
                gameSelectedIndex = -1;
            }
            RefreshGames();
        }

        private void bImport_Click(object sender, RoutedEventArgs e)
        {
            if (Core.Paths.Steam == null || !Directory.Exists(Core.Paths.Steam))
            {
                Core.Log.ReportError("Steam path is not defined in the Registry or not correct. Check if Steam is installed properly.");
                return;
            }

            // C:\Program Files (x86)\Steam\steamapps
            //HKEY_CURRENT_USER\Software\Valve\Steam

            try
            {
                int importedCount = 0;
                // Look for manifests
                foreach (var path in Core.Paths.SteamFolders)
                {
                    string steamPath = Path.Combine(path, "steamapps", "common");

                    string[] dirs = Directory.GetDirectories(steamPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var dir in dirs)
                    {
                        foreach (var game in Core.Games)
                        {
                            if (File.Exists(Path.Combine(dir, game.EXEName)))
                            {
                                importedCount += games.Add(new InstalledGame(game.Token, game.Name, dir), true) ? 1 : 0;
                            }
                        }
                    }
                }

                if (importedCount > 0)
                {
                    Core.Dispatch.DisplayMessageBox("Successfully imported " + importedCount + " game(s).", "Import",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    Core.Log.Write("Successfully imported " + importedCount + " games from Steam.");
                }
                else
                {
                    Core.Dispatch.DisplayMessageBox("No games to be imported from Steam were found. You can still add game folder manually.", "Import",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    Core.Log.Write("No games to be imported from Steam.");
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning("Importing games from Steam failed.", ex);
            }
            finally
            {
                RefreshGames();
            }
        }

        private void bRevert_Click(object sender, RoutedEventArgs e)
        {
            InitializeGames();
            RefreshGames();
        }

        private void SortableListViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            ((SortableListView)sender).GridViewColumnHeaderClicked(e.OriginalSource as GridViewColumnHeader);
        }

        private async void bLoadSavegame_Click(object sender, RoutedEventArgs e)
        {
            if (lSavegames.SelectedItem == null) return;
            Core.MainWindow.Ribbon.Backstage.IsOpen = false;
            string path = ((SavegameEntry)(lSavegames.SelectedItem)).Path;
            await Task.Run(() =>
            {
                Core.Data.LoadSavegame(path, false, true);
            });
        }

        private async void bImportSavegame_Click(object sender, RoutedEventArgs e)
        {
            if (lSavegames.SelectedItem == null) return;
            Core.MainWindow.Ribbon.Backstage.IsOpen = false;
            string path = ((SavegameEntry)(lSavegames.SelectedItem)).Path;
            await Task.Run(() =>
            {
                Core.Data.LoadSavegame(path, true, true);
            });
        }

        private async void bLoadSavegameFromDisk_Click(object sender, RoutedEventArgs e)
        {
            Core.OpenDialog.Filter = selectedGame.Game.OpenDialogFileFilter + "|All files (*.*)|*.*";

            if (Core.OpenDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(Core.OpenDialog.FileName).ToLowerInvariant().EndsWith("aar"))
                    await Task.Run(() => Core.Data.Load(Core.OpenDialog.FileName));
                else
                    await Task.Run(() => Core.Data.LoadSavegame(Core.OpenDialog.FileName, false, true));
                Core.Data.CurrentFile = Core.OpenDialog.FileName;
                Core.Data.UnsavedChanges = false;
            }
        }

        private async void bImportSavegameFromDisk_Click(object sender, RoutedEventArgs e)
        {
            Core.OpenDialog.Filter = selectedGame.Game.OpenDialogFileFilter + "|All files (*.*)|*.*";

            if (Core.OpenDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(Core.OpenDialog.FileName).ToLowerInvariant().EndsWith("aar"))
                    await Task.Run(() => Core.Data.Load(Core.OpenDialog.FileName));
                else
                    await Task.Run(() => Core.Data.LoadSavegame(Core.OpenDialog.FileName, true, true));
                Core.Data.CurrentFile = Core.OpenDialog.FileName;
                Core.Data.UnsavedChanges = false;
            }
        }

        private void lSavegames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bImportSavegame.IsEnabled = lSavegames.SelectedIndex >= 0;
            bLoadSavegame.IsEnabled = lSavegames.SelectedIndex >= 0;
            bDeleteSavegame.IsEnabled = lSavegames.SelectedIndex >= 0;
        }

        private async void lSavegames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lSavegames.SelectedItem == null) return;
            Core.MainWindow.Ribbon.Backstage.IsOpen = false;
            string path = ((SavegameEntry)(lSavegames.SelectedItem)).Path;
            await Task.Run(() =>
            {
                Core.Data.LoadSavegame(path, false, true);
            });
        }

        private void bDeleteSavegame_Click(object sender, RoutedEventArgs e)
        {
            if (lSavegames.SelectedItem == null) return;
            if (System.Windows.Forms.MessageBox.Show("Do you want to delete selected savegame file?", "Question",
                System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                return;
            try
            {
                File.Delete(((SavegameEntry)(lSavegames.SelectedItem)).Path);
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error deleting selected savegame file.", ex);
            }
        }

        private void bStartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (Core.Data.Game == null) return;
            Core.Data.StartRecording();
            SetInterfaceAvailability();
        }

        private void bStopRecording_Click(object sender, RoutedEventArgs e)
        {
            if (Core.Data.Game == null) return;
            Core.Data.StopRecording();
            SetInterfaceAvailability();
        }
    }

    public class GameDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return ((GameDate)value).GetString("yyyy-MM-dd");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return new GameDate(0, 0, 0, 0);
        }
    }
}
