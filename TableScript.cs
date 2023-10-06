using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    public class TableScript : INotifyPropertyChanged
    {
        public string Set { get; set; }
        public string AssemblyPath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Game { get; set; }
        long lastRunTime;
        public long LastRunTime
        {
            get
            {
                return lastRunTime;
            }
            set
            {
                lastRunTime = value;
                OnPropertyChanged("LastRunTime");
            }
        }

        public string Performance
        {
            get
            {
                if (ErrorsCount > 0)
                    return "(error)";
                else if (LastRunTime < 0)
                    return "(not run)";
                else return LastRunTime.ToString();
            }
        }
        public SolidColorBrush PerformanceColor
        {
            get
            {
                if (LastRunTime > 500)
                    return new SolidColorBrush(Color.FromRgb(220,100,100));
                else if (LastRunTime > 100)
                    return new SolidColorBrush(Color.FromRgb(255, 200, 20));
                else return new SolidColorBrush(Color.FromRgb(100, 220, 100));
            }
        }

        public int ErrorsCount { get { return Errors.Count; } }
        public bool IsEnabled { get; set; }
        public List<Exception> Errors { get; set; }
        public string Status { get; set; }

        public TableScript(string name)
        {
            Name = name;
            Set = "";
            AssemblyPath = "";
            Code = "";
            LastRunTime = -1;
            IsEnabled = true;
            Errors = new List<Exception>();
            Status = "Unspecified";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public Table CreateTable()
        {
            try
            {
                if (TableScripts.Assembly == null) return null;
                var t = (Table)Activator.CreateInstance(TableScripts.Assembly.GetType("Chronicle." + Name));
                t.ScriptName = Name;
                t.ScriptSet = Set;
                return t;
            }
            catch (Exception ex)
            {
                throw new Exception("<" + ex.Message + "> at table: <" + ("Chronicle." + Name) + "> in file: <" + AssemblyPath + ">.");
            }
        }

        public override string ToString()
        {
            return Name + " (" + Set + ")";
        }
    }
}
