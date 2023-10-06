using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_GraphSettings.xaml
    /// </summary>
    [Serializable]
    public partial class UI_GraphSettings : UserControl
    {
        public UI_GraphSettings()
        {
            InitializeComponent();
        }

        List<string> provs;
        List<string> countries;

        public void EnforcedListsRefresh()
        {
            // Initialize graph check lists
            provs = Core.Data.Defs.Provinces.List("({0}) {1}").ToList();
            countries = Core.Data.Defs.Countries.List("{1} ({0})").ToList();
            if (countries.Count > 0) countries.RemoveAt(0);
            for (int i = 0; i < Core.Data.Game.Game.SpecialCountryTags.Length; i++)
            {
                countries.RemoveAll(x => x.IndexOf("(" + Core.Data.Game.Game.SpecialCountryTags[i] + ")") > -1);
            }

            bEnforcedCountries.ItemsSource = countries;
            bEnforcedProvinces.ItemsSource = provs;
        }

        private void GraphOptionsChange(object sender, SelectionChangedEventArgs e)
        {
            if (Core.DesignMode || Core.UI_Graphview == null) return;
            Core.Settings.GraphSeriesCount = bDisplayedDataSeries.SelectedIndex + 1;
            Core.Settings.GraphSeriesSelectionMethod = bSeriesSelection.SelectedIndex;            
            Core.UI_Graphview.Refresh();
        }

        private void GraphOptionsChange(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if (Core.DesignMode || Core.UI_Graphview == null) return;
            Core.Settings.GraphSeriesCount = bDisplayedDataSeries.SelectedIndex + 1;
            Core.Settings.GraphSeriesSelectionMethod = bSeriesSelection.SelectedIndex;
            Core.UI_Graphview.Refresh();
        }

        private void bClearEnforcedProvinces_Click(object sender, RoutedEventArgs e)
        {
            bEnforcedProvinces.SelectedItems.Clear();
        }

        private void bClearEnforcedCountries_Click(object sender, RoutedEventArgs e)
        {
            bEnforcedCountries.SelectedItems.Clear();
        }

        public IEnumerable<ushort> GetEnforcedProvinces()
        {
            for (ushort i = 0; i < bEnforcedProvinces.Items.Count; i++)
            {
                if (bEnforcedProvinces.SelectedItems.Contains(bEnforcedProvinces.Items[i]))
                    yield return i;
            }
        }

        public IEnumerable<ushort> GetEnforcedCountries()
        {
            for (ushort i = 0; i < bEnforcedCountries.Items.Count; i++)
            {
                if (bEnforcedCountries.SelectedItems.Contains(bEnforcedCountries.Items[i]))
                    yield return (ushort)(i + 1);
            }
        }
    }
}
