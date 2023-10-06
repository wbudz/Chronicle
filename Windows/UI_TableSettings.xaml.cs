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
    /// Interaction logic for UI_DeveloperMapmodes.xaml
    /// </summary>
    [Serializable]
    public partial class UI_TableSettings : UserControl
    {
        public UI_TableSettings()
        {
            InitializeComponent();
        }

        private void TableSettingsChecked(object sender, RoutedEventArgs e)
        {
            Core.UI_Tableview.Refresh();
        }
    }
}
