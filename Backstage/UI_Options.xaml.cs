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

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Options.xaml
    /// </summary>
    public partial class UI_Options : UserControl
    {
        Settings settings;

        public UI_Options()
        {
            InitializeComponent();
        }

        public void SetSettings(Settings original)
        {
            settings = Settings.Clone(original);
            RefreshOptions();
        }

        public void RefreshOptions()
        {
            switch (settings.LabelsDisplayMode)
            {
                case 1: rProvinceLabelsIDs.IsChecked = true; break;
                case 2: rProvinceLabelsNames.IsChecked = true; break;
                case 3: rProvinceLabelsValues.IsChecked = true; break;
                default: rProvinceLabelsNone.IsChecked = true; break;
            }
            cLabelShadows.IsChecked = settings.LabelsShadows;

            cDisplayCaptionBar.IsChecked = settings.GIFDisplayCaptionBar;
            cDisplayDate.IsChecked = settings.GIFDisplayDate;
            cDisplayMapmode.IsChecked = settings.GIFDisplayMapmode;
            cDisplayLogo.IsChecked = settings.GIFDisplayLogo;

            BestEntitiesCountSlider.Value = settings.GraphSeriesCount;
            BestEntitiesSetup.SelectedIndex = settings.GraphSeriesSelectionMethod;

            rGraphExportSizeWindow.IsChecked = settings.GraphExportWidth <= 0 || settings.GraphExportHeight <= 0;
            rGraphExportSizeCustom.IsChecked = settings.GraphExportWidth > 0 && settings.GraphExportHeight > 0;
            rGraphExportSizeCustom.IsEnabled = settings.GraphExportWidth > 0 && settings.GraphExportHeight > 0;
            int graphWidht = settings.GraphExportWidth;
            int graphHeight = settings.GraphExportHeight;
            bGraphExportSizeWidth.Text = graphWidht < 0 ? "" : graphWidht.ToString();
            bGraphExportSizeHeight.Text = graphHeight < 0 ? "" : graphHeight.ToString();

            cRemoveEmptyRows.IsChecked = settings.HideEmptyRows;
            cRemoveNonExistentCountries.IsChecked = settings.HideNonExistentCountries;
            cRemoveWaterProvinces.IsChecked = settings.HideWaterProvinces;
            sDiplayTablePrecision.Value = settings.TableDisplayPrecision;

            // Graph lines colors
            if (boxGraphLines.SelectedIndex < 0)
            {
                GraphLinesColorPicker.IsEnabled = false;
            }
            else
            {
                GraphLinesColorPicker.IsEnabled = true;
                GraphLinesColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.GraphSeriesColors[boxGraphLines.SelectedIndex]);
            }

            ProvinceLandBordersColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.LandBorderColor);
            ProvinceSeaBordersColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.SeaBorderColor);
            ProvinceShoreBordersColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.ShoreBorderColor);
            CountryBordersColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.CountryBorderColor);

            kProvinceLandBordersVisible.IsChecked = settings.DisplayLandBorders;
            kProvinceSeaBordersVisible.IsChecked = settings.DisplaySeaBorders;
            kProvinceShoreBordersVisible.IsChecked = settings.DisplayShoreBorders;
            kCountryBordersVisible.IsChecked = settings.DisplayCountryBorders;
            kShadingVisible.IsChecked = settings.DisplayShading;

            sGIFZoom.Value = settings.GIFZoom;
            sGIFZoom_ValueChanged(this, null);

            switch (settings.SavegameCompressionLevel)
            {
                case 0: boxAARCompressionLevel.SelectedIndex = 1; break;
                case 1: boxAARCompressionLevel.SelectedIndex = 2; break;
                case 3: boxAARCompressionLevel.SelectedIndex = 3; break;
                case 5: boxAARCompressionLevel.SelectedIndex = 4; break;
                default: boxAARCompressionLevel.SelectedIndex = 0; break;
            }

            cLeftPaneVisibility.IsChecked = settings.LeftPaneVisible;
            cRightPaneVisibility.IsChecked = settings.RightPaneVisible;
        }

        private void rGraphExportSizeWindow_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            rGraphExportSizeCustom.IsEnabled = false;
        }

        private void rGraphExportSizeCustom_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            rGraphExportSizeCustom.IsEnabled = true;
            int graphWidht = settings.GraphExportWidth;
            int graphHeight = settings.GraphExportHeight;
            bGraphExportSizeWidth.Text = graphWidht < 0 ? "" : graphWidht.ToString();
            bGraphExportSizeHeight.Text = graphHeight < 0 ? "" : graphHeight.ToString();
        }

        public void SaveOptions()
        {
            if (rProvinceLabelsNone.IsChecked == true) settings.LabelsDisplayMode = 0;
            if (rProvinceLabelsIDs.IsChecked == true) settings.LabelsDisplayMode = 1;
            if (rProvinceLabelsNames.IsChecked == true) settings.LabelsDisplayMode = 2;
            if (rProvinceLabelsValues.IsChecked == true) settings.LabelsDisplayMode = 3;
            settings.LabelsShadows = cLabelShadows.IsChecked == true;
            settings.DisplayShading = kShadingVisible.IsChecked == true;

            settings.GIFDisplayCaptionBar = cDisplayCaptionBar.IsChecked == true;
            settings.GIFDisplayDate = cDisplayDate.IsChecked == true;
            settings.GIFDisplayMapmode = cDisplayMapmode.IsChecked == true;
            settings.GIFDisplayLogo = cDisplayLogo.IsChecked == true;

            settings.GraphSeriesCount = (int)BestEntitiesCountSlider.Value;
            settings.GraphSeriesSelectionMethod = BestEntitiesSetup.SelectedIndex;

            if (rGraphExportSizeWindow.IsChecked == true)
            {
                settings.GraphExportWidth = -1;
                settings.GraphExportHeight = -1;
            }
            else
            {
                int width;
                int height;
                if (Int32.TryParse(bGraphExportSizeWidth.Text, out width))
                    settings.GraphExportWidth = Math.Max(width, 10000);
                else
                    settings.GraphExportWidth = -1;
                if (Int32.TryParse(bGraphExportSizeHeight.Text, out height))
                    settings.GraphExportHeight = Math.Max(height, 30000);
                else
                    settings.GraphExportHeight = -1;
            }

            settings.HideEmptyRows = cRemoveEmptyRows.IsChecked == true;
            settings.HideNonExistentCountries = cRemoveNonExistentCountries.IsChecked == true;
            settings.HideWaterProvinces = cRemoveWaterProvinces.IsChecked == true;
            settings.TableDisplayPrecision = (int)sDiplayTablePrecision.Value;

            cRemoveEmptyRows.IsChecked = settings.HideEmptyRows;
            cRemoveNonExistentCountries.IsChecked = settings.HideNonExistentCountries;
            cRemoveWaterProvinces.IsChecked = settings.HideWaterProvinces;
            
            // Graph lines colors
            settings.LandBorderColor = CEBitmap.Bitmap.ColorToInt32(ProvinceLandBordersColorPicker.SelectedColor.Value);
            settings.SeaBorderColor = CEBitmap.Bitmap.ColorToInt32(ProvinceSeaBordersColorPicker.SelectedColor.Value);
            settings.ShoreBorderColor = CEBitmap.Bitmap.ColorToInt32(ProvinceShoreBordersColorPicker.SelectedColor.Value);
            settings.CountryBorderColor = CEBitmap.Bitmap.ColorToInt32(CountryBordersColorPicker.SelectedColor.Value);

            settings.DisplayLandBorders = kProvinceLandBordersVisible.IsChecked == true;
            settings.DisplaySeaBorders = kProvinceSeaBordersVisible.IsChecked == true;
            settings.DisplayShoreBorders = kProvinceShoreBordersVisible.IsChecked == true;
            settings.DisplayCountryBorders = kCountryBordersVisible.IsChecked == true;

            settings.GIFZoom = (int)sGIFZoom.Value;

            switch (boxAARCompressionLevel.SelectedIndex)
            {
                case 0: settings.SavegameCompressionLevel = -1; break;
                case 1: settings.SavegameCompressionLevel = 0; break;
                case 2: settings.SavegameCompressionLevel = 1; break;
                case 3: settings.SavegameCompressionLevel = 3; break;
                case 4: settings.SavegameCompressionLevel = 5; break;
                default: settings.SavegameCompressionLevel = -1; break;
            }

            settings.LeftPaneVisible = cLeftPaneVisibility.IsChecked == true;
            settings.RightPaneVisible = cRightPaneVisibility.IsChecked == true;
        }

        public void CommitSettings()
        {
            Core.Settings = settings;
            Core.MainWindow.SetPaneVisibility();
            Core.UI_Mapview.Redraw();
        }

        private void bRevert_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetSettings(Core.Settings);
            RefreshOptions();
        }

        private void bDefault_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settings = new Settings();
            RefreshOptions();
        }

        private void boxGraphLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Core.Settings == null) return;

            if (boxGraphLines.SelectedIndex < 0)
            {
                GraphLinesColorPicker.IsEnabled = false;
            }
            else
            {
                GraphLinesColorPicker.IsEnabled = true;
                GraphLinesColorPicker.SelectedColor = CEBitmap.Bitmap.Int32ToColor(settings.GraphSeriesColors[boxGraphLines.SelectedIndex]);
            }
        }

        private void GraphLinesColorPicker_SelectedColorChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (Core.Settings == null) return;

            settings.GraphSeriesColors[boxGraphLines.SelectedIndex] = CEBitmap.Bitmap.ColorToInt32(GraphLinesColorPicker.SelectedColor.Value);
        }

        private void sGIFZoom_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (lGIFZoom == null) return;
            double factor = Math.Pow(2, sGIFZoom.Value);
            if (Core.Data.Defs != null)
            {
                lGIFZoom.Content = factor + "x (" + Core.Data.Defs.Map.Width / factor + "x" + Core.Data.Defs.Map.Height / factor + ")";
            }
            else
            {
                lGIFZoom.Content = factor + "x";
            }
        }
    }
}
