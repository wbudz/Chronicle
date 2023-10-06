using System;

namespace Chronicle
{
    [Serializable]
    public class eu3_Terrain : MultiValueTable
    {
        public eu3_Terrain() : base("Terrain", TableType.Province)
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
                int ot = Core.Data.Defs.Provinces.GetOverriddenTerrain(i);
                if (ot >= 0)
                {
                    Set((ushort)i, Core.Data.Defs.Terrain.GetName(i), 1);
                }
                else
                {
                    var terrains = Core.Data.Defs.Provinces.GetTerrainTypes(i);
                    for (int j = 0; j < terrains.Length; j++)
                    {
                        Set((ushort)i, Core.Data.Defs.Terrain.GetName(j), terrains[j]);
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_CountryFlags : MultiDatakeyTable
    {
        public eu3_CountryFlags() : base("CountryFlags", TableType.Country)
        {
            Caption = "Country flags";
            Category = "General";
            Section = 2;

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
            }
        }
    }

    [Serializable]
    public class eu3_ProvinceFlags : MultiDatakeyTable
    {
        public eu3_ProvinceFlags() : base("ProvinceFlags", TableType.Province)
        {
            Caption = "Province flags";
            Category = "General";
            Section = 2;

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
            }
        }
    }

}
