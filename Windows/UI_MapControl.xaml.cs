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
using Xceed.Wpf.Toolkit.Zoombox;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_MapControl.xaml
    /// </summary>
    [Serializable]
    public partial class UI_MapControl : UserControl
    {
        bool manualZoomByUser = false;
        Zoombox viewport;
        UI_Mapview mapview;

        Point minimapFactor
        {
            get
            {
                return new Point(iMinimap.Width / Core.Data.Defs.Map.Width, iMinimap.Height / Core.Data.Defs.Map.Height);
            }
        }

        public UI_MapControl()
        {
            InitializeComponent();
        }

        public void InitializeMap(UI_Mapview mapview, Zoombox viewport, Image canvas)
        {
            this.viewport = viewport;
            this.mapview = mapview;

            RenderOptions.SetBitmapScalingMode(iMinimap, BitmapScalingMode.HighQuality);

            if (!Core.IsGameLoaded() || Core.Data.Defs.Map.Width <= 0 || Core.Data.Defs.Map.Height <= 0)
            {
                pParent.IsEnabled = false;
                rMinimap.Visibility = Visibility.Hidden;
            }
            else
            {
                pParent.IsEnabled = true;
                rMinimap.Visibility = Visibility.Visible;

                //Binding myBinding = new Binding();
                //myBinding.Source = canvas;
                //myBinding.Path = new PropertyPath("Source");
                //myBinding.Mode = BindingMode.TwoWay;
                //myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                //BindingOperations.SetBinding(iMinimap, Image.SourceProperty, myBinding);

                iMinimap.Source = new WriteableBitmap(Core.Data.Defs.Map.Width / 8, Core.Data.Defs.Map.Height / 8, 96, 96, PixelFormats.Bgra32, null);

                RefreshMinimap();
            }
        }

        public void RefreshMinimap()
        {
            if (!Core.IsGameLoaded() || viewport?.CurrentView.ViewKind != ZoomboxViewKind.Absolute || Core.Data.Defs.Map.Width <= 0 || Core.Data.Defs.Map.Height <= 0) return;

            UpdateMinimapRect();
        }

        public void RefreshMinimapAndZoom(Rect rect, double scale, bool round)
        {
            // Check if initialized

            if (!Core.IsGameLoaded() || double.IsNaN(iMinimap.Width) || Core.Data.Defs.Map.Width <= 0 || Core.Data.Defs.Map.Height <= 0) return;

            // Update zoom

            if (!manualZoomByUser)
            {
                double value = Math.Log(scale, 1.2);
                int pos = (int)Math.Round(value);
                if (pos < sZoom.Minimum) pos = (int)sZoom.Minimum;
                if (pos > sZoom.Maximum) pos = (int)sZoom.Maximum;

                sZoom.Value = round ? pos : value;
                lZoom.Content = (int)(scale * 100) + "%";
            }

            UpdateMinimapRect();
        }

        void UpdateMinimapRect()
        {
            // Update minimap

            iMinimap.Width = Math.Min(ActualWidth, 300);
            iMinimap.Height = Math.Min(ActualWidth, 300) * Core.Data.Defs.Map.Height / Core.Data.Defs.Map.Width;

            Rect m = new Rect(-viewport.CurrentView.Position.X / viewport.Scale * minimapFactor.X, -viewport.CurrentView.Position.Y / viewport.Scale * minimapFactor.Y,
            viewport.RenderSize.Width / viewport.Scale * minimapFactor.X, viewport.RenderSize.Height / viewport.Scale * minimapFactor.Y);

            double eLeft = -Math.Min(m.X, 0); //- Math.Min(m.X + m.Width - iMinimap.Width, 0);
            double eTop = -Math.Min(m.Y, 0); // - Math.Min(m.Y + m.Height - iMinimap.Height, 0);
            double eRight = Math.Max(m.X + m.Width - iMinimap.Width, 0);
            double eBottom = Math.Max(m.Y + m.Height - iMinimap.Height, 0);

            rMinimap.Margin = new Thickness(
                m.X + eLeft - eRight,
                m.Y + eTop - eBottom,
                Math.Max(0, iMinimap.Width - m.Right - eLeft + eRight),
                Math.Max(0, iMinimap.Height - m.Bottom - eTop + eBottom));

            Core.Debug("UI_MapControl.UpdateMinimapRect().eLeft", eLeft);
            Core.Debug("UI_MapControl.UpdateMinimapRect().eTop", eTop);
            Core.Debug("UI_MapControl.UpdateMinimapRect().eRight", eRight);
            Core.Debug("UI_MapControl.UpdateMinimapRect().eBottom", eBottom);

            Core.Debug("UI_MapControl.UpdateMinimapRect().rMinimap.Margin.Left", rMinimap.Margin.Left);
            Core.Debug("UI_MapControl.UpdateMinimapRect().rMinimap.Margin.Right", rMinimap.Margin.Right);
            Core.Debug("UI_MapControl.UpdateMinimapRect().rMinimap.Margin.Top", rMinimap.Margin.Top);
            Core.Debug("UI_MapControl.UpdateMinimapRect().rMinimap.Margin.Bottom", rMinimap.Margin.Bottom);
        }

        public void UpdateMouseoverProvince(Point coords)
        {
            if (!Core.IsGameLoaded() || Core.UI_Mapview == null || coords.X < 0 || coords.Y < 0 || Core.Data.Tables == null || 
                (!Core.Data.Tables.HasTimepoint(Core.UI_Mapview.CurrentTimepoint) && !Core.UI_Mapview.CurrentTimepoint.IsEmpty))
            {
                // Clear
                gMouseover.IsEnabled = false;
                tMouseoverProvinceName.Text = "";
                tMouseoverCountryName.Text = "";
                tMouseoverProvinceID.Text = "";
                tMouseoverValue.Visibility = Visibility.Visible;
                tMouseoverValue.Text = "";
                tMouseoverValueBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ushort id = Core.Data.Defs.GetProvinceByCoords((int)coords.X, (int)coords.Y);
                ushort country = Core.Data.GetProvinceMaster((Core.UI_Mapview?.CurrentTimepoint) ?? GameDate.Empty, id);

                gMouseover.IsEnabled = true;
                tMouseoverProvinceName.Text = Core.Data.Defs.Provinces.GetName(id, "{1}");
                tMouseoverCountryName.Text = Core.Data.Defs.Countries.GetName(Core.Data.GetProvinceMaster(Core.UI_Mapview.CurrentTimepoint, id), "{1}");
                tMouseoverProvinceID.Text = id.ToString();

                var table = Core.Data.Tables?.Select((Core.UI_Mapview?.CurrentTimepoint) ?? GameDate.Empty, Core.UI_Mapview?.CurrentTable?.Name);
                if (table is ISingleTable)
                {
                    tMouseoverValue.Visibility = Visibility.Visible;
                    tMouseoverValueBox.Visibility = Visibility.Collapsed;
                    tMouseoverValue.Text = (table as ISingleTable).GetValue(id, country).ToString();
                }
                else if (table is IMultiTable)
                {
                    tMouseoverValue.Visibility = Visibility.Collapsed;
                    tMouseoverValueBox.Visibility = Visibility.Visible;
                    tMouseoverValueBox.ItemsSource = (table as IMultiTable).GetValue(id, country);
                }
                else
                {
                    tMouseoverValue.Visibility = Visibility.Visible;
                    tMouseoverValueBox.Visibility = Visibility.Collapsed;
                    tMouseoverValue.Text = "";
                }
            }
        }

        public void UpdateSelectedProvince(Point coords)
        {
            if (!Core.IsGameLoaded() || coords.X < 0 || coords.Y < 0)
            {
                // Clear
                gSelected.IsEnabled = false;
                tSelectedProvinceName.Text = "";
                tSelectedCountryName.Text = "";
                tSelectedProvinceID.Text = "";
                tSelectedValue.Visibility = Visibility.Visible;
                tSelectedValue.Text = "";
                tSelectedValueBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ushort id = Core.Data.Defs.GetProvinceByCoords((int)coords.X, (int)coords.Y);
                ushort country = Core.Data.GetProvinceMaster((Core.UI_Mapview?.CurrentTimepoint) ?? GameDate.Empty, id);

                gSelected.IsEnabled = true;
                tSelectedProvinceName.Text = Core.Data.Defs.Provinces.GetName(id, "{1}");
                tSelectedCountryName.Text = Core.Data.Defs.Countries.GetName(Core.Data.GetProvinceMaster(Core.UI_Mapview.CurrentTimepoint, id), "{1}");
                tSelectedProvinceID.Text = id.ToString();

                var table = Core.Data.Tables?.Select((Core.UI_Mapview?.CurrentTimepoint) ?? GameDate.Empty, Core.UI_Mapview?.CurrentTable?.Name);
                if (table is ISingleTable)
                {
                    tSelectedValue.Visibility = Visibility.Visible;
                    tSelectedValueBox.Visibility = Visibility.Collapsed;
                    tSelectedValue.Text = (table as ISingleTable).GetValue(id, country).ToString();
                }
                else if (table is IMultiTable)
                {
                    tSelectedValue.Visibility = Visibility.Collapsed;
                    tSelectedValueBox.Visibility = Visibility.Visible;
                    tSelectedValueBox.ItemsSource = (table as IMultiTable).GetValue(id, country);
                }
                else
                {
                    tSelectedValue.Visibility = Visibility.Visible;
                    tSelectedValueBox.Visibility = Visibility.Collapsed;
                    tSelectedValue.Text = "";
                }

                Core.UI_Mapview.SetCurrentProvince(id);

            }
        }

        private void iMinimap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Core.IsGameLoaded()) return;

            Point c = e.GetPosition(sender as IInputElement);
            double width = viewport.Viewport.Width * minimapFactor.Y;
            double height = viewport.Viewport.Height * minimapFactor.Y;

            double eLeft = -Math.Min(c.X, 0);
            double eTop = -Math.Min(c.Y, 0);
            double eRight = Math.Max(c.X + width - iMinimap.Width, 0);
            double eBottom = Math.Max(c.Y + height - iMinimap.Height, 0);

            viewport.ZoomTo(new Point((c.X - width / 2) / minimapFactor.X * viewport.Scale, (c.Y - height / 2) / minimapFactor.Y * viewport.Scale));

            RefreshMinimapAndZoom(new Rect((c.X - width / 2 - eRight / 2) / minimapFactor.X, (c.Y - height / 2 - eBottom / 2) / minimapFactor.Y,
                (width) / minimapFactor.X, (height) / minimapFactor.Y), viewport.Scale, true);

            Core.UI_Mapview.SetMapQuality();
        }

        private void sZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Core.IsGameLoaded()) return;

            double newZoom = Math.Pow(1.2, e.NewValue);
            double zoomPercentage = (Math.Pow(1.2, e.NewValue - e.OldValue) - 1) * 100;

            viewport.ZoomOrigin = mapview.RelativeCenter;
            manualZoomByUser = true;
            viewport.ZoomTo(newZoom);
            manualZoomByUser = false;

            lZoom.Content = (int)(newZoom * 100) + "%";
        }

        private void bZoomOut_Click(object sender, RoutedEventArgs e)
        {
            sZoom.Value = Math.Max(sZoom.Value - 1, sZoom.Minimum);
        }

        private void bZoomIn_Click(object sender, RoutedEventArgs e)
        {
            sZoom.Value = Math.Min(sZoom.Value + 1, sZoom.Maximum);
        }

        private void bZoom100_Click(object sender, RoutedEventArgs e)
        {
            sZoom.Value = 0;
            sZoom_ValueChanged(null, new RoutedPropertyChangedEventArgs<double>(0, 0));
        }

        private void bZoomFit_Click(object sender, RoutedEventArgs e)
        {
            //UpdateMinimapAndZoom(new Rect(0, 0, Data.MapWidth, Data.MapHeight), Math.Min(Data.MapWidth / mapview.RenderSize.Width, Data.MapHeight / mapview.RenderSize.Height));
            double newZoom = Math.Min(mapview.RenderSize.Width / Core.Data.Defs.Map.Width, mapview.RenderSize.Height / Core.Data.Defs.Map.Height);
            viewport.ZoomTo(newZoom);
            viewport.ZoomTo(new Point(0, 0));
            RefreshMinimapAndZoom(new Rect(0, 0, Core.Data.Defs.Map.Width, Core.Data.Defs.Map.Height), newZoom, false);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshMinimap();
        }
    }
}
