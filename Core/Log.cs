using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chronicle
{
    /// <summary>
    /// Manages logging.
    /// </summary>
    public class Log
    {
        StreamWriter txt;
        public string Path { get; set; }

        DispatcherTimer timer = new DispatcherTimer();
        ConcurrentQueue<string> q = new ConcurrentQueue<string>();
        StringBuilder sb = new StringBuilder();

        int ErrorsReported = 0;

        /// <summary>
        /// Initializes the log file.
        /// </summary>
        public Log(string fileOutputPath)
        {
            if (Core.Settings.CleanUpOldLogsOnStartup)
                Core.AttemptDelete(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileOutputPath), "Log*"), true);

            try
            {
                if (Core.Settings.ReuseLogFile)
                    Path = fileOutputPath;
                else
                    Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileNameWithoutExtension(Path) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + System.IO.Path.GetExtension(Path));
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path))) Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                txt = new StreamWriter(Path, false);
                txt.AutoFlush = true;
            }
            catch
            {
                try
                {
                    Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileNameWithoutExtension(Path) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + System.IO.Path.GetExtension(Path));
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path))) Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                    txt = new StreamWriter(Path, false);
                    txt.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    Core.Dispatch.DisplayMessageBox("Log file cannot be initialized. Exception thrown: " + ex.Message + "\n\nNo data will be logged during this session.",
                          "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    Path = "";
                }
            }
        }

        public void InitializeLog()
        {
            // We presume that the event(s) are handled by now.
            Write("Chronicle " + Core.Version, true);
            Write("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), true);
            Write("", true);
            Write(Path == "" ? "Log file could not be initialized; no logging data will be written to the disk." : "Log file initialized: <" + Path + ">.");

            // Set up the timer.

            timer.Tick += timer_Tick;
            timer.Interval = Core.Settings.LogInterval;
            timer.Start();
            Write("Log timer initialized to interval: " + timer.Interval.TotalMilliseconds + " ms.");
        }

        void timer_Tick(object sender, EventArgs e)
        {
            sb.Clear();
            string text;
            while (q.TryDequeue(out text))
            {
                sb.AppendLine(text);
            }
            text = sb.ToString();
            if (text == "") return;

            OnLog(text);

            if (Path != "")
                txt.Write(text);
        }

        public void ReportError(string message, Exception ex)
        {
            if (ex != null)
            {
                // Log error message
                WriteError(message + "|| Exception message: " + ex.Message.Replace("\n", ">> ") + "|| stack trace: " + ex.StackTrace.Replace("\n", ">> "));

                // Display error window.
                if (!SuppressErrorMessages)
                    Core.Dispatch.DisplayMessageBox(message + "|| Exception message: " + ex.Message + "|| stack trace: " + ex.StackTrace,
                        "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            else
            {
                // Log error message
                WriteError(message);

                // Display error window.
                if (!SuppressErrorMessages)
                    Core.Dispatch.DisplayMessageBox(message,
                        "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            ErrorsReported++;
        }

        public void ReportError(string message)
        {
            ReportError(message, null);
        }

        public void ReportWarning(string message, Exception ex)
        {
            if (ex != null)
            {
                // Log error message
                WriteWarning(message + "|| Exception message: " + ex.Message.Replace("\n", ">> ") + "|| stack trace: " + ex.StackTrace.Replace("\n", ">> "));

                // Display error window.
                Core.Dispatch.DisplayMessageBox(message + "|| Exception message: " + ex.Message + "|| stack trace: " + ex.StackTrace,
                    "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            else
            {
                // Log error message
                WriteWarning(message);

                // Display error window.
                Core.Dispatch.DisplayMessageBox(message, "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public void ReportWarning(string message)
        {
            ReportWarning(message, null);
        }

        /// <summary>
        /// Writes an error information to the log file.
        /// </summary>
        /// <param name="text">Text to be added to the log file</param>
        public void WriteError(string text)
        {
            q.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + ": ERROR: " + text);
        }

        public void WriteError(string text, Exception ex)
        {
            q.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + ": ERROR: " + text + "|| Exception message: " + ex.Message.Replace("\n", ">> ") + "|| stack trace: " + ex.StackTrace.Replace("\n", ">> "));
        }

        /// <summary>
        /// Writes a warning information to the log file.
        /// </summary>
        /// <param name="verbosity">Verbosity: 0-critical, 1-important, 2-normal, 3-detail</param>
        /// <param name="text">Text to be added to the log file</param>
        public void WriteWarning(string text)
        {
            q.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + ": WARNING: " + text);
        }

        public void WriteWarning(string text, Exception ex)
        {
            q.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + ": WARNING: " + text + "|| Exception message: " + ex.Message.Replace("\n", ">> ") + "|| stack trace: " + ex.StackTrace.Replace("\n", ">> "));
        }

        /// <summary>
        /// Writes information to the log file.
        /// </summary>
        /// <param name="text">Text to be added to the log file</param>
        public void Write(string text, bool hideTime)
        {
            q.Enqueue((hideTime ? "" : (DateTime.Now.ToString("HH:mm:ss.fff") + ": ")) + text);
        }

        public void Write(string text)
        {
            Write(text, false);
        }

        /// <summary>
        /// Finalizes log writing all the pending information to the file.
        /// </summary>
        public void FinalizeLog()
        {
            try
            {
                timer.Stop();
                Write("Log file finalized.");
                timer_Tick(this, new EventArgs());
                txt.Close();
            }
            catch
            { }
        }

        public bool SuppressErrorMessages
        {
            get
            {
                return Core.Settings.MaximumErrorsReported < 0 ? false : ErrorsReported >= Core.Settings.MaximumErrorsReported;
            }
        }

        #region Event handling

        public delegate void LogHandler(object source, LogEventArgs e);

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(string message)
            {
                Message = message;
            }
            public string Message { get; private set; }
        }

        public event LogHandler LogMessage;

        private void OnLog(string message)
        {
            if (LogMessage != null)
                LogMessage(this, new LogEventArgs(message));
        }

        #endregion
    }
}
