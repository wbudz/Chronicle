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
    class HoI4DynamicDefs : DynamicDefs
    {
        Dictionary<string, double> buildings = new Dictionary<string, double>();
        Dictionary<string, double> manufactories = new Dictionary<string, double>();
        Dictionary<string, double> technologiesCount = new Dictionary<string, double>();

        Dictionary<string, int> religionsColorscale = new Dictionary<string, int>();
        Dictionary<string, int> terrainColorscale = new Dictionary<string, int>();

        Dictionary<string, string> provinceClimate = new Dictionary<string, string>();

        Dictionary<string, string> states = new Dictionary<string, string>();
        Dictionary<string, string> institutionsNames = new Dictionary<string, string>();

        public HoI4DynamicDefs(Gamedefs defs, InstalledGame game, string path, IEnumerable<Mod> mods)
        {
            List<string> filelist;
            try
            {
                //SetTerrain(defs);
                //filelist = Core.GetPaths(game, "common\\buildings\\*.*", mods).ToList();
                //SetBuildings(filelist);
                //filelist = Core.GetPaths(game, "common\\religions\\00_religion.txt", mods).ToList();
                //if (filelist.Count > 0) SetReligions(filelist[0]);
                //filelist = Core.GetPaths(game, "common\\technologies\\*.*", mods).ToList();
                //SetTechnologies(filelist);
                //SetClimates(Core.GetPath(game, "map\\climate.txt", mods));
                //SetStates(Core.GetPath(game, "map\\area.txt", mods));
                //SetInstitutions(Core.GetPath(game, "common\\institutions\\00_Core.txt", mods));
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
                //case "religions": return true;
                //case "countries": return true;
                //case "terrain": return true;
                default: return false;
            }
        }

        public override Dictionary<string, int> GetDatakeysColors(string identifier)
        {
            Dictionary<string, int> output;
            switch (identifier)
            {
                //case "religions": return religionsColorscale;
                //case "countries":
                //    output = new Dictionary<string, int>();
                //    for (ushort i = 0; i < Core.Data.Defs.Countries.Count; i++)
                //    {
                //        output.Add(Core.Data.Defs.Countries.GetTag(i), CEBitmap.Bitmap.ColorToInt32(Core.Data.Defs.Countries.GetColor(i)));
                //    }
                //    return output;
                //case "terrain": return terrainColorscale;
                default: return new Dictionary<string, int>();
            }
        }

        public override string GetTextValue(string identifier, string key)
        {
            string output = "";
            switch (identifier)
            {
                //case "climates": provinceClimate.TryGetValue(key, out output); return output;
                //case "states": states.TryGetValue(key, out output); return output;
                //case "institutions": institutionsNames.TryGetValue(key, out output); return output;
                default: return "";
            }
            return output ?? "";
        }

        public override double GetNumValue(string identifier, string key)
        {
            double output;
            switch (identifier)
            {
                //case "technologiescount": technologiesCount.TryGetValue(key, out output); return output;
                default: return 0;
            }
        }

        public override Dictionary<string, double> GetNumDict(string identifier)
        {
            switch (identifier)
            {
                //case "buildings": return buildings;
                //case "manufactories": return manufactories;
                //case "technologiescount": return technologiesCount;
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
