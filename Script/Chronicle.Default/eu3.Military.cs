using System;

namespace Chronicle
{
    [Serializable]
    public class eu3_CityGarrison : SingleValueTable
    {
        public eu3_CityGarrison() : base("CityGarrison", TableType.Province)
        {
            Caption = "City garrison";
            Category = "Military";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "garrison"));
            }
        }
    }

    [Serializable]
    public class eu3_CityManpower : SingleValueTable
    {
        public eu3_CityManpower() : base("ProvinceManpower", TableType.Province)
        {
            Caption = "Province manpower";
            Category = "Military";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "manpower"));
            }
        }
    }

    [Serializable]
    public class eu3_CountryManpower : SingleValueTable
    {
        public eu3_CountryManpower() : base("CountryManpower", TableType.Country)
        {
            Caption = "Country manpower";
            Category = "Military";
            Section = 0;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceManpower") as SingleValueTable;
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
    public class eu3_LandUnitsPerProvince : SingleValueTable
    {
        public eu3_LandUnitsPerProvince() : base("LandUnitsPerProvince", TableType.Province)
        {
            Caption = "Land units per province";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "army"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "regiment"))
                    {
                        Set(location, f.GetAttributeValue(regiment, "strength"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_LandUnitsPerCountry : SingleValueTable
    {
        public eu3_LandUnitsPerCountry() : base("LandUnitsPerCountry", TableType.Country)
        {
            Caption = "Land units per province";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "army"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "regiment"))
                    {
                        Set(n.Key, f.GetAttributeValue(regiment, "strength"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_LandUnitsComposition : MultiValueTable
    {
        public eu3_LandUnitsComposition() : base("LandUnitsComposition", TableType.Country)
        {
            Caption = "Land units composition";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale();
            AddColor("infantry", CreateColor(120, 60, 30));
            AddColor("cavalry", CreateColor(80, 180, 70));
            AddColor("artillery", CreateColor(180, 40, 220));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "army"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "regiment"))
                    {
                        Set(n.Key, DynamicDefs.GetTextValue("units", f.GetAttributeValue(regiment, "type")), f.GetAttributeValue(regiment, "strength"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_AverageLandMorale : SingleValueTable
    {
        public eu3_AverageLandMorale() : base("AverageLandMorale", TableType.Country)
        {
            Caption = "Land units average morale";
            Category = "Military";
            Section = 1;
            ParsingOrder = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("LandUnitsPerCountry") as SingleValueTable;
            if (t == null) return;
            double[] data = t.GetVector();

            double[] morale = new double[data.Length];

            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "army"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "regiment"))
                    {
                        morale[n.Key] += Ext.ParseDouble(f.GetAttributeValue(regiment, "strength")) * Ext.ParseDouble(f.GetAttributeValue(regiment, "morale"));
                    }
                }
            }

            for (ushort i = 0; i < data.Length; i++)
            {
                Set(i, data[i] == 0 ? 0 : (morale[i] / data[i]));
            }
        }
    }

    [Serializable]
    public class eu3_SeaUnitsPerProvince : SingleValueTable
    {
        public eu3_SeaUnitsPerProvince() : base("SeaUnitsPerProvince", TableType.Province)
        {
            Caption = "Sea units per province";
            Category = "Military";
            Section = 2;
            AggregateValues = true;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "navy"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "ship"))
                    {
                        Set(location, f.GetAttributeValue(regiment, "strength"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_SeaUnitsPerCountry : SingleValueTable
    {
        public eu3_SeaUnitsPerCountry() : base("SeaUnitsPerCountry", TableType.Country)
        {
            Caption = "Sea units per province";
            Category = "Military";
            Section = 2;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "navy"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "ship"))
                    {
                        Set(n.Key, f.GetAttributeValue(regiment, "strength"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class eu3_AverageSeaMorale : SingleValueTable
    {
        public eu3_AverageSeaMorale() : base("AverageSeaMorale", TableType.Country)
        {
            Caption = "Sea units average morale";
            Category = "Military";
            Section = 2;
            ParsingOrder = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("SeaUnitsPerCountry") as SingleValueTable;
            if (t == null) return;
            double[] data = t.GetVector();

            double[] morale = new double[data.Length];

            foreach (var n in f["countries"])
            {
                foreach (var army in f.GetSubnodes(n.Value, "navy"))
                {
                    var location = f.GetAttributeValue(army, "location");
                    foreach (var regiment in f.GetSubnodes(army, "ship"))
                    {
                        morale[n.Key] += Ext.ParseDouble(f.GetAttributeValue(regiment, "strength")) * Ext.ParseDouble(f.GetAttributeValue(regiment, "morale"));
                    }
                }
            }

            for (ushort i = 0; i < data.Length; i++)
            {
                Set(i, data[i] == 0 ? 0 : (morale[i] / data[i]));
            }
        }
    }

    [Serializable]
    public class eu3_WarExhaustion : SingleValueTable
    {
        public eu3_WarExhaustion() : base("WarExhaustion", TableType.Country)
        {
            Caption = "War exhaustion";
            Category = "Military";
            Section = 3;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "war_exhaustion"));
            }
        }
    }

    [Serializable]
    public class eu3_ManpowerPool : SingleValueTable
    {
        public eu3_ManpowerPool() : base("ManpowerPool", TableType.Country)
        {
            Caption = "Country manpower pool";
            Category = "Military";
            Section = 3;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "manpower"));
            }
        }
    }

    [Serializable]
    public class eu3_LandMaintenance : SingleValueTable
    {
        public eu3_LandMaintenance() : base("LandMaintenance", TableType.Country)
        {
            Caption = "Land maintenance";
            Category = "Military";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "land_maintenance"));
            }
        }
    }

    [Serializable]
    public class eu3_NavalMaintenance : SingleValueTable
    {
        public eu3_NavalMaintenance() : base("NavalMaintenance", TableType.Country)
        {
            Caption = "Naval maintenance";
            Category = "Military";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "naval_maintenance"));
            }
        }
    }

    [Serializable]
    public class eu3_ArmyTradition : SingleValueTable
    {
        public eu3_ArmyTradition() : base("ArmyTradition", TableType.Country)
        {
            Caption = "Army tradition";
            Category = "Military";
            Section = 5;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "army_tradition"));
            }
        }
    }

    [Serializable]
    public class eu3_NavalTradition : SingleValueTable
    {
        public eu3_NavalTradition() : base("NavalTradition", TableType.Country)
        {
            Caption = "Naval tradition";
            Category = "Military";
            Section = 5;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "naval_tradition"));
            }
        }
    }

}
