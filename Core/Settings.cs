using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Chronicle
{
    [Serializable]
    public class Settings
    {
        static readonly string[] presetUpdateLocations = new string[] { "http://codeofwar.wbudziszewski.pl/chronicle_autoupdater.txt" };

        public int LandColor = CEBitmap.Bitmap.ColorToInt32(60, 130, 60);
        public int WaterColor = CEBitmap.Bitmap.ColorToInt32(120, 160, 250);
        public int EmptyColor = CEBitmap.Bitmap.ColorToInt32(128, 128, 128);
        public int SelectedCountryColor = CEBitmap.Bitmap.ColorToInt32(220, 0, 220);
        public int DefaultColor = CEBitmap.Bitmap.ColorToInt32(240, 20, 40);
        public int UpperCutoffColor = CEBitmap.Bitmap.ColorToInt32(205, 205, 205);
        public int LowerCutoffColor = CEBitmap.Bitmap.ColorToInt32(50, 50, 50);
        public int RiverColor = CEBitmap.Bitmap.ColorToInt32(50, 100, 255);
        public int LabelsColor = CEBitmap.Bitmap.ColorToInt32(255, 255, 255);

        public int LandBorderColor = CEBitmap.Bitmap.ColorToInt32(150, 30, 30, 0);
        public int SeaBorderColor = CEBitmap.Bitmap.ColorToInt32(50, 100, 100, 0);
        public int ShoreBorderColor = CEBitmap.Bitmap.ColorToInt32(150, 25, 40, 255);
        public int CountryBorderColor = CEBitmap.Bitmap.ColorToInt32(255, 235, 15, 50);

        public int LandShadeLength = 32;
        public int WaterShadeLength = 32;
        public int LandShadeColor = CEBitmap.Bitmap.ColorToInt32(32, 48, 40, 32);
        public int WaterShadeColor = CEBitmap.Bitmap.ColorToInt32(12, 32, 48, 200);
        public float LandShadeRange = 0.5f;
        public float WaterShadeRange = 0.5f;

        public bool DisplayLandBorders = true;
        public bool DisplaySeaBorders = true;
        public bool DisplayShoreBorders = true;
        public bool DisplayCountryBorders = true;
        public int LabelsDisplayMode = 0;
        public bool LabelsShadows = true;
        public float LabelsOpacity = 1;
        public bool DisplayRivers = true;
        public bool DisplayShading = true;

        public TimeSpan LogInterval = new TimeSpan(0, 0, 1);
        public TimeSpan CacheInterval = new TimeSpan(0, 0, 1);
        public TimeSpan RecordInterval = new TimeSpan(0, 0, 0, 0, 100);
        public TimeSpan AutosaveInterval = new TimeSpan(0, 1, 0);
        public TimeSpan PlaybackInterval = new TimeSpan(0, 0, 1);
        public TimeSpan RefreshInterval = new TimeSpan(0, 0, 0, 0, 5);
        public bool Autosave = false;
        public int MaximumErrorsReported = 3;
        public bool CleanUpOldLogsOnStartup = true;
        public bool ReuseLogFile = true;
        public int SavegameCompressionLevel = 0;
        public string EnforcedSteamID = "";

        public double HighQualityAntialiasThreshold = 1f;
        public double LowQualityAntialiasThreshold = 2f;

        public int TableDisplayPrecision = 2;
        public string CSVDelimiter = "";
        public bool HideEmptyRows = false;
        public bool HideNonExistentCountries = false;
        public bool HideWaterProvinces;

        public int GraphSeriesCount = 8;
        public int[] GraphSeriesColors = new int[10];
        public int GraphSeriesSelectionMethod = 1;
        public int GraphExportWidth = -1;
        public int GraphExportHeight = -1;

        public bool GIFDisplayCaptionBar = true;
        public bool GIFDisplayDate = true;
        public bool GIFDisplayMapmode = true;
        public bool GIFDisplayLogo = true;
        public int GIFZoom = 1;

        public bool NotifyAboutUpdates = true;
        public bool AutocheckForUpdates = true;
        public List<string> UpdateLocations = new List<string>(presetUpdateLocations);

        public List<string> SupportedTableSources = new List<string>();
        public List<string> PreferredScriptSetsOrder = new List<string>();
        public List<string> DisabledScriptSets = new List<string>();
        public bool DebugMode;

        public bool LeftPaneVisible = true;
        public bool RightPaneVisible = true;

        public Settings()
        {
            GraphSeriesColors[0] = CEBitmap.Bitmap.ColorToInt32(Colors.Red);
            GraphSeriesColors[1] = CEBitmap.Bitmap.ColorToInt32(Colors.Green);
            GraphSeriesColors[2] = CEBitmap.Bitmap.ColorToInt32(Colors.AliceBlue);
            GraphSeriesColors[3] = CEBitmap.Bitmap.ColorToInt32(Colors.Plum);
            GraphSeriesColors[4] = CEBitmap.Bitmap.ColorToInt32(Colors.Black);
            GraphSeriesColors[5] = CEBitmap.Bitmap.ColorToInt32(Colors.Firebrick);
            GraphSeriesColors[6] = CEBitmap.Bitmap.ColorToInt32(Colors.LightCoral);
            GraphSeriesColors[7] = CEBitmap.Bitmap.ColorToInt32(Colors.Yellow);
            GraphSeriesColors[8] = CEBitmap.Bitmap.ColorToInt32(Colors.Teal);
            GraphSeriesColors[9] = CEBitmap.Bitmap.ColorToInt32(Colors.Sienna);
        }

        public void EnsureUpdateLocations()
        {
            for (int i = 0; i < presetUpdateLocations.Length; i++)
            {
                if (!UpdateLocations.Contains(presetUpdateLocations[i])) UpdateLocations.Add(presetUpdateLocations[i]);
            }
        }

        public void EnsureMinimalIntervals()
        {

            if (CacheInterval.TotalMilliseconds < 100) CacheInterval = new TimeSpan(0, 0, 0, 0, 100);
            if (LogInterval.TotalMilliseconds < 100) LogInterval = new TimeSpan(0, 0, 0, 0, 100);
            if (RecordInterval.TotalMilliseconds < 100) LogInterval = new TimeSpan(0, 0, 0, 0, 100);
            if (PlaybackInterval.TotalMilliseconds < 250) LogInterval = new TimeSpan(0, 0, 0, 0, 250);
            if (AutosaveInterval.TotalMilliseconds < 1000) LogInterval = new TimeSpan(0, 0, 1, 0, 0);
            if (RefreshInterval.TotalMilliseconds < 1) LogInterval = new TimeSpan(0, 0, 0, 0, 1);
        }

        public void SaveScriptSetsPreferredOrder()
        {
            TableScripts.SaveOrder();
            PreferredScriptSetsOrder.Clear();
            for (int i = 0; i < TableScripts.ScriptSets.Count; i++)
            {
                PreferredScriptSetsOrder.Add(TableScripts.ScriptSets[i].Name);
            }
        }

        public static Settings LoadFromFile(string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Settings)); try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    Settings output = (Settings)deserializer.Deserialize(textReader);
                    Core.Log.Write("Configuration loaded from the file: <" + path + ">.");
                    return output;
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning("Error reading configuration file: <" + path + ">.", ex);
                return new Settings();
            }
        }

        public static void SaveToFile(Settings settings, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (TextWriter textWriter = new StreamWriter(path))
            {
                try
                {
                    serializer.Serialize(textWriter, settings);
                    Core.Log.Write("Settings written to the configuration file: <" + path + ">.");
                }
                catch (Exception ex)
                {
                    Core.Log.ReportWarning("Error writing setting to the configuration file: <" + path + ">. Settings were not saved. Exception: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static Settings Clone(Settings source)
        {
            if (!typeof(Settings).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(Settings);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (Settings)formatter.Deserialize(stream);
            }
        }
    }
}
