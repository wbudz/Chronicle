using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for UI_Developer.xaml
    /// </summary>
    [Serializable]
    public partial class UI_Developer : UserControl
    {
        public UI_Developer()
        {
            InitializeComponent();

            lSets.ItemsSource = TableScripts.ScriptSets;
        }

        public void Refresh()
        {
            Core.Dispatch.Run(() =>
            {
                //
            });
        }

        public void Reset()
        {
            Refresh();
        }

        private void bInstallNew_Click(object sender, RoutedEventArgs e)
        {
            UI_NewTableScriptSet wnd = new UI_NewTableScriptSet();
            wnd.ShowDialog();
            if (wnd.DialogResult == true)
            {
                TableScripts.ScriptSets.Add(wnd.TableScriptSet);
                pRecompilationWarning.Visibility = Visibility.Visible;
            }
        }

        private void bDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lSets.SelectedItem == null) return;
            if ((lSets.SelectedItem as TableScriptSet).Name.StartsWith("Chronicle."))
            {
                MessageBox.Show("Built-in script sets cannot be deleted.", "Delete", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show("Do you want to delete the selected scripts set? This cannot be undone.", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete((lSets.SelectedItem as TableScriptSet).Path);
                }
                catch
                {
                    MessageBox.Show("Error deleting script set directory. Directory may be in use.", "Delete", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                TableScripts.ScriptSets.Remove(lSets.SelectedItem as TableScriptSet);
                pRecompilationWarning.Visibility = Visibility.Visible;
            }
        }

        private void bOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (lSets.SelectedItem == null) return;
            Process.Start((lSets.SelectedItem as TableScriptSet).Path);
        }

        private void bRename_Click(object sender, RoutedEventArgs e)
        {
            if (lSets.SelectedItem == null) return;
            InputBox input = new InputBox("Name", "Enter new name");
            if (input.ShowDialog() == true)
            {
                if (input.Output.StartsWith("Chronicle."))
                {
                    MessageBox.Show("Table script sets starting with 'Chronicle.' are reserved for built-in types.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (input.Output.IndexOfAny(System.IO.Path.GetInvalidPathChars()) > -1)
                {
                    MessageBox.Show("Specified name contains illegal characters.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                (lSets.SelectedItem as TableScriptSet).Name = input.Output.Trim();
            }
        }

        private void bReloadScripts_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.SaveScriptSetsPreferredOrder();
            TableScripts.Initialize();
            TableScripts.Compile();

            pRecompilationWarning.Visibility = Visibility.Collapsed;
            Core.Dispatch.DisplayMessageBox("Table scripts recompiled.", "Table scripts", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void lSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bMoveUp.IsEnabled = lSets.SelectedIndex > 0;
            bMoveDown.IsEnabled = lSets.SelectedIndex != -1 && lSets.SelectedIndex < lSets.Items.Count - 1;            
        }

        private void bMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lSets.SelectedItem == null || lSets.SelectedIndex == 0) return;
            TableScripts.ScriptSets.Insert(lSets.SelectedIndex - 1, lSets.SelectedItem as TableScriptSet);
            TableScripts.ScriptSets.RemoveAt(lSets.SelectedIndex);

            pRecompilationWarning.Visibility = Visibility.Visible;
        }

        private void bMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lSets.SelectedItem == null || lSets.SelectedIndex == lSets.Items.Count - 1) return;
            TableScripts.ScriptSets.Insert(lSets.SelectedIndex + 2, lSets.SelectedItem as TableScriptSet);
            TableScripts.ScriptSets.RemoveAt(lSets.SelectedIndex);

            pRecompilationWarning.Visibility = Visibility.Visible;
        }

        private void bCode_Click(object sender, RoutedEventArgs e)
        {
            if (lCompiledScripts.SelectedItem == null) return;
            Core.Dispatch.DisplayMessageBox((lCompiledScripts.SelectedItem as TableScript).Code, "Code", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void bErrors_Click(object sender, RoutedEventArgs e)
        {
            if (lCompiledScripts.SelectedItem == null) return;
            if ((lCompiledScripts.SelectedItem as TableScript).Errors.Count == 0)
            {
                Core.Dispatch.DisplayMessageBox("No errors encountered.", "Errors", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Errors:");
                sb.AppendLine();
                for (int i = 0; i < (lCompiledScripts.SelectedItem as TableScript).Errors.Count; i++)
                {
                    sb.AppendLine((i + 1) + ". " + (lCompiledScripts.SelectedItem as TableScript).Errors[i].Message);
                    sb.AppendLine("Stack trace: " + (lCompiledScripts.SelectedItem as TableScript).Errors[i].StackTrace);
                    sb.AppendLine();
                }
                Core.Dispatch.DisplayMessageBox(sb.ToString(), "Errors", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        private void lCompiledScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bCode.IsEnabled = lCompiledScripts.SelectedItem != null;
            bErrors.IsEnabled = lCompiledScripts.SelectedItem != null;
        }

        private void ScriptSetCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Set disabled scripts
            foreach (var s in (lSets.ItemsSource as ObservableCollection<TableScriptSet>))
            {
                if (!s.IsEnabled && !Core.Settings.DisabledScriptSets.Contains(s.Name))
                {
                    Core.Settings.DisabledScriptSets.Add(s.Name);
                }
                if (s.IsEnabled && Core.Settings.DisabledScriptSets.Contains(s.Name))
                {
                    Core.Settings.DisabledScriptSets.Remove(s.Name);
                }
            }
        }

        private void ScriptSetCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Set disabled scripts
            foreach (var s in (lSets.ItemsSource as ObservableCollection<TableScriptSet>))
            {
                if (!s.IsEnabled && !Core.Settings.DisabledScriptSets.Contains(s.Name))
                {
                    Core.Settings.DisabledScriptSets.Add(s.Name);
                }
                if (s.IsEnabled && Core.Settings.DisabledScriptSets.Contains(s.Name))
                {
                    Core.Settings.DisabledScriptSets.Remove(s.Name);
                }
            }
        }
    }
}
