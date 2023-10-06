using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public partial class UI_Ribbon : UserControl
    {
        public UI_Ribbon()
        {
            InitializeComponent();
        }

        private void Backstage_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Backstage.IsOpen)
            {
                bNew.IsEnabled = Core.Data.Game != null;
                bOpen.IsEnabled = true;
                bSave.IsEnabled = Core.Data.Game != null && Core.Data.Tables.GetTimepointsCount() > 0;
                bSaveAs.IsEnabled = Core.Data.Game != null && Core.Data.Tables.GetTimepointsCount() > 0;
                UI_Import.SetInterfaceAvailability();
                UI_Options.SetSettings(Core.Settings);
            }
            else
            {
                UI_Options.SaveOptions();
                UI_Options.CommitSettings();
                Core.UI_Mapview.UpdateLabelsOpacity();
                UI_Import.Commit();
                CloseBackstage();
            }
        }

        public void CloseBackstage()
        {
            UI_Import.Commit();
        }

        private void Ribbon_SelectedTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Core.FullyLoaded) return;

            if (e.AddedItems.Contains(MapRibbonTab))
            {
                SelectTab(MapRibbonTab);
            }
            if (e.AddedItems.Contains(TablesRibbonTab))
            {
                SelectTab(TablesRibbonTab);
            }
            if (e.AddedItems.Contains(GraphsRibbonTab))
            {
                SelectTab(GraphsRibbonTab);
            }
        }

        public void SelectTab(Fluent.RibbonTabItem tab)
        {
            if (tab == MapRibbonTab)
            {
                Core.MainWindow.UI_Mapview.Visibility = Visibility.Visible;
                Core.MainWindow.UI_Tableview.Visibility = Visibility.Collapsed;
                Core.MainWindow.UI_Graphview.Visibility = Visibility.Collapsed;

                Core.MainWindow.tUI_MapControl.Visibility = Visibility.Visible;
                Core.MainWindow.tUI_MapStats.Visibility = Visibility.Visible;
                Core.MainWindow.tUI_MapColors.Visibility = Visibility.Visible;
                Core.MainWindow.tUI_TableSettings.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_GraphSettings.Visibility = Visibility.Collapsed;

                Core.MainWindow.tUI_MapControl.IsSelected = true;
            }
            if (tab == TablesRibbonTab)
            {
                Core.MainWindow.UI_Mapview.Visibility = Visibility.Collapsed;
                Core.MainWindow.UI_Tableview.Visibility = Visibility.Visible;
                Core.MainWindow.UI_Graphview.Visibility = Visibility.Collapsed;

                Core.MainWindow.tUI_MapControl.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_MapStats.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_MapColors.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_TableSettings.Visibility = Visibility.Visible;
                Core.MainWindow.tUI_GraphSettings.Visibility = Visibility.Collapsed;

                Core.MainWindow.tUI_TableSettings.IsSelected = true;
            }
            if (tab == GraphsRibbonTab)
            {
                Core.MainWindow.UI_Mapview.Visibility = Visibility.Collapsed;
                Core.MainWindow.UI_Tableview.Visibility = Visibility.Collapsed;
                Core.MainWindow.UI_Graphview.Visibility = Visibility.Visible;

                Core.MainWindow.tUI_MapControl.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_MapStats.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_MapColors.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_TableSettings.Visibility = Visibility.Collapsed;
                Core.MainWindow.tUI_GraphSettings.Visibility = Visibility.Visible;

                Core.MainWindow.tUI_GraphSettings.IsSelected = true;
            }

            Core.MainWindow.tUI_Debug.Visibility = Core.Settings.DebugMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private void mSimpleTable_Click(object sender, RoutedEventArgs e)
        {
            Core.UI_Tableview.Refresh();
        }

        private async void mExportWholeMap_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveGFXDialog.DefaultExt = ".png";
            Core.SaveGFXDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            if (Core.SaveGFXDialog.ShowDialog() == true)
            {
                Core.Log.Write("Map export to file: <" + Core.SaveGFXDialog.FileName + "> initiated.");
                Core.Dispatch.DisplayProgress("Rendering bitmap...");

                ImageSource src = Core.UI_Mapview.iMap.Source.Clone();
                src.Freeze();

                try
                {
                    await Task.Run(() => Core.SaveImage(src));

                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error when exporting whole map to external graphics file: <" + Core.SaveGFXDialog.FileName + ">.", ex);
                }
                finally
                {
                    Core.Dispatch.HideProgress();
                }
            }
        }

        private async void mExportVisiblePortion_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveGFXDialog.DefaultExt = ".png";
            Core.SaveGFXDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            if (Core.SaveGFXDialog.ShowDialog() == true)
            {
                Core.Log.Write("Map export to file: <" + Core.SaveGFXDialog.FileName + "> initiated.");
                Core.Dispatch.DisplayProgress("Rendering bitmap...");

                RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)Core.UI_Mapview.viewport.ActualWidth,
                    (int)Core.UI_Mapview.viewport.ActualHeight, 96, 96, PixelFormats.Default);
                targetBitmap.Render(Core.UI_Mapview.viewport);

                ImageSource src = targetBitmap.Clone();
                src.Freeze();

                try
                {
                    await Task.Run(() => Core.SaveImage(src));
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error when exporting visible part of the map to external graphics file: <" + Core.SaveGFXDialog.FileName + ">.", ex);
                }
                finally
                {
                    Core.Dispatch.HideProgress();
                }
            }
        }

        private async void bExportAnimated_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveGFXDialog.DefaultExt = ".gif";
            Core.SaveGFXDialog.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
            if (Core.SaveGFXDialog.ShowDialog() == true)
            {
                try
                {
                    await Task.Run(() => Core.Animated.SaveImage(Core.SaveGFXDialog.FileName, Core.UI_Mapview.CurrentTable, Core.UI_Mapview.CurrentTable.Name == "Political"));
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error when exporting map to external animated graphics file: <" + Core.SaveGFXDialog.FileName + ">.", ex);
                }
                finally
                {
                    Core.Dispatch.HideProgress();
                }
            }
        }

        private async void bNew_Click(object sender, RoutedEventArgs e)
        {
            Core.Data.CurrentFile = "";
            Core.Data.UnsavedChanges = false;
            await Task.Run(() => Core.Data.LoadGame(Core.Data.Game, Core.UI_Mapview));
        }

        private async void bOpen_Click(object sender, RoutedEventArgs e)
        {
            Core.OpenDialog.Filter = "AAR files (*.aar)|*.aar|All files (*.*)|*.*";

            if (Core.OpenDialog.ShowDialog() == true)
            {
                await Task.Run(() => Core.Data.Load(Core.OpenDialog.FileName));
                Core.Data.CurrentFile = Core.OpenDialog.FileName;
                Core.Data.UnsavedChanges = false;
            }
        }

        private async void bSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Core.IsSavegameLoaded()) return;

            if (Core.Data.CurrentFile == "")
            {
                bSaveAs_Click(this, new RoutedEventArgs());
                return;
            }

            await Task.Run(() =>
                        {
                            Core.Data.SaveAs(Core.Data.CurrentFile);
                            Core.Data.UnsavedChanges = false;
                        });
        }

        private async void bSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (!Core.IsSavegameLoaded()) return;
            Core.SaveDialog.Filter = "AAR files (*.aar)|*.aar|All files (*.*)|*.*";

            if (Core.SaveDialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    Core.Data.SaveAs(Core.SaveDialog.FileName);
                    Core.Data.CurrentFile = Core.SaveDialog.FileName;
                });
                Core.Data.UnsavedChanges = false;
            }
        }

        private void bExportTableClipboard_Click(object sender, RoutedEventArgs e)
        {
            Core.UI_Tableview.CopyTable();
        }

        private void bExportTableFile_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveCSVDialog.DefaultExt = ".csv";
            Core.SaveCSVDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (Core.SaveCSVDialog.ShowDialog() == true)
            {
                Core.UI_Tableview.ExportTable(Core.SaveCSVDialog.FileName);
            }
        }

        private void bExportGraphFile_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveGFXDialog.DefaultExt = ".png";
            Core.SaveGFXDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            if (Core.SaveGFXDialog.ShowDialog() == true)
            {
                Core.UI_Graphview.ExportGraph(Core.SaveCSVDialog.FileName);
            }
        }

        private void bCaptionsDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (mCaptionsNames.IsChecked) Core.Settings.LabelsDisplayMode = 2;
            if (mCaptionsIDs.IsChecked) Core.Settings.LabelsDisplayMode = 1;
            if (mCaptionsValues.IsChecked) Core.Settings.LabelsDisplayMode = 3;
            if (mCaptionsNone.IsChecked) Core.Settings.LabelsDisplayMode = 0;

            Core.UI_Mapview.RecreateLabels();
        }

        private void bBordersDisplay_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.DisplayLandBorders = mLandBorders.IsChecked;
            Core.Settings.DisplaySeaBorders = mWaterBorders.IsChecked;
            Core.Settings.DisplayShoreBorders = mShoreBorders.IsChecked;
            Core.Settings.DisplayCountryBorders = mCountryBorders.IsChecked;

            Core.UI_Mapview.Redraw();
        }

        private void bCaptions_DropDownOpened(object sender, EventArgs e)
        {
            mCaptionsNames.IsChecked = Core.Settings.LabelsDisplayMode == 2;
            mCaptionsIDs.IsChecked = Core.Settings.LabelsDisplayMode == 1;
            mCaptionsValues.IsChecked = Core.Settings.LabelsDisplayMode == 3;
            mCaptionsNone.IsChecked = Core.Settings.LabelsDisplayMode == 0;
        }

        private void bBorders_DropDownOpened(object sender, EventArgs e)
        {
            mLandBorders.IsChecked = Core.Settings.DisplayLandBorders;
            mWaterBorders.IsChecked = Core.Settings.DisplaySeaBorders;
            mShoreBorders.IsChecked = Core.Settings.DisplayShoreBorders;
            mCountryBorders.IsChecked = Core.Settings.DisplayCountryBorders;
        }
    }
}
