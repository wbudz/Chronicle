using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    [Serializable]
    class EU4DynamicDefs : DynamicDefs
    {
        Dictionary<string, double> buildings = new Dictionary<string, double>();
        Dictionary<string, double> manufactories = new Dictionary<string, double>();
        Dictionary<string, double> technologiesCount = new Dictionary<string, double>();

        Dictionary<string, int> religionsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        Dictionary<string, string> provinceClimate = new Dictionary<string, string>();

        Dictionary<string, string> states = new Dictionary<string, string>();
        Dictionary<string, string> institutionsNames = new Dictionary<string, string>();

        public EU4DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                SetTerrain(defs);
                filelist = Core.GetPaths(game, "common\\buildings\\*.*", mods).ToList();
                SetBuildings(filelist);
                filelist = Core.GetPaths(game, "common\\religions\\00_religion.txt", mods).ToList();
                if (filelist.Count > 0) SetReligions(filelist[0]);
                filelist = Core.GetPaths(game, "common\\technologies\\*.*", mods).ToList();
                SetTechnologies(filelist);
                SetClimates(Core.GetPath(game, "map\\climate.txt", mods));
                SetStates(Core.GetPath(game, "map\\area.txt", mods));
                SetInstitutions(Core.GetPath(game, "common\\institutions\\00_Core.txt", mods));
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("There was an error creating extensions data. Some of data may not appear properly.", ex);
            }
        }

        void SetTerrain(Gamedefs defs)
        {
            var colors = defs.Terrain.GetColors();
            for (int i = 0; i < colors.Length; i++)
            {
                terrainColorscale.Add(defs.Terrain.GetName(i), colors[i]);
            }
        }

        private void SetTechnologies(IEnumerable<string> path)
        {
            foreach (var f in path)
            {
                CEParser.File file = new CEParser.TextFile(f);
                file.Parse();

                var nodes = file.GetSubnodes(file.Root, x => file.GetNodeName(x) == "technology").ToArray();

                string type = file.GetAttributeValue(file.Root, "monarch_power");

                switch (type)
                {
                    case "ADM": technologiesCount.Add("ADM", nodes.Length); break;
                    case "DIP": technologiesCount.Add("DIP", nodes.Length); break;
                    case "MIL": technologiesCount.Add("MIL", nodes.Length); break;
                    default: break;
                }
            }
        }

        private void SetBuildings(IEnumerable<string> path)
        {
            foreach (var f in path)
            {
                CEParser.File file = new CEParser.TextFile(f);
                file.Parse();

                var nodes = file.GetSubnodes(file.Root);

                foreach (CEParser.Node n in nodes)
                {
                    int level = 1;
                    bool manufactory = file.HasAnAttributeWithName(n, "manufactory");
                    if (file.HasAnAttributeWithName(n, "make_obsolete"))
                    {
                        level = (int)(buildings[file.GetAttributeValue(n, "make_obsolete")] + 1);
                    }

                    if (manufactory)
                    {
                        if (manufactories.ContainsKey(file.GetNodeName(n))) continue;
                        manufactories.Add(file.GetNodeName(n), level);
                    }
                    else
                    {
                        if (buildings.ContainsKey(file.GetNodeName(n))) continue;
                        buildings.Add(file.GetNodeName(n), level);
                    }
                }
            }
        }

        private void SetReligions(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (CEParser.Node n in nodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                double r = Ext.ParseDouble(file.GetEntry(c, 0));
                double g = Ext.ParseDouble(file.GetEntry(c, 1));
                double b = Ext.ParseDouble(file.GetEntry(c, 2));
                religionsColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
            }

        }

        private void SetClimates(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root).ToArray();

            foreach (var climateNode in nodes)
            {
                foreach (string province in file.GetEntries(climateNode))
                {
                    if (provinceClimate.ContainsKey(province)) continue;
                    provinceClimate.Add(province, file.GetNodeName(climateNode));
                }
            }
        }

        private void SetStates(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root).ToArray();

            foreach (var n in nodes)
            {
                foreach (string province in file.GetEntries(n))
                {
                    if (states.ContainsKey(province)) continue;
                    states.Add(province, file.GetNodeName(n));
                }
            }
        }

        private void SetInstitutions(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root).ToArray();

            foreach (var n in nodes)
            {
                institutionsNames.Add((institutionsNames.Count + 1).ToString(), file.GetNodeName(n));
            }
        }

        public override bool HasDatakeyColors(string identifier)
        {
            switch (identifier)
            {
                case "religions": return true;
                case "countries": return true;
                case "terrain": return true;
                default: return false;
            }
        }

        public override Dictionary<string, int> GetDatakeysColors(string identifier)
        {
            Dictionary<string, int> output;
            switch (identifier)
            {
                case "religions": return religionsColorscale;
                case "countries":
                    output = new Dictionary<string, int>();
                    for (ushort i = 0; i < Core.Data.Defs.Countries.Count; i++)
                    {
                        output.Add(Core.Data.Defs.Countries.GetTag(i), CEBitmap.Bitmap.ColorToInt32(Core.Data.Defs.Countries.GetColor(i)));
                    }
                    return output;
                case "terrain": return terrainColorscale;
                default: return new Dictionary<string, int>();
            }
        }

        public override string GetTextValue(string identifier, string key)
        {
            string output = "";
            switch (identifier)
            {
                case "climates": provinceClimate.TryGetValue(key, out output); return output;
                case "states": states.TryGetValue(key, out output); return output;
                case "institutions": institutionsNames.TryGetValue(key, out output); return output;
            }
            return output ?? "";
        }

        public override double GetNumValue(string identifier, string key)
        {
            double output;
            switch (identifier)
            {
                case "technologiescount": technologiesCount.TryGetValue(key, out output); return output;
                default: return 0;
            }
        }

        public override Dictionary<string, double> GetNumDict(string identifier)
        {
            switch (identifier)
            {
                case "buildings": return buildings;
                case "manufactories": return manufactories;
                case "technologiescount": return technologiesCount;
                default: return new Dictionary<string, double>();
            }
        }

        public override Dictionary<string, string> GetTextDict(string identifier)
        {
            switch (identifier)
            {
                default: return new Dictionary<string, string>();
            }
        }

        public override bool IsOnList(string identifier, string key)
        {
            return false;
        }
    }
}
