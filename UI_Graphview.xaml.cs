using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Graphview.xaml
    /// </summary>
    public partial class UI_Graphview : UserControl
    {
        PlotModel model;
        public ModeInterfaceElement CurrentTable { get; private set; } = null;

        StatsData CurrentGraphBody = null;

        public UI_Graphview()
        {
            InitializeComponent();
        }

        public void SetCurrentMode(ModeInterfaceElement mode)
        {
            tSelectedGraph.Text = mode.Category + ": " + mode.Caption;
            CurrentTable = mode;
            SetInterfaceAvailability();

            CurrentGraphBody = GenerateTable();
            DisplayGraph(CurrentGraphBody, mode.Caption, SelectEntities(CurrentGraphBody, new List<ushort>()));
        }

        /// <summary>
        /// Initializes graphs, setting legend and axes.
        /// </summary>
        public void Initialize()
        {
            model = new PlotModel();
            Plot.DataContext = model;

            model.LegendTitle = "Legend";
            model.LegendOrientation = LegendOrientation.Horizontal;
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.TopRight;
            model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            model.LegendBorder = OxyColors.Black;

            var dateAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Date",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 80
            };
            model.Axes.Add(dateAxis);
            var valueAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Value"
            };
            model.Axes.Add(valueAxis);
        }

        public StatsData GenerateTable()
        {
            if (CurrentTable == null) return null;

            if (CurrentTable.Multi)
            {
                System.Windows.Forms.MessageBox.Show("It is not possible to generate a graph for a multiple values table. You can export a table data to an external spreadsheet program for further analysis.",
                    "Graph generation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
            else
            {
                return Core.Data.Tables.GetStats(CurrentTable.Name, false, false, false);
            }
        }

        public void Reset()
        {
            CurrentGraphBody = null;
            Core.Dispatch.Run(() => ClearGraph());
        }

        public void Refresh()
        {
            SetInterfaceAvailability();

            CurrentGraphBody = GenerateTable();

            List<ushort> selected = new List<ushort>();
            if (CurrentTable != null)
            {
                selected = CurrentTable.Type == TableType.Country ? Core.UI_GraphSettings.GetEnforcedCountries().ToList() : Core.UI_GraphSettings.GetEnforcedProvinces().ToList();
            }
            DisplayGraph(CurrentGraphBody, (CurrentTable?.Caption) ?? "", SelectEntities(CurrentGraphBody, selected));
        }

        public void SetInterfaceAvailability()
        {
            Core.MainWindow.Ribbon.bExportGraphFile.IsEnabled = CurrentTable != null;
        }

        private List<ushort> SelectEntities(StatsData data, List<ushort> enforced)
        {
            // Prepare entity choice
            List<ushort> selected = new List<ushort>();
            if (data == null) return selected;

            selected.AddRange(enforced.Count > Core.Settings.GraphSeriesCount ? enforced.Take(Core.Settings.GraphSeriesCount) : enforced);

            if (selected.Count >= Core.Settings.GraphSeriesCount) return selected;

            int column;
            switch (Core.Settings.GraphSeriesSelectionMethod)
            {
                case 0: column = 0; break;
                case 1: column = -1; break;
                case 2: column = data.Columns.Length - 1 - 1; break; // one -1 for caption column
                default: column = -1; break;
            }

            selected.AddRange(data.GetBest(Core.Settings.GraphSeriesCount - selected.Count, column).Select(x => x.Key));

            return selected;
        }

        public void DisplayGraph(StatsData data, string title, List<ushort> selected)
        {
            if (data == null)
            {
                ClearGraph();
                return;
            }

            int timepoints = data.Columns.Length - 1;

            if (timepoints < 1)
            {
                ClearGraph();
                return;
            }

            // Prepare the graph

            OxyPlot.Series.LineSeries[] series = new OxyPlot.Series.LineSeries[selected.Count];
            for (int i = 0; i < selected.Count; i++)
            {
                series[i] = new OxyPlot.Series.LineSeries();
                series[i].StrokeThickness = 2;
                //System.Windows.Media.Color color = Core.Settings.GraphColors[i];
                //series[i].Color = OxyColor.FromRgb(color.R, color.G, color.B);
                series[i].Title = data.Rows[selected[i]].Header;
            }

            model.Series.Clear();
            List<string> axisSource = new List<string>(data.Columns);
            axisSource.RemoveAt(0);
            model.Axes[0].LabelFormatter = new Func<double, string>((i) =>
            {
                int index = (int)i;
                return (index < 0 || index >= axisSource.Count) ? "" : axisSource[index];
            });
            for (int i = 0; i < selected.Count; i++)
            {
                for (int j = 0; j < timepoints; j++)
                {
                    series[i].Points.Add(new DataPoint(j, (data.Rows[selected[i]] as StatsRowNumValue).Data[j]));
                }
                model.Series.Add(series[i]);
            }

            model.Title = title;

            model.InvalidatePlot(true);
        }

        public void ClearGraph()
        {
            if (model == null) return;
            model.Series.Clear();
        }

        public void ExportGraph(string path)
        {
            ExportGraph(CurrentGraphBody, path);
        }

        public void ExportGraph(IStatsSource data, string path)
        {
            if (data == null) return;
            try
            {
                int width = Core.Settings.GraphExportWidth < 0 ? (int)ActualWidth : Core.Settings.GraphExportWidth;
                int height = Core.Settings.GraphExportHeight < 0 ? (int)ActualHeight : Core.Settings.GraphExportHeight;
                if (width <= 0) width = 100;
                if (height <= 0) height = 100;
                BitmapSource bitmap = PngExporter.ExportToBitmap(model, width, height, OxyColor.FromArgb(0, 0, 0, 0), 96);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(new FileStream(path, FileMode.Create));
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error exporting graph data to PNG file: <" + path + ">.", ex);
            }
        }

        public void RefreshAvailableGraphsUI()
        {
            Fluent.RibbonGroupBox gb = new Fluent.RibbonGroupBox();
            gb.Header = "Categories";

            List<ModeInterfaceElement> modes = Core.Data.GetTablesInterfaceElements(false);

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

                mi.IsChecked = mode.Name == Core.UI_Graphview.CurrentTable?.Name;
                switch (mode.Type)
                {
                    case TableType.Special: continue;
                    case TableType.Province: mi.Icon = Core.ProvinceModeIcon; break;
                    case TableType.Country: mi.Icon = Core.CountryModeIcon; break;
                }
                mi.Click += GraphMenuItem_Click;
                lastSection = mode.Section;
                category.Items.Add(mi);
            }

            if (Core.MainWindow.Ribbon.GraphsRibbonTab.Groups.Count > 0 && Core.MainWindow.Ribbon.GraphsRibbonTab.Groups[0].Header.ToString() == "Categories")
                Core.MainWindow.Ribbon.GraphsRibbonTab.Groups.RemoveAt(0);
            Core.MainWindow.Ribbon.GraphsRibbonTab.Groups.Insert(0, gb);

        }

        private static void GraphMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var category in Core.MainWindow.Ribbon.GraphsRibbonTab.Groups[0].Items)
            {
                foreach (var item in (category as Fluent.DropDownButton).Items)
                {
                    (item as Fluent.MenuItem).IsChecked = false;
                }
            }
            ((Fluent.MenuItem)(sender)).IsChecked = true;
            Core.UI_Graphview.SetCurrentMode((ModeInterfaceElement)((Fluent.MenuItem)(sender)).Tag);
        }
    }
}
