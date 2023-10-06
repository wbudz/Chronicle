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
    /// Interaction logic for UI_Log.xaml
    /// </summary>
    [Serializable]
    public partial class UI_Log : UserControl
    {
        public UI_Log()
        {
            InitializeComponent();
        }

        public void InitializeLog()
        {
            if (Core.Log == null) return;
            tLog.Clear();
            Core.Log.LogMessage += Log_LogMessage;
        }

        private void Log_LogMessage(object source, Log.LogEventArgs e)
        {
            Task.Run(() => Core.Dispatch.AppendTextbox(tLog, e.Message));
        }

        private void bPush_Click(object sender, RoutedEventArgs e)
        {
            Core.Log.Write("Let's push the tempo!");
        }

        private void bCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tLog.Text);
        }
    }
}
