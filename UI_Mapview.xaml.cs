using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Zoombox;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Mapview.xaml
    /// </summary>
    public partial class UI_Mapview : UserControl
    {
        bool manualTimepointSlideByUser = true;
        TextBlock[] labels;
        RotateTransform[] rt;
        Point[] lpos;

        DispatcherTimer PlaybackTimer = new DispatcherTimer();

        public ModeInterfaceElement DefaultMapmode;

        public ModeInterfaceElement CurrentTable { get; private set; } = null;
        public GameDate CurrentTimepoint { get; private set; } = GameDate.Empty;

        public ushort CurrentProvince { get; private set; } = 0;
        public ushort CurrentCountry { get { return Core.Data.GetProvinceMaster(CurrentTimepoint, CurrentProvince); } }

        public Point RelativeCenter
        {
            get
            {
                return new Point((viewport.Viewport.X + viewport.Viewport.Width / 2) / Core.Data.Defs.Map.Width, (viewport.Viewport.Y + viewport.Viewport.Height / 2) / Core.Data.Defs.Map.Height);
            }
        }

        public Point AbsolutePos
        {
            get
            {
                return new Point((viewport.Viewport.X + currentPos.X / viewport.Scale), (viewport.Viewport.Y + currentPos.Y / viewport.Scale));
            }
        }

        Point currentPos;
        bool mouseScrolling = false;

        public UI_Mapview()
        {
            InitializeComponent();

            Zoombox.SetViewFinderVisibility(viewport, Visibility.Hidden);
            viewport.DragModifiers.Clear();
            viewport.RelativeZoomModifiers.Clear();
            viewport.DragModifiers.Add(Xceed.Wpf.Toolkit.Core.Input.KeyModifier.None);
            viewport.RelativeZoomModifiers.Add(Xceed.Wpf.Toolkit.Core.Input.KeyModifier.None);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PlaybackTimer.Tick += PlaybackTimer_Tick;

            iPlayPause.Source = Ext.LoadBitmap("pack://application:,,,/Chronicle;component/Icons/play-16.png");
        }

        public void SetCurrentMode(ModeInterfaceElement mode)
        {
            foreach (var category in Core.MainWindow.Ribbon.MapRibbonTab.Groups[0].Items)
            {
                foreach (var item in (category as Fluent.DropDownButton).Items)
                {
                    if (item is Fluent.GroupSeparatorMenuItem) continue;
                    (item as Fluent.MenuItem).IsChecked = ((item as Fluent.MenuItem).Tag as ModeInterfaceElement).Name == mode.Name;
                }
            }

            if (mode != null)
            {
                lSelectedTable.Content = mode.Category + ": " + mode.Caption;
                CurrentTable = mode;
                Refresh();
            }
        }

        public void SetCurrentMode()
        {
            SetCurrentMode(DefaultMapmode);
        }

        public void Refresh()
        {
            SetInterfaceAvailability();

            Redraw();
        }

        public void SetInterfaceAvailability()
        {
            if (CurrentTable == null)
            {
                pMultiValueSettings.Visibility = Visibility.Collapsed;
                return;
            }

            pMultiValueSettings.Visibility = CurrentTable.Multivalue ? Visibility.Visible : Visibility.Collapsed;

            if (CurrentTable.Multivalue)
            {
                MultiValueTable t = Core.Data.Tables.Select(CurrentTimepoint, CurrentTable.Name) as MultiValueTable;
                if (t == null) return;

                cMultiValueKeys.ItemsSource = t.GetAllDatakeys(false);
            }

            Core.MainWindow.Ribbon.bExportMap.IsEnabled = CurrentTable != null;
            Core.MainWindow.Ribbon.bExportAnimated.IsEnabled = CurrentTable != null;
        }

        public void InitializeMap()
        {
            Core.UI_MapControl.InitializeMap(this, viewport, iMap);

            if (!Core.IsGameLoaded() || Core.Data.Defs.Map.Width <= 0 || Core.Data.Defs.Map.Height <= 0) return;

            if (iMap.Source == null || iMap.Source.Width != Core.Data.Defs.Map.Width || iMap.Source.Height != Core.Data.Defs.Map.Height)
            {
                iMap.Source = new WriteableBitmap(Core.Data.Defs.Map.Width, Core.Data.Defs.Map.Height, 96, 96, PixelFormats.Bgra32, null);
            }

            labels = Core.Data.Defs.Provinces.InitializeLabels(out rt, out lpos);
            cLabels.Children.Clear();
            Core.Dispatch.Run(() => SetLabelsVisibility());

            Core.UI_MapControl.UpdateMouseoverProvince(new Point(-1, -1));
            Core.UI_MapControl.UpdateSelectedProvince(new Point(-1, -1));
        }

        public void Redraw()
        {
            if (!Core.IsGameLoaded()) return;
            if (CurrentTable == null) return;

            Table t = Core.Data.Tables.Select(CurrentTimepoint, CurrentTable.Name);

            int[][] colors; double min; double max; double[] values;
            if (t != null)
            {
                colors = t.GetColors(
                           (MultivalueSetting)(Core.UI_Mapview.cMultiValueMode.SelectedIndex + 1),
                           ((Core.UI_Mapview.cMultiValueKeys.SelectedItem) ?? "").ToString(),
                           Core.Data.IndexToTag(Core.UI_Mapview.CurrentCountry),
                           (MultivalueColorSetting)(Core.UI_Mapview.cMultiValueColor.SelectedIndex + 1),
                           new int[] { Core.Settings.EmptyColor, Core.Settings.LandColor, Core.Settings.WaterColor, Core.Settings.SelectedCountryColor,
                           cAbsoluteColor.IsChecked==false?Core.Settings.DefaultColor:0},
                           out min, out max, out values);
            }
            else
            {
                colors = GenerateEmptyMapColors(Core.Data.Defs.Provinces.WaterProvinces);
                min = 0; max = 0; values = new double[t.Type == TableType.Country ? Core.Data.Defs.Countries.Count : Core.Data.Defs.Provinces.Count];
            }

            using (var bc = (iMap.Source as WriteableBitmap).GetBitmapContext())
            {
                Core.Data.Defs.EditBitmap(bc, colors);

                Core.Dispatch.Run(() =>
                (Core.UI_MapControl.iMinimap.Source as WriteableBitmap).Blit(
                    new Rect(0, 0, (Core.UI_MapControl.iMinimap.Source as WriteableBitmap).Width, (Core.UI_MapControl.iMinimap.Source as WriteableBitmap).Height), (iMap.Source as WriteableBitmap),
                    new Rect(0, 0, (iMap.Source as WriteableBitmap).Width, (iMap.Source as WriteableBitmap).Height)));

                if (Core.Settings.DisplayRivers) Core.Data.Defs.DrawRivers(bc);
                if (Core.Settings.DisplayShading) Core.Data.Defs.DrawAreaShading(bc);
                Core.Data.Defs.DrawBorders(bc, new bool[] { Core.Settings.DisplayCountryBorders, Core.Settings.DisplayLandBorders, Core.Settings.DisplaySeaBorders, Core.Settings.DisplayShoreBorders });
            }

            Core.UI_MapColors.DisplayColorscale(t.GetColorscale(null), min, max);
            Core.UI_MapStats.DisplayStats(t.Type == TableType.Country ? Core.Data.Defs.Countries.List("{1} ({0})") : Core.Data.Defs.Provinces.List("({0}) {1}"), values);

            Core.Dispatch.Run(() =>
            {
                RecreateLabels();
                UpdateLabelsOpacity();
            });
        }

        int[][] GenerateEmptyMapColors(bool[] waterProvinces)
        {
            int[][] output = new int[Core.Data.Defs.Provinces.Count][];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = new int[] { waterProvinces[i] ? Core.Settings.WaterColor : Core.Settings.EmptyColor };
            }
            return output;
        }

        public void RefreshViewpoint()
        {
            viewport_CurrentViewChanged(this, new Xceed.Wpf.Toolkit.Zoombox.ZoomboxViewChangedEventArgs(
                viewport.CurrentView, viewport.CurrentView, 0, 0));
        }

        private void viewport_CurrentViewChanged(object sender, Xceed.Wpf.Toolkit.Zoombox.ZoomboxViewChangedEventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            if (e.NewValue.ViewKind != ZoomboxViewKind.Absolute) return; // happens during quick movements

            double scale = double.IsNaN(e.NewValue.Scale) ? viewport.Scale : e.NewValue.Scale;

            Core.UI_MapControl.RefreshMinimapAndZoom(new Rect(-e.NewValue.Position.X / scale, -e.NewValue.Position.Y / scale,
                viewport.RenderSize.Width / scale, viewport.RenderSize.Height / scale), scale, true);
            UpdateLabelsOpacity();

            SetMapQuality();
        }

        private void viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseScrolling = false;
            currentPos = e.GetPosition(viewport);
        }

        private void viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if ((currentPos.X != e.GetPosition(viewport).X || currentPos.Y != e.GetPosition(viewport).Y) && e.LeftButton == MouseButtonState.Pressed)
                mouseScrolling = true;
            currentPos = e.GetPosition(viewport);

            Core.UI_MapControl.UpdateMouseoverProvince(AbsolutePos);
        }

        public void SetMapQuality()
        {
            if (iMap == null | viewport == null) return;

            if (viewport.Scale < Core.Settings.HighQualityAntialiasThreshold) // 1
            {
                RenderOptions.SetBitmapScalingMode(iMap, BitmapScalingMode.HighQuality);
            }
            else if (viewport.Scale < Core.Settings.LowQualityAntialiasThreshold) // 2
            {
                RenderOptions.SetBitmapScalingMode(iMap, BitmapScalingMode.LowQuality);
            }
            else
            {
                RenderOptions.SetBitmapScalingMode(iMap, BitmapScalingMode.NearestNeighbor);
            }
        }

        private void viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentPos = e.GetPosition(viewport);
            if (!mouseScrolling)
                Core.UI_MapControl.UpdateSelectedProvince(AbsolutePos);
            mouseScrolling = false;
        }

        public void ResetTimeline()
        {
            manualTimepointSlideByUser = false;

            int count = Core.Data.Tables == null ? 0 : Core.Data.Tables.GetTimepointsCount();
            if (count > 1)
            {
                CurrentTimepoint = Core.Data.Tables.GetTimepoint(0);
                sTimepoint.Minimum = 0;
                sTimepoint.Maximum = count - 1;
                sTimepoint.Value = 0;
                lTimepoint.Content = CurrentTimepoint.GetString("d MMMM yyy");
                lTimepoint.Visibility = Visibility.Visible;
                sTimepoint.Visibility = Visibility.Visible;
                bPlayPause.Visibility = Visibility.Visible;
            }
            else if (count == 1)
            {
                CurrentTimepoint = Core.Data.Tables.GetTimepoint(0);
                sTimepoint.Minimum = 0;
                sTimepoint.Maximum = count - 1;
                sTimepoint.Value = 0;
                lTimepoint.Content = CurrentTimepoint.GetString("d MMMM yyy");
                lTimepoint.Visibility = Visibility.Visible;
                sTimepoint.Visibility = Visibility.Collapsed;
                bPlayPause.Visibility = Visibility.Collapsed;


                sTimepoint.Minimum = 0;
                sTimepoint.Maximum = 100;
            }
            else
            {
                CurrentTimepoint = GameDate.Empty;
                sTimepoint.Minimum = 0;
                sTimepoint.Maximum = 0;
                sTimepoint.Value = 0;
                lTimepoint.Visibility = Visibility.Collapsed;
                sTimepoint.Visibility = Visibility.Collapsed;
                bPlayPause.Visibility = Visibility.Collapsed;
            }

            manualTimepointSlideByUser = true;
        }

        public void SetCurrentProvince(ushort id)
        {
            if (id == CurrentProvince) return;

            CurrentProvince = id;

            if (CurrentTable.Multi) Redraw();
        }

        private void sTimepoint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!manualTimepointSlideByUser) return;
            CurrentTimepoint = Core.Data.Tables.GetTimepoint((int)sTimepoint.Value);
            lTimepoint.Content = CurrentTimepoint.GetString("d MMMM yyy");
            
            Task.Run(() => RefreshTask.Set());
        }

        private void cMultiValueMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
            if (pMultiValueKeys == null || pMultiValueColor == null || pAbsoluteColor == null) return;
            pMultiValueKeys.Visibility = cMultiValueMode.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
            pMultiValueColor.Visibility = cMultiValueMode.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
            pAbsoluteColor.Visibility = cMultiValueMode.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Labels

        public void RecreateLabels()
        {
            if (Core.Data.Defs == null) return;
            switch (Core.Settings.LabelsDisplayMode)
            {
                case 1:
                    for (ushort i = 1; i < Core.Data.Defs.Provinces.Count; i++)
                    {
                        labels[i].Text = i.ToString();
                    }
                    break;
                case 2:
                    var list = Core.Data.Defs.Provinces.List("({0}) {1}").ToArray();
                    for (ushort i = 0; i < Core.Data.Defs.Provinces.Count - 1; i++)
                    {
                        labels[i].Text = list[i];
                    }
                    break;
                case 3:
                    if (CurrentTable == null || CurrentTimepoint.IsEmpty)
                    {
                        for (ushort i = 1; i < Core.Data.Defs.Provinces.Count; i++)
                        {
                            labels[i].Text = "";
                        }
                    }
                    else
                    {
                        var table = Core.Data.Tables?.Select(CurrentTimepoint, CurrentTable?.Name);
                        var masters = Core.Data.GetProvinceBasekeys(CurrentTimepoint, true);
                        for (ushort i = 1; i < Core.Data.Defs.Provinces.Count; i++)
                        {
                            if (table is ISingleTable) labels[i].Text = (table as ISingleTable).GetValue(i, masters[i]).ToString();
                            else if (table is IMultiTable) labels[i].Text = (table as IMultiTable).GetValue(i, masters[i]).ToString();
                            else labels[i].Text = "";
                        }
                    }
                    break;
                default:
                    for (ushort i = 1; i < Core.Data.Defs.Provinces.Count; i++)
                    {
                        labels[i].Text = "";
                    }
                    break;
            }
        }

        public void UpdateLabelsOpacity()
        {
            if (Core.Settings.LabelsOpacity < 0)
            {
                if (viewport.Scale <= 1)
                    cLabels.Opacity = 0;
                else
                    cLabels.Opacity = (viewport.Scale - 1) / 4;
            }
            else
            {
                cLabels.Opacity = Core.Settings.LabelsOpacity;
            }

            if (Core.Settings.LabelsShadows)
                cLabels.Effect = Core.LabelsShadow;
            else
                cLabels.Effect = null;
        }

        public void SetLabelsVisibility()
        {
            if (Core.Data.Defs == null) return;

            for (ushort i = 0; i < Core.Data.Defs.Provinces.Count; i++)
            {
                if (!cLabels.Children.Contains(labels[i]))
                {
                    cLabels.Children.Add(labels[i]);
                    Canvas.SetLeft(labels[i], lpos[i].X - labels[i].Text.Length / 3 * labels[i].FontSize);
                    Canvas.SetTop(labels[i], lpos[i].Y - 2 * labels[i].FontSize);
                }
            }
        }

        #endregion

        public void RefreshAvailableMapmodesUI()
        {
            Fluent.RibbonGroupBox gb = new Fluent.RibbonGroupBox();
            gb.Header = "Categories";

            List<ModeInterfaceElement> modes = Core.Data.GetTablesInterfaceElements(true);

            Fluent.DropDownButton category = null;
            int lastSection = -1;

            foreach (var mode in modes)
            {
                if (mode.Category != category?.Header?.ToString())
                {
                    category = new Fluent.DropDownButton();
                    category.Header = mode.Category;
                    category.MaxDropDownHeight = 600;
                    string token = Core.Data.Game.Token == "hoi3tfh" ? "hoi3" : Core.Data.Game.Token;
                    string iconPath = Path.Combine(Core.Paths.Application, "Script", "Chronicle.Default", token + "." + mode.Category + ".png");
                    if (File.Exists(iconPath))
                    {
                        BitmapImage icon = new BitmapImage();
                        icon.BeginInit();
                        icon.UriSource = new Uri(iconPath, UriKind.Absolute);
                        icon.CacheOption = BitmapCacheOption.OnLoad;
                        icon.EndInit();
                        category.LargeIcon = icon;
                    }
                    gb.Items.Add(category);
                    lastSection = -1;
                }
                if (lastSection > -1 && mode.Section != lastSection)
                {
                    var s = new Fluent.GroupSeparatorMenuItem();
                    s.Height = 2;
                    category.Items.Add(s);
                }
                var mi = new Fluent.MenuItem();
                mi.Header = mode.Caption;
                mi.Tag = mode;

                mi.IsChecked = mode.Name == Core.UI_Mapview.CurrentTable?.Name;
                switch (mode.Type)
                {
                    case TableType.Special: continue;
                    case TableType.Province: mi.Icon = Core.ProvinceModeIcon; break;
                    case TableType.Country: mi.Icon = Core.CountryModeIcon; break;
                }
                mi.Click += MapMenuItem_Click;
                lastSection = mode.Section;
                category.Items.Add(mi);
            }

            if (Core.MainWindow.Ribbon.MapRibbonTab.Groups.Count > 0 && Core.MainWindow.Ribbon.MapRibbonTab.Groups[0].Header.ToString() == "Categories")
                Core.MainWindow.Ribbon.MapRibbonTab.Groups.RemoveAt(0);
            Core.MainWindow.Ribbon.MapRibbonTab.Groups.Insert(0, gb);

            DefaultMapmode = modes.Find(x => x.Name == "Default");
        }

        private static void MapMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var category in Core.MainWindow.Ribbon.MapRibbonTab.Groups[0].Items)
            {
                foreach (var item in (category as Fluent.DropDownButton).Items)
                {
                    (item as Fluent.MenuItem).IsChecked = false;
                }
            }
            ((Fluent.MenuItem)(sender)).IsChecked = true;
            Core.UI_Mapview.SetCurrentMode((ModeInterfaceElement)((Fluent.MenuItem)(sender)).Tag);
        }

        private void cAbsoluteColor_Checked(object sender, RoutedEventArgs e)
        {
            cMultiValueMode_SelectionChanged(this, null);
        }

        private void bPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (PlaybackTimer.IsEnabled)
                StopPlayback();
            else
                StartPlayback();
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (sTimepoint.Value == sTimepoint.Maximum)
            {
                sTimepoint.Value = 0;
                StopPlayback();
            }
            else
            {
                sTimepoint.Value++;
                //CurrentTimepoint = Core.Data.Tables.GetTimepoint((int)sTimepoint.Value + 1);
                //lTimepoint.Content = CurrentTimepoint.GetString("d MMMM yyy");
                //Task.Run(() => RefreshTask.Set());
            }
        }

        public void StartPlayback()
        {
            // Start the playback
            iPlayPause.Source = Ext.LoadBitmap("pack://application:,,,/Chronicle;component/Icons/pause-16.png");
            tPlayPause.Text = "Pause";
            PlaybackTimer.Start();
        }

        public void StopPlayback()
        {
            // Stop the playback
            iPlayPause.Source = Ext.LoadBitmap("pack://application:,,,/Chronicle;component/Icons/play-16.png");
            tPlayPause.Text = "Play";
            PlaybackTimer.Stop();
        }
    }

    public static class RefreshTask
    {
        static bool finished = true;
        static bool set = false;
        static DispatcherTimer timer;

        public static void Initialize()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (set) Run();
        }

        public static void Set()
        {
            set = true;
            if (finished) Run();
        }

        static void Finished()
        {
            finished = true;
            if (set) Run();
        }

        static void Run()
        {
            set = false;
            Core.Dispatch.Run(() =>
            {
                Core.UI_Mapview.Refresh();
                Finished();
            });
        }
    }
}
