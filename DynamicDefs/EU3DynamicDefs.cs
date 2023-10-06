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
    class EU3DynamicDefs : DynamicDefs
    {
        Dictionary<string, int> religionsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        Dictionary<string, string> units = new Dictionary<string, string>();

        Dictionary<ushort, List<string>> provinceRegions = new Dictionary<ushort, List<string>>();
        List<string> regions = new List<string>();
        Dictionary<ushort, List<string>> provinceContinents = new Dictionary<ushort, List<string>>();
        List<string> continents = new List<string>();
        Dictionary<ushort, List<string>> provinceClimates = new Dictionary<ushort, List<string>>();
        List<string> climates = new List<string>();

        public EU3DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                SetTerrain(defs);
                filelist = Core.GetPaths(game, "common\\religion.txt", mods).ToList();
                SetReligions(filelist[0]);
                filelist = Core.GetPaths(game, "common\\units\\*.*", mods).ToList();
                SetUnits(filelist);
                filelist = Core.GetPaths(game, "map\\climate.txt", mods).ToList();
                SetClimates(filelist[0]);
                filelist = Core.GetPaths(game, "map\\region.txt", mods).ToList();
                SetRegions(filelist[0]);
                filelist = Core.GetPaths(game, "map\\continent.txt", mods).ToList();
                SetContinents(filelist[0]);
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("There was an error creating extensions data. Some of data may not appear properly.", ex);
            }
        }

        #region Setters

        void SetTerrain(Gamedefs defs)
        {
            var colors = defs.Terrain.GetColors();
            for (int i = 0; i < colors.Length; i++)
            {
                terrainColorscale.Add(defs.Terrain.GetName(i), colors[i]);
            }
        }

        void SetReligions(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] religionNodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (CEParser.Node n in religionNodes)
            {
                CEParser.Node c = file.GetSubnode(n, "color");
                double r = Ext.ParseDouble(file.GetEntry(c, 0));
                double g = Ext.ParseDouble(file.GetEntry(c, 1));
                double b = Ext.ParseDouble(file.GetEntry(c, 2));
                religionsColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32(Color.FromRgb((byte)(r * 255 % 255), (byte)(g * 255 % 255), (byte)(b * 255 % 255))));
            }
        }

        void SetUnits(List<string> filelist)
        {
            foreach (var path in filelist)
            {
                CEParser.File file = new CEParser.TextFile(path);
                file.Parse();

                units.Add(Path.GetFileNameWithoutExtension(path), file.GetAttributeValue(file.Root, "type"));
            }
        }

        void SetRegions(string path)
        {
            if (!File.Exists(path))
            {
                Core.Log.ReportError("Could not find regions definition file <" + path + ">.");
                return;
            }

            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            foreach (CEParser.Node n in file.GetSubnodes(file.Root))
            {
                string region = file.GetNodeName(n);
                string[] provs = file.GetEntries(n);
                for (int i = 0; i < provs.Length; i++)
                {
                    ushort id = UInt16.Parse(provs[i]);
                    if (!provinceRegions.ContainsKey(id))
                        provinceRegions.Add(id, new List<string>());
                    provinceRegions[id].Add(region);
                    if (!regions.Contains(region)) regions.Add(region);
                }
            }
        }

        void SetContinents(string path)
        {
            if (!File.Exists(path))
            {
                Core.Log.ReportError("Could not find continents definition file <" + path + ">.");
                return;
            }

            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            foreach (CEParser.Node n in file.GetSubnodes(file.Root))
            {
                string continent = file.GetNodeName(n);
                string[] provs = file.GetEntries(n);
                for (int i = 0; i < provs.Length; i++)
                {
                    ushort id = UInt16.Parse(provs[i]);
                    if (!provinceContinents.ContainsKey(id))
                        provinceContinents.Add(id, new List<string>());
                    provinceContinents[id].Add(continent);
                    if (!continents.Contains(continent)) continents.Add(continent);
                }
            }
        }

        void SetClimates(string path)
        {
            if (!File.Exists(path))
            {
                Core.Log.ReportError("Could not find climates definition file <" + path + ">.");
                return;
            }

            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            foreach (CEParser.Node n in file.GetSubnodes(file.Root))
            {
                string climate = file.GetNodeName(n);
                string[] provs = file.GetEntries(n);
                for (int i = 0; i < provs.Length; i++)
                {
                    ushort id = UInt16.Parse(provs[i]);
                    if (!provinceClimates.ContainsKey(id))
                        provinceClimates.Add(id, new List<string>());
                    provinceClimates[id].Add(climate);
                    if (!climates.Contains(climate)) climates.Add(climate);
                }
            }
        }

        #endregion

        public override bool HasDatakeyColors(string identifier)
        {
            switch (identifier)
            {
                case "countries": return true;
                case "religions": return true;
                case "terrain": return true;
                default: return false;
            }
        }

        public override Dictionary<string, int> GetDatakeysColors(string identifier)
        {
            Dictionary<string, int> output;
            switch (identifier)
            {
                case "countries":
                    output = new Dictionary<string, int>();
                    for (ushort i = 0; i < Core.Data.Defs.Countries.Count; i++)
                    {
                        output.Add(Core.Data.Defs.Countries.GetTag(i), CEBitmap.Bitmap.ColorToInt32(Core.Data.Defs.Countries.GetColor(i)));
                    }
                    return output;
                case "religions": return religionsColorscale;
                case "terrain": return terrainColorscale;
                default: return new Dictionary<string, int>();
            }
        }

        public override string GetTextValue(string identifier, string key)
        {
            string output = "";
            switch (identifier)
            {
                case "units": units.TryGetValue(key, out output); break;
            }
            return output ?? "";
        }

        public override double GetNumValue(string identifier, string key)
        {
            return 0;
        }

        public override Dictionary<string, double> GetNumDict(string identifier)
        {
            return new Dictionary<string, double>();
        }

        public override Dictionary<string, string> GetTextDict(string identifier)
        {
            return new Dictionary<string, string>();
        }

        public override bool IsOnList(string identifier, string key)
        {
            return false;
        }

        public string GetUnitType(string type)
        {
            string output;
            units.TryGetValue(type, out output);
            return output ?? "";
        }

        public string[] GetRegions(ushort id)
        {
            List<string> output;
            provinceRegions.TryGetValue(id, out output);
            if (output == null)
                return new string[0];
            else
                return output.ToArray();
        }

        public int GetRegionsCount()
        {
            return regions.Count;
        }

        public int GetRegionIndex(string region)
        {
            return regions.FindIndex(x => x == region);
        }

        public string[] GetContinents(ushort id)
        {
            List<string> output;
            provinceContinents.TryGetValue(id, out output);
            if (output == null)
                return new string[0];
            else
                return output.ToArray();
        }

        public int GetContinentsCount()
        {
            return continents.Count;
        }

        public int GetContinentIndex(string continent)
        {
            return continents.FindIndex(x => x == continent);
        }

        public string[] GetClimates(ushort id)
        {
            List<string> output;
            provinceClimates.TryGetValue(id, out output);
            if (output == null)
                return new string[0];
            else
                return output.ToArray();
        }

        public int GetClimatesCount()
        {
            return climates.Count;
        }

        public int GetClimateIndex(string climate)
        {
            return climates.FindIndex(x => x == climate);
        }
    }
}
