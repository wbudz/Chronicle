using System;

namespace Chronicle
{
    [Serializable]
    public class hoi3_Terrain : MultiValueTable
    {
        public hoi3_Terrain() : base("Terrain", TableType.Province)
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
    public class hoi3_ProvinceVP : SingleValueTable
    {   
        public hoi3_ProvinceVP() : base("ProvinceVP", TableType.Province)
        {
            Caption = "Province victory points";
            Category = "General";
            Section = 1;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "points"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryVP : SingleValueTable
    {
        public hoi3_CountryVP() : base("CountryVP", TableType.Country)
        {
            Caption = "Country victory points";
            Category = "General";
            Section = 1;
            AggregateValues = true;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceVP") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceFlags : MultiDatakeyTable
    {
        public hoi3_ProvinceFlags() : base("ProvinceFlags", TableType.Province)
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

    [Serializable]
    public class hoi3_CountryFlags : MultiDatakeyTable
    {
        public hoi3_CountryFlags() : base("CountryFlags", TableType.Country)
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

}
