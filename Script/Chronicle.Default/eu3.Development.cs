using System;

namespace Chronicle
{
    [Serializable]
    public class eu3_LandTechnology : SingleValueTable
    {
        public eu3_LandTechnology() : base("LandTechnology", TableType.Country)
        {
            Caption = "Land technology";
            Category = "Development";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var tech = f.GetSubnode(n.Value, "technology", "land_tech");
                Set(n.Key, f.GetEntry(tech, 0));
            }
        }
    }

    [Serializable]
    public class eu3_NavalTechnology : SingleValueTable
    {
        public eu3_NavalTechnology() : base("NavalTechnology", TableType.Country)
        {
            Caption = "Naval technology";
            Category = "Development";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var tech = f.GetSubnode(n.Value, "technology", "naval_tech");
                Set(n.Key, f.GetEntry(tech, 0));
            }
        }
    }

    [Serializable]
    public class eu3_TradeTechnology : SingleValueTable
    {
        public eu3_TradeTechnology() : base("TradeTechnology", TableType.Country)
        {
            Caption = "Trade technology";
            Category = "Development";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var tech = f.GetSubnode(n.Value, "technology", "trade_tech");
                Set(n.Key, f.GetEntry(tech, 0));
            }
        }
    }

    [Serializable]
    public class eu3_ProductionTechnology : SingleValueTable
    {
        public eu3_ProductionTechnology() : base("ProductionTechnology", TableType.Country)
        {
            Caption = "Production technology";
            Category = "Development";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var tech = f.GetSubnode(n.Value, "technology", "production_tech");
                Set(n.Key, f.GetEntry(tech, 0));
            }
        }
    }

    [Serializable]
    public class eu3_GovernmentTechnology : SingleValueTable
    {
        public eu3_GovernmentTechnology() : base("GovernmentTechnology", TableType.Country)
        {
            Caption = "Government technology";
            Category = "Development";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var tech = f.GetSubnode(n.Value, "technology", "government_tech");
                Set(n.Key, f.GetEntry(tech, 0));
            }
        }
    }

    [Serializable]
    public class eu3_CitySize : SingleValueTable
    {
        public eu3_CitySize() : base("CitySize", TableType.Province)
        {
            Caption = "City size";
            Category = "Development";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), -2);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "citysize"));
            }
        }
    }

    [Serializable]
    public class eu3_CulturalTradition : SingleValueTable
    {
        public eu3_CulturalTradition() : base("CulturalTradition", TableType.Country)
        {
            Caption = "Cultural tradition";
            Category = "Development";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "cultural_tradition"));
            }
        }
    }

}
