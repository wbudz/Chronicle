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

namespace Chronicle
{
    public partial class Data
    {
        public void EnumerateSavegames(InstalledGame game, ListView listview)
        {
            List<string> files = new List<string>();

            string[] dirs = game.GenerateSavegamePaths();
            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    files.AddRange(Directory.GetFiles(dir, "*." + game.Game.SavegameExtension));
                }
            }
            SavegameEntry[] entries = new SavegameEntry[files.Count];


            for (int i = 0; i < files.Count; i++)
            {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                bool binary = Core.IsBinarySavegame(files[i]);
                bool zipped = Core.IsCompressedSavegame(files[i]);
                DateTime lastSaved = new FileInfo(files[i]).LastWriteTime;
                GameDate gameDate = Core.GetGameDate(game, files[i]);
                string path = files[i];

                entries[i] = new SavegameEntry(name, binary, zipped, lastSaved, gameDate, path);
            }

            Array.Sort(entries, new SavegameEntryGameDateComparer());

            Core.Dispatch.DisplaySavegames(listview, entries);
        }
    }

    public class SavegameEntry
    {
        public string Name { get; set; }
        public BitmapImage BinaryIcon { get; set; }
        public string BinaryText { get; set; }
        public BitmapImage ZippedIcon { get; set; }
        public string ZippedText { get; set; }
        public string LastSaved { get; set; }
        public GameDate GameDate { get; set; }
        public string GameDateText { get; set; }
        public string Path { get; set; }

        public SavegameEntry(string name, bool binary, bool zipped, DateTime lastSaved, GameDate gameDate, string path)
        {
            Name = name;
            LastSaved = lastSaved.ToString("yyyy-MM-dd hh:mm");
            GameDate = gameDate;
            GameDateText = gameDate.GetString("yyyy-MM-dd");
            Path = path;

            BinaryIcon = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/" + (binary ? "binary" : "text") + "-16.png"));
            BinaryIcon.Freeze();
            BinaryText = binary ? "Yes" : "No";

            ZippedIcon = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/Icons/" + (zipped ? "zipped" : "unzipped") + "-16.png"));
            ZippedText = zipped ? "Yes" : "No";
            ZippedIcon.Freeze();
        }
    }

    public class SavegameEntryGameDateComparer : IComparer<SavegameEntry>
    {
        public int Compare(SavegameEntry a, SavegameEntry b)
        {
            return a.GameDate.CompareTo(b.GameDate);
        }
    }
}
