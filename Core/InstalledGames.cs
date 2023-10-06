using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Chronicle
{
    public class InstalledGames
    {
        private List<InstalledGame> games = new List<InstalledGame>();

        public int Count { get { return games.Count; } }

        /// <summary>
        /// Initializes information about installed games.
        /// </summary>
        public InstalledGames()
        {

        }

        /// <summary>
        /// Initializes information about installed games, using already existing information as a source.
        /// </summary>
        public InstalledGames(InstalledGames copy)
        {
            games = new List<InstalledGame>(copy.games);
        }

        public Button[] PrepareControls(UI_Import parent, int selectedIndex)
        {
            List<Button> output = new List<Button>();

            for (int i = 0; i < games.Count; i++)
            {
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Chronicle;component/GameIcons/" + games[i].Token + "-32.png")),
                    Stretch = Stretch.None,
                    Margin = new System.Windows.Thickness(5)
                };
                var label = new Label()
                {
                    Content = games[i].Name,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new System.Windows.Thickness(5)
                };
                var panel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                var button = new Button()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    Margin = new System.Windows.Thickness(5),
                    ToolTip = games[i].Directory,
                    Tag = games[i],
                    TabIndex = i
                };

                panel.Children.Add(image);
                panel.Children.Add(label);
                button.Content = panel;
                SetSelectedStatus(button, i == selectedIndex);

                button.Click += parent.GameButton_Click;

                output.Add(button);
            }

            return output.ToArray();
        }

        public IEnumerable<string> GetList()
        {
            for (int i = 0; i < games.Count; i++)
            {
                yield return games[i].Name;
            }
        }

        public void SetSelectedStatus(Button button, bool selected)
        {
            var gameInactiveFontBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            var gameInactiveBgBrush = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
            var gameInactiveBorderBrush = new SolidColorBrush(Color.FromArgb(255, 112, 112, 112));
            var gameSelectedFontBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            var gameSelectedBgBrush = new SolidColorBrush(Color.FromArgb(255, 25, 121, 202));
            var gameSelectedBorderBrush = new SolidColorBrush(Color.FromArgb(255, 112, 112, 112));

            try
            {
                (button.Content as StackPanel).Background = selected ? gameSelectedBgBrush : gameInactiveBgBrush;
                ((button.Content as StackPanel).Children[1] as Label).Foreground = selected ? gameSelectedFontBrush : gameInactiveFontBrush;
            }
            catch (Exception ex)
            {
                Core.Log.WriteWarning("Failed to set selected status to a game button.", ex);
            }
        }

        /// <summary>
        /// Loads information about installed games from an external XML file.
        /// </summary>
        /// <param name="path">XML file path</param>
        public void Load(string path)
        {
            try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<InstalledGame>));
                    games = (List<InstalledGame>)deserializer.Deserialize(textReader);
                    Core.Log.Write("Installed games database loaded from disk: <" + path + ">.");
                }
            }
            catch (Exception ex)
            {
                games = new List<InstalledGame>();
                Core.Log.ReportWarning("Error reading installed games database from disk: <" + path + ">.", ex);
            }
        }

        /// <summary>
        /// Saves information about installed games to an external XML file.
        /// </summary>
        /// <param name="path">XML file path</param>
        public void Save(string path)
        {
            try
            {
                using (TextWriter textWriter = new StreamWriter(path))
                {

                    XmlSerializer serializer = new XmlSerializer(typeof(List<InstalledGame>));
                    serializer.Serialize(textWriter, games);
                    Core.Log.Write("Installed games data written to disk: <" + path + ">.");
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportWarning("Error writing installed games data to disk: <" + path + ">. Settings were not saved. ", ex);
            }
        }

        public InstalledGame this[int i]
        {
            get { return this.games[i]; }
        }

        public bool Add(InstalledGame game, bool suppressErrors)
        {
            if (games.Exists(x => x.Name == game.Name))
            {
                if (!suppressErrors)
                    Core.Log.ReportError("A game with given name already exists. Provide another name for the game.");
                return false;
            }

            games.Add(game);
            return true;
        }

        public void Edit(InstalledGame game, int index)
        {
            games[index] = new InstalledGame(game);
        }

        public void RemoveAt(int index)
        {
            games.RemoveAt(index);
        }

        public InstalledGame GetBySavegameExtension(string ext)
        {
            return games.Find(x => x.Game.SavegameExtension == ext);
        }

        public void Initialize()
        {
            foreach (var g in games)
            {
                g.RefreshModList();
            }
        }
    }
}
