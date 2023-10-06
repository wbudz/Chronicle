using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for UI_Help.xaml
    /// </summary>
    public partial class UI_Help : UserControl
    {
        class Component
        {
            public string Name { get; set; }
            public string Website { get; set; }

            public Component(string name, string website)
            {
                Name = name;
                Website = website;
            }
        }

        public UI_Help()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lComponents.Items.Clear();
            lComponents.Items.Add(new Component("DotNetZip", "http://dotnetzip.codeplex.com/"));
            lComponents.Items.Add(new Component("OxyPlot", "http://oxyplot.codeplex.com/"));
            lComponents.Items.Add(new Component("Fluent Ribbon", "http://fluent.codeplex.com/"));
            lComponents.Items.Add(new Component("Extended WPF Toolkit", "http://wpftoolkit.codeplex.com/"));
            lComponents.Items.Add(new Component("Icons", "http://icons8.com/"));
            lVersion.Content = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void bHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Manual.pdf");
            }
            catch
            {
                Core.Log.ReportError("Could not open manual file.");
            }
        }

        private void bChangelog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Changelog.txt");
            }
            catch
            {
                Core.Log.ReportError("Could not open changelog file.");
            }
        }
    }
}
