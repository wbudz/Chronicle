using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class DefaultTable : SingleDatakeyTable
    {
        public DefaultTable() : base("Default", TableType.Province)
        {
            Caption = "Default";
            Category = "General";
            RequiresSavegame = false;
            ForceCache = true;
            ParsingOrder = -3;

            ScriptName = "Default";
            ScriptSet = "(built-in)";

            SetColorscale();
            AddColor("land", CreateColor(CEBitmap.Bitmap.Int32ToColor(Core.Settings.LandColor)));
            AddColor("water", CreateColor(CEBitmap.Bitmap.Int32ToColor(Core.Settings.WaterColor)));
        }

        public override void Parse(CEParser.File f)
        {
            for (ushort i = 0; i < Core.Data.Defs.Provinces.Count; i++)
            {
                Set(i, Core.Data.Defs.Provinces.IsWater(i) ? "water" : "land");
            }
        }
    }

    [Serializable]
    public class MasterTable : SingleValueTable
    {
        public MasterTable() : base("Master", TableType.Province)
        {
            Caption = "Master";
            Category = "General";
            Hidden = true;
            RequiresSavegame = true;
            ForceCache = true;
            ParsingOrder = -3;

            ScriptName = "Master";
            ScriptSet = "(built-in)";

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            if (Core.Data.Game.Token == "ck2")
            {
                foreach (var n in f["baronies"])
                {
                    // Find barony title
                    CEParser.Node ownertitle = f.GetSubnode(f.Root, "title", f.GetNodeName(n.Value));
                    // Traverse through liege trees
                    if (ownertitle == null) continue;
                    while (f.HasAContainer(ownertitle, "liege"))
                    {
                        string t = f.GetAttributeValue(f.GetSubnode(ownertitle, "liege"), "title");
                        ownertitle = f.GetSubnode(f.Root, "title", t);
                    }

                    Set(n.Key, Core.Data.TagToIndex(f.GetNodeName(ownertitle)));
                }
            }
            else
            {
                foreach (var n in f["provinces"])
                {
                    Set(n.Key, Core.Data.TagToIndex(f.GetAttributeValue(n.Value, "owner")));
                }
            }
        }
    }
}
