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
    class CK2DynamicDefs : DynamicDefs
    {
        Dictionary<string, string> dynastiesNames = new Dictionary<string, string>();
        Dictionary<string, string> dynastiesCultures = new Dictionary<string, string>();

        Dictionary<string, int> culturesColorscale = new Dictionary<string, int>();
        Dictionary<string, int> religionsColorscale = new Dictionary<string, int>();

        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        public CK2DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                SetTerrain(defs);
                filelist = Core.GetPaths(game,"common\\dynasties\\00_dynasties.txt", mods).ToList();
                SetDynasties(filelist[0]);
                filelist = Core.GetPaths(game, "common\\cultures\\00_cultures.txt", mods).ToList();
                SetCultures(filelist[0]);
                filelist = Core.GetPaths(game, "common\\religions\\00_religions.txt", mods).ToList();
                SetReligions(filelist[0]);
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

        private void SetCultures(string path)
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
                culturesColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
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

        private void SetDynasties(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root).ToArray();

            foreach (var n in nodes)
            {
                if (!dynastiesNames.ContainsKey(file.GetNodeName(n)))
                    dynastiesNames.Add(file.GetNodeName(n), file.GetAttributeValue(n, "name"));
                if (!dynastiesCultures.ContainsKey(file.GetNodeName(n)))
                    dynastiesCultures.Add(file.GetNodeName(n), file.GetAttributeValue(n, "culture"));
            }
        }

        public override bool HasDatakeyColors(string identifier)
        {
            switch (identifier)
            {
                case "cultures": return true;
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
                case "cultures": return culturesColorscale;
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
                case "dynasties": dynastiesNames.TryGetValue(key, out output); break;
                case "dynastiescultures": dynastiesCultures.TryGetValue(key, out output); break;
            }
            return output ?? "";
        }

        public override double GetNumValue(string identifier, string key)
        {
            switch (identifier)
            {
                default: return 0;
            }
        }

        public override Dictionary<string, double> GetNumDict(string identifier)
        {
            switch (identifier)
            {
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
