using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Debug.xaml
    /// </summary>
    [Serializable]
    public partial class UI_MapStats : UserControl
    {
        Thread StatsThread;
        AutoResetEvent CalcStats;
        List<string> names;
        List<double> values;
        bool finalize;

        public ObservableCollection<StatsEntry> TopEntries { get; set; } = new ObservableCollection<StatsEntry>();
        public ObservableCollection<StatsEntry> BottomEntries { get; set; } = new ObservableCollection<StatsEntry>();

        public UI_MapStats()
        {
            InitializeComponent();
            CalcStats = new AutoResetEvent(false);
            StatsThread = new Thread(CalculateAndDisplay);
            StatsThread.Start();
        }

        public void DisplayStats(IEnumerable<string> names, double[] values)
        {
            if (values == null)
            {
                this.names = names.ToList();
                this.values = null;
                gTotals.Visibility = Visibility.Collapsed;
                gTopEntities.Visibility = Visibility.Collapsed;
                gBottomEntities.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                this.names = names.ToList();
                this.values = values.ToList();
                lTotal.Content = "Total: (calculating)";
                lAverage.Content = "Average: (calculating)";
                lStdDev.Content = "Standard deviation: (calculating)";
                lRange.Content = "Range: (calculating)";

                CalcStats.Set();

                gTotals.Visibility = Visibility.Visible;
                gTopEntities.Visibility = Visibility.Visible;
                gBottomEntities.Visibility = Visibility.Visible;
                return;
            }
        }

        void CalculateAndDisplay()
        {
            while (true)
            {
                CalcStats.WaitOne(250);

                if (finalize || values == null || values.Count < 1)
                {
                    Core.Dispatch.Run(() =>
                    {
                        gTotals.Visibility = Visibility.Collapsed;
                        gTopEntities.Visibility = Visibility.Collapsed;
                        gBottomEntities.Visibility = Visibility.Collapsed;
                    });
                    return;
                }

                string total = values.Sum().ToString("N2");
                string average = (values.Sum() / values.Count).ToString("N2");
                string stddev = (CalculateStdDev(values)).ToString("N2");
                //string gini = (CalculateGiniCoefficient(values)).ToString("N4");
                string range = (values.Max() - values.Min()).ToString("N2") + " (from " + (values.Min()).ToString("N2") + " to " + (values.Max()).ToString("N2") + ")";
                
                Core.Dispatch.Run(() =>
                {
                    lTotal.Content = "Total: " + total;
                    lAverage.Content = "Average: " + average;
                    lStdDev.Content = "Standard deviation: " + stddev;
                    //lGini.Content = "Gini coefficient: " + gini;
                    lRange.Content = "Range: " + range;

                    gTotals.Visibility = Visibility.Visible;
                    gTopEntities.Visibility = Visibility.Visible;
                    gBottomEntities.Visibility = Visibility.Visible;

                    CalculateTopBottomEntities(names, values, 5);
                });
            }
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        private double CalculateGiniCoefficient(IEnumerable<double> values)
        {
            var v = values.ToList();

            //Sorting the Array in ascending order
            v.Sort();

            double m = 0;
            for (int i = 0; i < v.Count; i++)
            {
                m += (v.Count + 1 - i) * v[i];
            }

            return (v.Count + 1 - 2 * (m / v.Sum())) / v.Count;
        }

        private void CalculateTopBottomEntities(List<string> names, List<double> values, int count)
        {
            List<StatsEntry> entries = new List<StatsEntry>();
            for (int i = 0; i < names.Count() && i < values.Count(); i++)
            {
                if (names[i].Contains("(0)")) continue;
                entries.Add(new StatsEntry(names[i], values[i]));
            }

            TopEntries.Clear();
            BottomEntries.Clear();

            entries.Sort(); //ascending
            for (int i = 0; i <= count && i <= entries.Count; i++)
            {
                BottomEntries.Add(entries[i]);
            }

            entries.Reverse(); //descending
            for (int i = 0; i <= count && i <= entries.Count; i++)
            {
                TopEntries.Add(entries[i]);
            }
        }

        public void Finish()
        {
            finalize = true;
            CalcStats.Set();
        }
    }

    public class StatsEntry : IComparable<StatsEntry>
    {
        public string Name
        {
            get; set;
        }

        public double Value
        {
            get; set;
        }

        public StatsEntry(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public int CompareTo(StatsEntry other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
