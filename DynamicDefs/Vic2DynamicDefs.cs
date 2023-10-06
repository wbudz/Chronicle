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
    class Vic2DynamicDefs : DynamicDefs
    {
        Dictionary<string, int> ideologiesColorscale = new Dictionary<string, int>();
        Dictionary<string, int> religionsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> nationalitiesColorscale = new Dictionary<string, int>();
        Dictionary<string, int> occupationsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> goodsColorscale = new Dictionary<string, int>();
        Dictionary<string, double> politicalReforms = new Dictionary<string, double>();
        Dictionary<string, double> socialReforms = new Dictionary<string, double>();
        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        public Vic2DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                SetTerrain(defs);
                filelist = Core.GetPaths(game, "common\\ideologies.txt", mods).ToList();
                SetIdeologies(filelist[0]);
                filelist = Core.GetPaths(game, "common\\religion.txt", mods).ToList();
                SetReligions(filelist[0]);
                filelist = Core.GetPaths(game, "common\\cultures.txt", mods).ToList();
                SetNationalities(filelist[0]);
                filelist = Core.GetPaths(game, "poptypes\\*.txt", mods).ToList();
                SetOccupations(filelist.ToArray());
                filelist = Core.GetPaths(game, "common\\goods.txt", mods).ToList();
                SetGoods(filelist[0]);
                filelist = Core.GetPaths(game, "common\\issues.txt", mods).ToList();
                SetReforms(filelist[0]);
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
                if (!terrainColorscale.ContainsKey(defs.Terrain.GetName(i)))
                    terrainColorscale.Add(defs.Terrain.GetName(i), colors[i]);
            }
        }

        private void SetIdeologies(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (var n in nodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                byte r = Ext.ParseByte(file.GetEntry(c, 0));
                byte g = Ext.ParseByte(file.GetEntry(c, 1));
                byte b = Ext.ParseByte(file.GetEntry(c, 2));
                if (!ideologiesColorscale.ContainsKey(file.GetNodeName(n)))
                    ideologiesColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }
        }

        private void SetReligions(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (var n in nodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                double r;
                double g;
                double b;
                r = Ext.ParseDouble(file.GetEntry(c, 0));
                g = Ext.ParseDouble(file.GetEntry(c, 1));
                b = Ext.ParseDouble(file.GetEntry(c, 2));
                if (!religionsColorscale.ContainsKey(file.GetNodeName(n)))
                    religionsColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255)));
            }
        }

        private void SetNationalities(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (var n in nodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                byte r = Ext.ParseByte(file.GetEntry(c, 0));
                byte g = Ext.ParseByte(file.GetEntry(c, 1));
                byte b = Ext.ParseByte(file.GetEntry(c, 2));
                if (!nationalitiesColorscale.ContainsKey(file.GetNodeName(n)))
                    nationalitiesColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }
        }

        private void SetOccupations(string[] filelist)
        {
            foreach (var path in filelist)
            {
                CEParser.File file = new CEParser.TextFile(path);
                file.Parse();

                var c = file.GetSubnode(file.Root, "color");
                byte r = Ext.ParseByte(file.GetEntry(c, 0));
                byte g = Ext.ParseByte(file.GetEntry(c, 1));
                byte b = Ext.ParseByte(file.GetEntry(c, 2));
                if (!occupationsColorscale.ContainsKey(Path.GetFileNameWithoutExtension(path)))
                    occupationsColorscale.Add(Path.GetFileNameWithoutExtension(path), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }
        }


        private void SetReforms(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, "political_reforms", "*").ToArray();

            foreach (var n in nodes)
            {
                var reforms = file.GetSubnodes(n).ToArray();
                for (int i = 0; i < reforms.Length; i++)
                {
                    if (!politicalReforms.ContainsKey(file.GetNodeName(n) + "|" + file.GetNodeName(reforms[i])))
                        politicalReforms.Add(file.GetNodeName(n) + "|" + file.GetNodeName(reforms[i]), i);
                }
            }

            nodes = file.GetSubnodes(file.Root, "social_reforms", "*").ToArray();
            foreach (var n in nodes)
            {
                var reforms = file.GetSubnodes(n).ToArray();
                for (int i = 0; i < reforms.Length; i++)
                {
                    if (!socialReforms.ContainsKey(file.GetNodeName(n) + "|" + file.GetNodeName(reforms[i])))
                        socialReforms.Add(file.GetNodeName(n) + "|" + file.GetNodeName(reforms[i]), i);
                }
            }
        }

        private void SetGoods(string path)
        {
            CEParser.File file = new CEParser.TextFile(path);
            file.Parse();

            CEParser.Node[] nodes = file.GetSubnodes(file.Root, true).FindAll(x => file.GetDepth(x) == 2).ToArray();

            foreach (var n in nodes)
            {
                if (!file.HasAContainer(n, "color")) continue;
                CEParser.Node c = file.GetSubnode(n, "color");
                byte r = Ext.ParseByte(file.GetEntry(c, 0));
                byte g = Ext.ParseByte(file.GetEntry(c, 1));
                byte b = Ext.ParseByte(file.GetEntry(c, 2));
                goodsColorscale.Add(file.GetNodeName(n), CEBitmap.Bitmap.ColorToInt32(r, g, b));
            }
        }

        public override bool HasDatakeyColors(string identifier)
        {
            switch (identifier)
            {
                case "ideologies": return true;
                case "religions": return true;
                case "nationalities": return true;
                case "tax": return true;
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
                case "ideologies":
                    return ideologiesColorscale;
                case "religions":
                    return religionsColorscale;
                case "nationalities":
                    return nationalitiesColorscale;
                case "tax":
                    output = new Dictionary<string, int>();
                    output.Add("rich", CEBitmap.Bitmap.ColorToInt32(160, 80, 160));
                    output.Add("middle", CEBitmap.Bitmap.ColorToInt32(250, 250, 20));
                    output.Add("poor", CEBitmap.Bitmap.ColorToInt32(180, 100, 0));
                    return output;
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
            //switch (identifier)
            //{
            //    case "units": units.TryGetValue(key, out output); break;
            //}
            return output ?? "";
        }

        public override double GetNumValue(string identifier, string key)
        {
            switch (identifier)
            {
                case "political_reforms": return politicalReforms[key];
                case "social_reforms": return socialReforms[key];
                default: return 0;
            }
        }

        public override Dictionary<string, double> GetNumDict(string identifier)
        {
            switch (identifier)
            {
                case "political_reforms": return politicalReforms;
                case "social_reforms": return socialReforms;
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
            switch (identifier)
            {
                case "pops": return occupationsColorscale.Keys.Contains(key);
                default: return false;
            }
        }
    }
}
