using System;
using System.Collections.Generic;
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
using Ionic.Zip;
using Microsoft.Win32;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_NewTableScriptSet.xaml
    /// </summary>
    public partial class UI_NewTableScriptSet : Window
    {
        public TableScriptSet TableScriptSet { get; set; }

        OpenFileDialog ofd = new OpenFileDialog();
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        OpenFileDialog ozd = new OpenFileDialog();

        string path;

        public UI_NewTableScriptSet()
        {
            InitializeComponent();

            ofd.Filter = "Text files (*.txt)|*.txt|Code files (*.cs)|*.cs|All files (*.*)|*.*";
            ozd.Filter = "ZIP archives (*.zip)|*.zip|All files (*.*)|*.*";
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            string targetPath = "Script";

            if (tName.Text.Trim().ToLower().StartsWith("Chronicle."))
            {
                MessageBox.Show("Table script sets starting with 'Chronicle.' are reserved for built-in types.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Directory.Exists(path))
            {
                targetPath = Path.Combine(targetPath, Path.GetFileName(Path.GetDirectoryName(path)));
                if (Directory.Exists(targetPath))
                {
                    if (MessageBox.Show("There is already a directory of specified name. Do you want to copy the files, possibly overwriting existing ones?", "Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.No)
                        return;
                }
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(path, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(path, targetPath));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(path, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(path, targetPath), true);
            }
            else if (File.Exists(path))
            {
                targetPath = Path.Combine(targetPath, tName.Text.Trim());
                if (tName.Text.Trim().IndexOfAny(System.IO.Path.GetInvalidPathChars()) > -1)
                {
                    MessageBox.Show("Specified name contains illegal characters.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (File.Exists(targetPath))
                {
                    if (MessageBox.Show("There is already a file of specified name. Do you want to copy the files, possibly overwriting existing ones?", "Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.No)
                        return;
                }

                if (path.ToLowerInvariant().EndsWith(".zip"))
                {
                    ZipFile zf = new ZipFile(path);
                    zf.ExtractAll(Path.Combine(targetPath, Path.GetFileName(path)));
                }
                else
                {
                    File.Copy(path, Path.Combine(targetPath, Path.GetFileName(path)));
                }
            }
            else
            {
                MessageBox.Show("File or directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void bFromFile_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                lPath.Content = "Path: " + path;
                tName.Text = Path.GetFileName(Path.GetDirectoryName(path));
                tName.IsEnabled = true;
            }
        }

        private void bFromFolder_Click(object sender, RoutedEventArgs e)
        {
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
                lPath.Content = "Path: " + path;
                tName.Text = "";
                tName.IsEnabled = false;
            }
        }

        private void bFromZIP_Click(object sender, RoutedEventArgs e)
        {
            if (ozd.ShowDialog() == true)
            {
                path = ofd.FileName;
                lPath.Content = "Path: " + path;
                tName.Text = Path.GetFileNameWithoutExtension(path);
                tName.IsEnabled = true;
            }
        }
    }
}
