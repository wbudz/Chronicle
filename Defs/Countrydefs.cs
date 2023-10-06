    using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CEParser;

namespace Chronicle
{
    public class Countrydefs : MarshalByRefObject
    {
        List<Country> countries = new List<Country>();
        Dictionary<string, ushort> countryID = new Dictionary<string, ushort>();

        public int Count { get { return countries.Count; } }

        public Countrydefs(InstalledGame game, string[] paths, IEnumerable<Mod> mods)
        {
            List<string> consumedFilenames = new List<string>();
            Core.Log.Write("Reading country definitions from definition files...");

            for (int i = paths.Length - 1; i >= 0; i--) // we start from the last entry because mod files are preferred over vanilla ones
            {
                string path = paths[i];
                if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                try
                {
                    if (game.Game.ClausewitzEngineVersion == 1)
                    {
                        CEParser.File file = new CEParser.TextFile(path);
                        file.Parse();

                        // Add an empty country
                        countries.Add(new Country(""));

                        var cDefs = file.GetAttributes(file.Root);

                        foreach (var cDef in cDefs)
                        {
                            if (cDef.Key == "dynamic_tags") continue; // ignore "dynamic_tags"

                            Country c = new Country(cDef.Key);

                            // Load specific definition file to get a color

                            string p = Core.GetPath(game, "common\\" + cDef.Value.Replace('/', '\\').Replace("\"", ""), mods);
                            if (!System.IO.File.Exists(p)) continue;
                            CEParser.File deffile = new CEParser.TextFile(p);
                            deffile.Parse();

                            string[] colorDef = deffile.GetEntries(deffile.GetSubnode(deffile.Root, "color"));
                            c.Color = CEBitmap.Bitmap.StringToColor(colorDef);

                            AddCountry(c);
                        }
                    }
                    if (game.Game.ClausewitzEngineVersion == 2)
                    {
                        CEParser.File file = new CEParser.TextFile(path);
                        file.Parse();

                        // Add an empty country
                        countries.Add(new Country(""));

                        CEParser.Node[] allnodes = file.GetSubnodes(file.Root, true).ToArray();
                        CEParser.Node[] nodes = allnodes.Where(x =>
                           (file.GetNodeName(x).StartsWith("e_") || file.GetNodeName(x).StartsWith("k_") || file.GetNodeName(x).StartsWith("d_") || file.GetNodeName(x).StartsWith("c_")) &&
                           !file.HasAParent(x, "allow")).ToArray();

                        foreach (var cDef in nodes)
                        {
                            Country c = new Country(file.GetNodeName(cDef));

                            // Load color
                            if (file.HasAContainer(cDef, "color"))
                            {
                                string[] colorDef = file.GetEntries(file.GetSubnode(cDef, "color"));
                                c.Color = CEBitmap.Bitmap.StringToColor(colorDef);
                            }

                            AddCountry(c);
                        }
                    }
                    if (game.Game.ClausewitzEngineVersion == 3)
                    {
                        CEParser.File file = new CEParser.TextFile(path);
                        file.Parse();

                        // Add an empty country
                        countries.Add(new Country(""));

                        var cDefs = file.GetAttributes(file.Root);

                        foreach (var cDef in cDefs)
                        {
                            if (cDef.Key == "dynamic_tags") continue; // ignore "dynamic_tags"

                            Country c = new Country(cDef.Key);

                            // Load color
                            string p = Core.GetPath(game, "common\\" + cDef.Value.Replace('/', '\\').Replace("\"", ""), mods);
                            if (!System.IO.File.Exists(p)) continue;
                            CEParser.File deffile = new CEParser.TextFile(p);
                            deffile.Parse();

                            string[] colorDef = deffile.GetEntries(deffile.GetSubnode(deffile.Root, "color"));
                            c.Color = CEBitmap.Bitmap.StringToColor(colorDef);

                            AddCountry(c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error reading country definitions from <" + path + "> file.", ex);
                    return;
                }

                consumedFilenames.Add(Path.GetFileName(path));
            }
        }

        public Countrydefs(byte[] raw)
        {
            Core.Log.Write("Reading country definitions from cache...");
            Load(raw);
        }

        public ushort GetIndex(string tag)
        {
            ushort index;
            countryID.TryGetValue(tag, out index);
            return index;
        }

        public string GetTag(ushort index)
        {
            if (index >= countries.Count) return "";
            return countries[index].Tag;
        }

        public string GetName(ushort index, string format)
        {
            if (index >= countries.Count) return "";
            return String.Format(format, countries[index].Tag, countries[index].Name);
        }

        public IEnumerable<string> List(string format)
        {
            for (int i = 0; i < countries.Count; i++)
            {
                yield return String.Format(format, countries[i].Tag, countries[i].Name);
            }
        }

        public Color GetColor(ushort id)
        {
            if (id >= countries.Count) return new Color();
            return countries[id].Color;
        }

        public void AddDynamicTags(string token, CEParser.File file)
        {
            if (token == "ck2")
            {
                CEParser.Node[] nodes = file.GetSubnodes(file.Root, "title", "*").ToArray();
                foreach (var c in nodes)
                {
                    if (countries.Find(x => x.Tag == file.GetNodeName(c)) != null) continue;
                    Country newCountry = new Country(file.GetNodeName(c));
                    newCountry.Name = file.GetAttributeValue(c, "name");

                    // Load color
                    if (file.HasAContainer(c, "color"))
                    {
                        string[] colorDef = file.GetEntries(file.GetSubnode(c, "color"));
                        newCountry.Color = CEBitmap.Bitmap.StringToColor(colorDef);
                    }

                    AddCountry(newCountry);
                }
            }
            else if (token == "eu4")
            {
                CEParser.Node[] nodes = file.GetSubnodes(file.Root, "countries", "*").ToArray();
                foreach (var c in nodes)
                {
                    if (countries.Find(x => x.Tag == file.GetNodeName(c)) != null) continue;
                    Country newCountry = new Country(file.GetNodeName(c));
                    newCountry.Name = file.GetAttributeValue(c, "name");

                    // Load color
                    if (file.HasAContainer(c, "country_color"))
                    {
                        string[] colorDef = file.GetEntries(file.GetSubnode(c, "country_color"));
                        newCountry.Color = CEBitmap.Bitmap.StringToColor(colorDef);
                    }

                    AddCountry(newCountry);
                }
            }
        }

        private void Load(byte[] raw)
        {
            countries = new List<Country>();

            int index = 0;
            int len = 0;
            string str;
            Country c;

            while (index < raw.Length)
            {
                len = raw[index++];
                str = len == 0 ? "" : Encoding.UTF8.GetString(raw, index, len);
                index += len;
                c = new Country(str);

                len = raw[index++];
                str = len == 0 ? "" : Encoding.UTF8.GetString(raw, index, len);
                index += len;
                c.Name = str;

                c.Color = Color.FromRgb(raw[index++], raw[index++], raw[index++]);
                c.Special = BitConverter.ToBoolean(raw, index++);

                countries.Add(c);
            }

            // Rebuild dictionary
            countryID.Clear();
            for (ushort i = 0; i < countries.Count; i++)
            {
                countryID.Add(countries[i].Tag, i);
            }
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter b = new BinaryWriter(ms))
                {
                    foreach (var c in countries)
                    {
                        b.Write((byte)c.Tag.Length);
                        b.Write(Encoding.UTF8.GetBytes(c.Tag));
                        b.Write((byte)Encoding.UTF8.GetByteCount(c.Name));
                        b.Write(Encoding.UTF8.GetBytes(c.Name));
                        b.Write(c.Color.R);
                        b.Write(c.Color.G);
                        b.Write(c.Color.B);
                        b.Write(c.Special);
                    }
                    return ms.ToArray();
                }
            }
        }

        void AddCountry(Country c)
        {
            ushort index;
            countryID.TryGetValue(c.Tag, out index);

            if (index > 0)
            {
                countries[index] = c;
            }
            else
            {
                countries.Add(c);
                countryID.Add(c.Tag, (ushort)(countries.Count - 1));
            }
        }

        public void LoadNames(InstalledGame game, List<string> locale)
        {
            if (game.Game.ClausewitzEngineVersion == 1)
            {
                for (int i = 0; i < locale.Count; i++)
                {
                    if (locale[i].Length > 3 && locale[i][3] == ';')
                    {
                        string[] def = locale[i].Split(';');
                        if (def == null || !countryID.ContainsKey(def[0]) || def.Length < 2) continue;
                        countries[countryID[def[0]]].Name = def[1];
                    }
                }
            }
            else if (game.Game.ClausewitzEngineVersion == 2)
            {
                for (int i = 0; i < locale.Count; i++)
                {
                    if (locale[i].Length > 1 && locale[i][1] == '_')
                    {
                        string[] def = locale[i].Split(';');
                        if (def == null || !countryID.ContainsKey(def[0]) || def.Length < 2) continue;
                        countries[countryID[def[0]]].Name = def[1];
                    }
                }
            }
            else if (game.Game.ClausewitzEngineVersion == 3)
            {
                for (int i = 0; i < locale.Count; i++)
                {
                    if (locale[i].Length > 4 && locale[i][4] == ':')
                    {
                        string[] def = locale[i].Split(':');
                        if (def == null || def.Length < 2) continue; // !countryID.ContainsKey(def[0]) || 
                        int c = countries.FindIndex(x => x.Tag.Length == 3 && x.Tag[0] == def[0][1] && x.Tag[1] == def[0][2] && x.Tag[2] == def[0][3]);
                        //countries[countryID[def[0]]].Name = def[1];
                        if (c > 0 && c < countries.Count) countries[c].Name = def[1].Trim().Replace("\"", "").Replace("0 ", "");
                    }
                }
            }
        }

        public void Add(Country[] country)
        {
            Array.ForEach(country, i => AddCountry(i));
        }
    }

    public class Country
    {
        public string Tag;
        public string Name;
        public Color Color;
        public bool Special;

        public Country(string tag)
        {
            Tag = tag;
            Name = "";
            Color = new Color();
            Special = false;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", Tag, Name);
        }
    }
}
