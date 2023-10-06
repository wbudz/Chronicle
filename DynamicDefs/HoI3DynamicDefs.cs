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
    class HoI3DynamicDefs : DynamicDefs
    {
        Dictionary<string, int> ideologiesColorscale = new Dictionary<string, int>();
        Dictionary<string, int> factionsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        public HoI3DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                SetTerrain(defs);
                filelist = Core.GetPaths(game, "common\\ideologies.txt", mods).ToList();
                SetIdeologiesAndFactions(filelist[0]);
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("There was an error creating extensions data. Some of data may not appear properly.", ex);
            }
        }

        public override bool HasDatakeyColors(string identifier)
        {
            switch (identifier)
            {
                case "countries": return true;
                case "ideologies": return true;
                case "factions": return true;
                case "terrain": return true;
                default: return false;
            }
        }

        public override Dictionary<string, int> GetDatakeysColors(string identifier)
        {
            Dictionary<string, int> output;
            switch (identifier)
            {
                case "terrain": return terrainColorscale;
                case "countries":
                    output = new Dictionary<string, int>();
                    for (ushort i = 0; i < Core.Data.Defs.Countries.Count; i++)
                    {
                        output.Add(Core.Data.Defs.Countries.GetTag(i), CEBitmap.Bitmap.ColorToInt32(Core.Data.Defs.Countries.GetColor(i)));
                    }
                    return output;
                case "ideologies": return ideologiesColorscale;
                case "factions": return factionsColorscale;
                default: return new Dictionary<string, int>();
            }
        }

        public override string GetTextValue(string identifier, string key)
        {
            string output = "";
            //switch (identifier)
            //{
            //    case "units": units.TryGetValue(key, out output); break;
            //}
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

        #region Setters
        
        void SetTerrain(Gamedefs defs)
        {
            var colors = defs.Terrain.GetColors();
            for (int i = 0; i < colors.Length; i++)
            {
                terrainColorscale.Add(defs.Terrain.GetName(i), colors[i]);
            }
        }

        void SetIdeologiesAndFactions(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] ideologiesNodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (CEParser.Node n in ideologiesNodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                byte r;
                byte g;
                byte b;
                Byte.TryParse(file.GetEntry(c, 0), out r);
                Byte.TryParse(file.GetEntry(c, 1), out g);
                Byte.TryParse(file.GetEntry(c, 2), out b);
                ideologiesColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }

            CEParser.Node[] factionsNodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 1).ToArray();

            foreach (CEParser.Node n in factionsNodes)
            {
                if (!file.HasAnAttributeWithName(file.GetSubnode(n, "faction"), "tag")) continue;
                CEParser.Node c = file.GetSubnode(file.GetSubnode(n, 0), "color");
                byte r;
                byte g;
                byte b;
                Byte.TryParse(file.GetEntry(c, 0), out r);
                Byte.TryParse(file.GetEntry(c, 1), out g);
                Byte.TryParse(file.GetEntry(c, 2), out b);
                factionsColorscale.Add(file.GetAttributeValue(file.GetSubnode(n, "faction"), "tag"), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }
        }

        #endregion
    }
}
