using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_Terrain : MultiValueTable
    {
        public eu4_Terrain() : base("Terrain", TableType.Province)
        {
            Caption = "Terrain";
            Category = "General";
            Section = 1;
            RequiresSavegame = false;

            SetColorscale("terrain");
        }

        public override void Parse(CEParser.File f)
        {
            for (int i = 0; i < Core.Data.Defs.Provinces.Count; i++)
            {
                var terrains = Core.Data.Defs.Provinces.GetTerrainTypes(i);
                for (int j = 0; j < terrains.Length; j++)
                {
                    Set((ushort)i, Core.Data.Defs.Terrain.GetName(j), terrains[j]);
                }
            }
        }
    }

    [Serializable]
    public class eu4_Climate : SingleDatakeyTable
    {
        public eu4_Climate() : base("Climate", TableType.Province)
        {
            Caption = "Climate";
            Category = "General";
            Section = 1;
            RequiresSavegame = false;

            SetColorscale();
            AddColor("tropical", CreateColor(0, 127, 70));
            AddColor("arid", CreateColor(255, 216, 0));
            AddColor("arctic", CreateColor(170, 255, 255));
            AddColor("mild_winter", CreateColor(160, 200, 160));
            AddColor("normal_winter", CreateColor(210, 255, 210));
            AddColor("severe_winter", CreateColor(255, 255, 255));
            AddColor("impassable", CreateColor(160, 0, 0));
        }

        public override void Parse(CEParser.File f)
        {
            for (ushort i = 0; i < Core.Data.Defs.Provinces.Count; i++)
            {
                Set(i, DynamicDefs.GetTextValue("climates", i.ToString()));
            }
        }
    }

    [Serializable]
    public class eu4_ProvinceFlags : MultiDatakeyTable
    {
        public eu4_ProvinceFlags() : base("ProvinceFlags", TableType.Province)
        {
            Caption = "Province flags";
            Category = "General";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var flags = f.GetSubnodes(n.Value, "flags", "*");
                foreach (var flag in flags)
                {
                    Set(n.Key, f.GetNodeName(flag));
                }
                flags = f.GetSubnodes(n.Value, "hidden_flags", "*");
                foreach (var flag in flags)
                {
                    Set(n.Key, f.GetNodeName(flag));
                }
            }
        }
    }

    [Serializable]
    public class eu4_CountryFlags : MultiDatakeyTable
    {
        public eu4_CountryFlags() : base("CountryFlags", TableType.Country)
        {
            Caption = "Country flags";
            Category = "General";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var flags = f.GetSubnodes(n.Value, "flags", "*");
                foreach (var flag in flags)
                {
                    Set(n.Key, f.GetNodeName(flag));
                }
                flags = f.GetSubnodes(n.Value, "hidden_flags", "*");
                foreach (var flag in flags)
                {
                    Set(n.Key, f.GetNodeName(flag));
                }
            }
        }
    }

    [Serializable]
    public class eu4_LuckyNations : SingleDatakeyTable
    {
        public eu4_LuckyNations() : base("LuckyNations", TableType.Country)
        {
            Caption = "Lucky nations";
            Category = "General";
            Section = 1;

            SetColorscale();
            AddColor("lucky", CreateColor(100, 200, 100));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                if (f.HasAnAttribute(n.Value, "luck", "yes"))
                {
                    Set(n.Key, "lucky");
                }
            }
        }
    }

    [Serializable]
    public class eu4_AIPersonality : SingleDatakeyTable
    {
        public eu4_AIPersonality() : base("AIPersonality", TableType.Country)
        {
            Caption = "AI personality";
            Category = "General";
            Section = 1;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "ai"), "personality"));
            }
        }
    }

    [Serializable]
    public class eu4_AIAttitude : MultiDatakeyTable
    {
        public eu4_AIAttitude() : base("AIAttitude", TableType.Country)
        {
            Caption = "AI attitude";
            Category = "General";
            Section = 1;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var relations = f.GetSubnodes(n.Value, "active_relations", "*");
                foreach (var r in relations)
                {
                    Set(n.Key, f.GetAttributeValue(r, "attitude"), TagToIndex(f.GetNodeName(r)));
                }
            }
        }
    }

}
