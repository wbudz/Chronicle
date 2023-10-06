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
    /// Interaction logic for UI_Mods.xaml
    /// </summary>
    public partial class UI_Mods : UserControl
    {
        public UI_Mods()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Core.DesignMode) return;

            RefreshGames();

            cGames.SelectedIndex = Math.Min(0, cGames.Items.Count - 1);
        }

        private void RefreshGames()
        {
            cGames.ItemsSource = Core.InstalledGames.GetList();
        }

        private void cGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cGames.SelectedIndex == -1)
            {
                rAutomaticallySelectMods.IsChecked = true;
                lMods.ItemsSource = null;
                return;
            }

            rAutomaticallySelectMods.IsChecked = Core.InstalledGames[cGames.SelectedIndex].AutomaticallySelectMods;
            rLoadSelectedMods.IsChecked = !Core.InstalledGames[cGames.SelectedIndex].AutomaticallySelectMods;

            lMods.ItemsSource = Core.InstalledGames[cGames.SelectedIndex].Mods;

            for (int i = 0; i < lMods.Items.Count; i++)
            {
                if (Core.InstalledGames[cGames.SelectedIndex].ManuallyEnabledMods.Contains(lMods.Items[i].ToString()))
                    lMods.SelectedItems.Add(lMods.Items[i]);
            }
        }

        private void lMods_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if ((e.IsSelected) && (!Core.InstalledGames[cGames.SelectedIndex].ManuallyEnabledMods.Contains(e.Item.ToString())))
                Core.InstalledGames[cGames.SelectedIndex].ManuallyEnabledMods.Add(e.Item.ToString());

            if ((!e.IsSelected) && (Core.InstalledGames[cGames.SelectedIndex].ManuallyEnabledMods.Contains(e.Item.ToString())))
                Core.InstalledGames[cGames.SelectedIndex].ManuallyEnabledMods.Remove(e.Item.ToString());
        }

        private void rAutomaticallySelectMods_Checked(object sender, RoutedEventArgs e)
        {
            Core.InstalledGames[cGames.SelectedIndex].AutomaticallySelectMods = rAutomaticallySelectMods.IsChecked == true;
        }

        private void rLoadSelectedMods_Checked(object sender, RoutedEventArgs e)
        {
            Core.InstalledGames[cGames.SelectedIndex].AutomaticallySelectMods = rLoadSelectedMods.IsChecked != true;
        }
    }
}
