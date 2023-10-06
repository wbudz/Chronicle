using System;
using System.Collections.Generic;
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

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_GameWindow.xaml
    /// </summary>
    public partial class UI_GameWindow : Window
    {
        InstalledGame game;

        public UI_GameWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bGame.Items.Clear();
            string[] games = Game.GetGamesList();
            Array.ForEach(games, g => bGame.Items.Add(g));

            bAutosaveFrequency.Items.Clear();
            string[] freqs = Game.GetAutosaveFrequenciesList();
            Array.ForEach(freqs, i => bAutosaveFrequency.Items.Add(i));
            bAutosaveFrequency.SelectedIndex = 0;

            bGameListPriority.Items.Clear();
            bGameListPriority.Items.Add("Higher");
            bGameListPriority.Items.Add("Normal");
            bGameListPriority.Items.Add("Lower");
            bGameListPriority.SelectedIndex = 1;
        }

        public void Initialize(InstalledGame game)
        {
            this.game = game;

            Title = (game.Directory == "") ? "Add game" : "Edit game";
            tPath.Text = game.Directory;
            tName.Text = game.Name;
            bGame.SelectedIndex = Core.Games.FindIndex(x => x.Token == game.Token);
            bAutosaveFrequency.SelectedIndex = (int)game.IntervalBetweenSaves;
            bGameListPriority.SelectedIndex = game.ListPriority;
        }

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tPath.Text = fbd.SelectedPath;
            }

            // Autoselect game type and name
            if (Directory.Exists(tPath.Text))
            {
                if (bGame.SelectedIndex < 0) bGame.SelectedIndex = Core.Games.FindIndex(x => File.Exists(Path.Combine(tPath.Text, x.EXEName)));
                if (tName.Text.Trim() == "") tName.Text = Core.Games.Find(x => File.Exists(Path.Combine(tPath.Text, x.EXEName))).Name;
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            // Check
            if (bGame.SelectedIndex < 0)
            {
                Core.Dispatch.DisplayMessageBox("Game type not specified for the new installed game.", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            if (tName.Text.Trim() == "")
            {
                Core.Dispatch.DisplayMessageBox("You did not specify name for the game. Default value will be used.", "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                tName.Text = Core.Games[bGame.SelectedIndex].Name;
            }
            if (!Directory.Exists(tPath.Text))
            {
                Core.Dispatch.DisplayMessageBox("Specified path does not exist.", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(Path.Combine(tPath.Text, Core.Games[bGame.SelectedIndex].EXEName)))
            {
                Core.Dispatch.DisplayMessageBox("Specified path does not contain expected game EXE file.", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            // Set properties
            game.Token = Core.Games[bGame.SelectedIndex].Token;
            game.Name = tName.Text;
            game.Directory = tPath.Text;
            game.IntervalBetweenSaves = (AutosaveFrequency)bAutosaveFrequency.SelectedIndex;
            game.ListPriority = bGameListPriority.SelectedIndex;

            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
