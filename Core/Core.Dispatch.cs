using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chronicle
{
    public static partial class Core
    {
        public static class Dispatch
        {
            public static void Run(Action action)
            {
                if (Application.Current == null) return;
                Application.Current.Dispatcher.Invoke(action);
            }



            public static void DisplayMessageBox(string message, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon image)
            {
                MainWindow.Dispatcher.BeginInvoke(new DisplayMessageCallback(_DisplayMessageBox), new object[] { message, caption, buttons, image });
            }
            public delegate void DisplayMessageCallback(string message, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon image);
            private static void _DisplayMessageBox(string message, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon image)
            {
                System.Windows.Forms.MessageBox.Show(message, caption, buttons, image);
            }



            public static void AppendTextbox(TextBox control, string message)
            {
                control.Dispatcher.BeginInvoke(new AppendTextboxCallback(_AppendTextbox), new object[] { control, message });
            }
            public delegate void AppendTextboxCallback(TextBox control, string message);
            private static void _AppendTextbox(TextBox control, string message)
            {
                control.AppendText(message);
                control.ScrollToEnd();
            }



            public static void DisplaySavegames(ListView control, SavegameEntry[] entries)
            {
                control.Dispatcher.BeginInvoke(new DisplaySavegamesCallback(_DisplaySavegames), new object[] { control, entries });
            }
            public delegate void DisplaySavegamesCallback(ListView control, SavegameEntry[] entries);
            private static void _DisplaySavegames(ListView control, SavegameEntry[] entries)
            {
                control.ItemsSource = entries;
            }



            public static void HideProgress()
            {
                MainWindow.bProgress.Dispatcher.BeginInvoke(new DisplayProgressCallback(_DisplayProgress), new object[] { "", -1 });
            }
            public static void DisplayProgress(string message)
            {
                MainWindow.bProgress.Dispatcher.BeginInvoke(new DisplayProgressCallback(_DisplayProgress), new object[] { message, -1 });
            }
            public static void DisplayProgress(string message, double progress)
            {
                MainWindow.bProgress.Dispatcher.BeginInvoke(new DisplayProgressCallback(_DisplayProgress), new object[] { message, progress });
            }
            public delegate void DisplayProgressCallback(string message, double progress);
            private static void _DisplayProgress(string message, double progress)
            {
                if (progress < 0)
                {
                    MainWindow.bProgress.Visibility = Visibility.Collapsed;
                    MainWindow.bProgress.Value = 0;
                }
                else
                {
                    MainWindow.bProgress.Visibility = Visibility.Visible;
                    MainWindow.bProgress.Value = progress;
                }

                if (message == "")
                {
                    MainWindow.lProgress.Visibility = Visibility.Collapsed;
                    MainWindow.lProgress.Content = "";
                }
                else
                {
                    MainWindow.lProgress.Visibility = Visibility.Visible;
                    MainWindow.lProgress.Content = message;
                }
            }

            

            public static void EnableUI()
            {
                MainWindow.Dispatcher.BeginInvoke(new EnableDisableUICallback(_EnableDisableUI), new object[] { true });
            }
            public static void DisableUI()
            {
                MainWindow.Dispatcher.BeginInvoke(new EnableDisableUICallback(_EnableDisableUI), new object[] { false });
            }
            public delegate void EnableDisableUICallback(bool value);
            private static void _EnableDisableUI(bool value)
            {
                MainWindow.IsEnabled = value;
            }
        }
    }
}
