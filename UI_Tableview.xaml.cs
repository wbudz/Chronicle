using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Tableview.xaml
    /// </summary>
    public partial class UI_Tableview : UserControl
    {
        public ModeInterfaceElement CurrentTable { get; private set; } = null;
        IStatsSource CurrentTableBody = null;

        public UI_Tableview()
        {
            InitializeComponent();
        }

        public void SetCurrentMode(ModeInterfaceElement mode)
        {
            tSelectedTable.Text = mode.Category + ": " + mode.Caption;
            CurrentTable = mode;
            Refresh();
        }

        public void Refresh()
        {
            SetInterfaceAvailability();

            CurrentTableBody = GenerateTable();
            DisplayTable(CurrentTableBody);
        }

        public void SetInterfaceAvailability()
        {
            Core.MainWindow.Ribbon.bExportTableClipboard.IsEnabled = CurrentTable != null;
            Core.MainWindow.Ribbon.bExportTableFile.IsEnabled = CurrentTable != null;
        }

        public IStatsSource GenerateTable()
        {
            if (CurrentTable == null) return null;

            // Set options
            bool removeEmptyRows = Core.UI_TableSettings.cRemoveEmptyRows.IsChecked == true;
            bool removeNonExistentCountries = Core.UI_TableSettings.cRemoveNonExistentCountries.IsChecked == true;
            bool removeWaterProvs = Core.UI_TableSettings.cRemoveWaterProvinces.IsChecked == true;

            if (Core.MainWindow.Ribbon.mSimpleTable.IsChecked == true)
            {
                if (CurrentTable.Multi)
                {
                    System.Windows.Forms.MessageBox.Show("It is not possible to generate a standard table for a multiple values table. Advanced type will be selected instead.",
                        "Table generation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    Core.MainWindow.Ribbon.mAdvancedTable.IsChecked = true;
                    return GenerateTable();
                }
                else
                {
                    return Core.Data.Tables.GetStats(CurrentTable.Name, removeEmptyRows, removeNonExistentCountries, removeWaterProvs);
                }
            }
            else if (Core.MainWindow.Ribbon.mAdvancedTable.IsChecked == true)
            {
                return Core.Data.Tables.GetPivot(CurrentTable.Name, removeEmptyRows);
            }
            return null;
        }

        public void Reset()
        {
            CurrentTableBody = null;
            Core.Dispatch.Run(() => ClearTable());
        }

        public bool DisplayTable(IStatsSource data)
        {
            Stats.Columns.Clear();

            if (data == null || data.Columns == null || data.Columns.Length < 1) return false;

            Xceed.Wpf.DataGrid.Column col;

            if (data is StatsData)
            {
                col = new Xceed.Wpf.DataGrid.Column();
                col.FieldName = "Header";
                col.Title = data.Columns[0];
                col.CellContentTemplate = EntityTemplate;
                Stats.Columns.Add(col);

                for (int i = 1; i < data.Columns.Length; i++)
                {
                    col = new Xceed.Wpf.DataGrid.Column();
                    col.FieldName = "Data[" + (i - 1) + "]";
                    col.Title = data.Columns[i];
                    col.CellContentTemplate = DataTemplate;
                    Stats.Columns.Add(col);
                }

                Stats.ItemsSource = ((StatsData)data).Rows;
            }
            if (data is StatsPivot)
            {
                string[] titles = data.Columns;

                col = new Xceed.Wpf.DataGrid.Column();
                col.FieldName = "Date";
                col.Title = titles[0];
                col.CellContentTemplate = EntityTemplate;
                Stats.Columns.Add(col);

                col = new Xceed.Wpf.DataGrid.Column();
                col.FieldName = "Basekey";
                col.Title = titles[1];
                col.CellContentTemplate = EntityTemplate;
                Stats.Columns.Add(col);

                col = new Xceed.Wpf.DataGrid.Column();
                col.FieldName = "Datakey";
                col.Title = titles[2];
                col.CellContentTemplate = EntityTemplate;
                Stats.Columns.Add(col);

                col = new Xceed.Wpf.DataGrid.Column();
                col.FieldName = "Value";
                col.Title = titles[3];
                col.CellContentTemplate = DataTemplate;
                Stats.Columns.Add(col);

                Stats.ItemsSource = ((StatsPivot)data).Rows;
            }

            return true;
        }

        public void ClearTable()
        {
            Stats.ItemsSource = null;
            Stats.Items.Clear();
            Stats.Columns.Clear();
        }

        public void CopyTable()
        {
            CopyTable(CurrentTableBody);
        }

        public void CopyTable(IStatsSource data)
        {
            if (data == null) return;
            Clipboard.SetText(data.GetText("\t"));
        }

        public void ExportTable(string path)
        {
            ExportTable(CurrentTableBody, path);
        }

        public void ExportTable(IStatsSource data, string path)
        {
            if (data == null) return;
            try
            {
                System.IO.File.WriteAllText(path,
                    data.GetText(Core.Settings.CSVDelimiter == "" ? Core.OriginalCulture.TextInfo.ListSeparator : Core.Settings.CSVDelimiter),
                    Encoding.GetEncoding(1252));
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error exporting data grid to CSV file: <" + path + ">.", ex);
            }
        }

        public DataTemplate EntityTemplate
        {
            get
            {
                var factory = new FrameworkElementFactory(typeof(TextBlock));
                factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                factory.SetBinding(TextBlock.TextProperty, new Binding(""));
                factory.SetValue(TextBlock.LanguageProperty, XmlLanguage.GetLanguage(Core.OriginalCulture.IetfLanguageTag));

                DataTemplate template = new DataTemplate(typeof(Xceed.Wpf.DataGrid.Column));
                template.VisualTree = factory;

                return template;
            }
        }

        public DataTemplate DataTemplate
        {
            get
            {
                var factory = new FrameworkElementFactory(typeof(TextBlock));
                var binding = new Binding();
                binding.StringFormat = "N" + Core.Settings.TableDisplayPrecision;
                factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                factory.SetBinding(TextBlock.TextProperty, binding);
                factory.SetValue(TextBlock.LanguageProperty, XmlLanguage.GetLanguage(Core.OriginalCulture.IetfLanguageTag));

                DataTemplate template = new DataTemplate(typeof(Xceed.Wpf.DataGrid.Column));
                template.VisualTree = factory;

                return template;
            }
        }

        public void RefreshAvailableTablesUI()
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

                mi.IsChecked = mode.Name == Core.UI_Tableview.CurrentTable?.Name;
                switch (mode.Type)
                {
                    case TableType.Special: mi.Icon = Core.SpecialModeIcon; break;
                    case TableType.Province: mi.Icon = Core.ProvinceModeIcon; break;
                    case TableType.Country: mi.Icon = Core.CountryModeIcon; break;
                }
                mi.Click += TableMenuItem_Click;
                lastSection = mode.Section;
                category.Items.Add(mi);
            }

            if (Core.MainWindow.Ribbon.TablesRibbonTab.Groups.Count > 0 && Core.MainWindow.Ribbon.TablesRibbonTab.Groups[0].Header.ToString() == "Categories")
                Core.MainWindow.Ribbon.TablesRibbonTab.Groups.RemoveAt(0);
            Core.MainWindow.Ribbon.TablesRibbonTab.Groups.Insert(0, gb);
        }

        private static void TableMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var category in Core.MainWindow.Ribbon.TablesRibbonTab.Groups[0].Items)
            {
                foreach (var item in (category as Fluent.DropDownButton).Items)
                {
                    (item as Fluent.MenuItem).IsChecked = false;
                }
            }
            ((Fluent.MenuItem)(sender)).IsChecked = true;
            Core.UI_Tableview.SetCurrentMode((ModeInterfaceElement)((Fluent.MenuItem)(sender)).Tag);
        }
    }
}
