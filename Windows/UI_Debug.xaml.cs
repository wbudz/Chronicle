using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Debug.xaml
    /// </summary>
    [Serializable]
    public partial class UI_Debug : UserControl
    {
        public class Property : INotifyPropertyChanged
        {
            string name;
            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }

            string value;
            public string Value
            {
                get
                {
                    return value;
                }
                set
                {
                    this.value = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Value"));
                }
            }

            public Property(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, e);
                }
            }
        }

        public ObservableCollection<Property> Properties;

        public UI_Debug()
        {
            InitializeComponent();

            Properties = new ObservableCollection<Property>();
            lProperties.ItemsSource = Properties;
        }

        private void bOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            switch (cFolders.SelectedIndex)
            {
                case 1: Process.Start(Core.Paths.AppData); break;
                case 2: Process.Start(Core.Paths.LocalAppData); break;
                default: Process.Start(Core.Paths.Application); break;
            }
        }

        public void RefreshCacheStats(int cachedTables, int totalTables, float cacheSize, int lastCached, int lastUncached)
        {
            lCacheTablesCount.Content = "Cached tables: " + cachedTables + " (out of " + totalTables + " tables)";
            lCacheTablesSize.Content = "Cached size: " + cacheSize.ToString("N2") + " MB";
            lLastRun.Content = "Last run results: " + lastCached + " loaded, " + lastUncached + " disposed";
        }

        public void SetProperty(string name, object value)
        {
            Property p = Properties.FirstOrDefault(x => x.Name == name);
            if (p == null)
            {
                Properties.Add(new Property(name, value.ToString()));
            }
            else
            {
                p.Value = value.ToString();
            }
        }
    }
}
