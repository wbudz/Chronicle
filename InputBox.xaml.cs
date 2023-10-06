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
using System.Windows.Shapes;

namespace Enterprise
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string Output
        {
            get;
            set;
        }

        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string label, string prompt):this()
        {
            this.Title = label;
            lLabel.Content = prompt;
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            Output = tInput.Text.Trim();
            DialogResult = true;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
