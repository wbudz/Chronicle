using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Chronicle
{
    public partial class Data
    {
        public bool IsRecording
        {
            get
            {
                return RecordingTimer.IsEnabled;
            }
        }

        string[] monitoredDirectories;
        DateTime[] monitoredDirectoriesLastWrite;
        DispatcherTimer RecordingTimer;

        public void InitializeRecording()
        {
            RecordingTimer = new DispatcherTimer();
            RecordingTimer.Interval = Core.Settings.RecordInterval;
            RecordingTimer.Tick += RecordingTimer_Tick;
        }

        /// <summary>
        /// Starts recording, putting imported savegame data into the given dataset.
        /// </summary>
        /// <param name="dataset">The dataset where new tables should be put.</param>
        public void StartRecording()
        {
            if (Game == null)
            {
                System.Windows.Forms.MessageBox.Show("No game is currently loaded.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            monitoredDirectories = Array.FindAll(Game.GenerateSavegamePaths(), x => Directory.Exists(x)).ToArray();
            monitoredDirectoriesLastWrite = new DateTime[monitoredDirectories.Length];
            for (int i = 0; i < monitoredDirectories.Length; i++)
            {
                Ext.GetDirectoryLastWrite(ref monitoredDirectoriesLastWrite[i], monitoredDirectories[i]);
            }

            RecordingTimer.Start();
            Core.MainWindow.SetProgramStatus(ProgramStatus.Recording);
        }

        /// <summary>
        /// Stops recording.
        /// </summary>
        public void StopRecording()
        {
            RecordingTimer.Stop();
            monitoredDirectories = null;
            Core.MainWindow.SetProgramStatus(ProgramStatus.Idle);
        }

        async void RecordingTimer_Tick(object sender, EventArgs e)
        {
            string path;
            GameDate lastTimepoint = GameDate.Empty;

            for (int i = 0; i < monitoredDirectories.Length; i++)
            {
                if ((path = Ext.GetDirectoryLastWrite(ref monitoredDirectoriesLastWrite[i], monitoredDirectories[i])) != "")
                {
                    RecordingTimer.IsEnabled = false;

                    // File will be read from 0,5s after the last change to hash.
                    await Ext.WaitForWritingFinish(path);
                    await Ext.WaitForFileUnlock(path);
                    await Task.Delay(500);

                    if (Tables == null || Tables.GetTimepoints().Count < 1)
                        lastTimepoint = GameDate.Empty;
                    else
                        lastTimepoint = Tables.GetTimepoints().Last();

                    await Task.Run(() => LoadSavegame(path, true, false));
                    monitoredDirectoriesLastWrite[i] = File.GetLastWriteTime(path); // to ensure that the very last modified time is recorded
                    UnsavedChanges = true;

                    // Autosave, if applicable
                    if (CurrentFile != "" && Core.Settings.Autosave && DateTime.Now - lastSaveTime > Core.Settings.RecordInterval)
                    {
                        await Task.Run(() => SaveAs(CurrentFile));
                    }

                    RecordingTimer.IsEnabled = true;
                    return; // we read the file, it's enough for now
                }
            }
        }
    }

}
