using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_ProvinceManpower : SingleValueTable
    {
        public eu4_ProvinceManpower() : base("ProvinceManpower", TableType.Province)
        {
            Caption = "Province manpower";
            Category = "Military";
            Section = 0;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "base_manpower"));
            }
        }
    }

    [Serializable]
    public class eu4_CountryManpower : SingleValueTable
    {
        public eu4_CountryManpower() : base("CountryManpower", TableType.Country)
        {
            Caption = "Country base manpower";
            Category = "Military";
            Section = 0;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceManpower");
        }
    }

    [Serializable]
    public class eu4_MilitaryStrength : SingleValueTable
    {
        public eu4_MilitaryStrength() : base("MilitaryStrength", TableType.Country)
        {
            Caption = "Military strength";
            Category = "Military";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "military_strength"));
            }
        }
    }

    [Serializable]
    public class eu4_ArmyStrength : SingleValueTable
    {
        public eu4_ArmyStrength() : base("ArmyStrength", TableType.Country)
        {
            Caption = "Army strength";
            Category = "Military";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "army_strength"));
            }
        }
    }

    [Serializable]
    public class eu4_NavyStrength : SingleValueTable
    {
        public eu4_NavyStrength() : base("NavyStrength", TableType.Country)
        {
            Caption = "Navy strength";
            Category = "Military";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "navy_strength"));
            }
        }
    }

    public class eu4_LandMorale : SingleValueTable
    {
        public eu4_LandMorale() : base("LandMorale", TableType.Country)
        {
            Caption = "Land morale";
            Category = "Military";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "land_morale"));
            }
        }
    }

    public class eu4_NavyMorale : SingleValueTable
    {
        public eu4_NavyMorale() : base("NavyMorale", TableType.Country)
        {
            Caption = "Navy morale";
            Category = "Military";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "navy_morale"));
            }
        }
    }

    public class eu4_LandMaintenance : SingleValueTable
    {
        public eu4_LandMaintenance() : base("LandMaintenance", TableType.Country)
        {
            Caption = "Land maintenance";
            Category = "Military";
            Section = 3;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "land_maintenance"));
            }
        }
    }

    public class eu4_NavyMaintenance : SingleValueTable
    {
        public eu4_NavyMaintenance() : base("NavyMaintenance", TableType.Country)
        {
            Caption = "Navy maintenance";
            Category = "Military";
            Section = 3;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "navy_maintenance"));
            }
        }
    }

    public class eu4_LandTradition : SingleValueTable
    {
        public eu4_LandTradition() : base("LandTradition", TableType.Country)
        {
            Caption = "Land tradition";
            Category = "Military";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "land_tradition"));
            }
        }
    }

    public class eu4_NavyTradition : SingleValueTable
    {
        public eu4_NavyTradition() : base("Navytradition", TableType.Country)
        {
            Caption = "Navy tradition";
            Category = "Military";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "navy_tradition"));
            }
        }
    }

    public class eu4_CountryManpowerpool : SingleValueTable
    {
        public eu4_CountryManpowerpool() : base("CountryManpowerpool", TableType.Country)
        {
            Caption = "Country manpower pool";
            Category = "Military";
            Section = 5;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "manpower"));
            }
        }
    }

    public class eu4_CountryMaxManpowerpool : SingleValueTable
    {
        public eu4_CountryMaxManpowerpool() : base("CountryMaxManpowerpool", TableType.Country)
        {
            Caption = "Country maximum manpower pool";
            Category = "Military";
            Section = 5;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "max_manpower"));
            }
        }
    }

    public class eu4_CountrySailorpool : SingleValueTable
    {
        public eu4_CountrySailorpool() : base("CountrySailorpool", TableType.Country)
        {
            Caption = "Country sailors pool";
            Category = "Military";
            Section = 5;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "sailors"));
            }
        }
    }

    public class eu4_CountryMaxSailorpool : SingleValueTable
    {
        public eu4_CountryMaxSailorpool() : base("CountryMaxSailorpool", TableType.Country)
        {
            Caption = "Country maximum sailors pool";
            Category = "Military";
            Section = 5;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "max_sailors"));
            }
        }
    }

    public class eu4_LandForcesPerProvince : SingleValueTable
    {
        public eu4_LandForcesPerProvince() : base("LandForcesPerProvince", TableType.Province)
        {
            Caption = "Land forces (per province)";
            Category = "Military";
            Section = 6;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var armies = f.GetSubnodes(n.Value, "army");
                foreach (var a in armies)
                {
                    if (!f.HasAnAttributeWithName(a, "location")) continue;
                    var regiments = f.GetSubnodes(a, "regiment");
                    double strength = 0;
                    regiments.ForEach(x => strength += Ext.ParseDouble(f.GetAttributeValue(x, "strength")));
                    Set(UInt16.Parse(f.GetAttributeValue(a, "location")), strength);
                }
            }
        }
    }

    public class eu4_NavyForcesPerProvince : SingleValueTable
    {
        public eu4_NavyForcesPerProvince() : base("NavyForcesPerProvince", TableType.Province)
        {
            Caption = "Naval forces (per province)";
            Category = "Military";
            Section = 6;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var armies = f.GetSubnodes(n.Value, "navy");
                foreach (var a in armies)
                {
                    if (!f.HasAnAttributeWithName(a, "location")) continue;
                    var regiments = f.GetSubnodes(a, "ship");
                    double strength = 0;
                    regiments.ForEach(x => strength += Ext.ParseDouble(f.GetAttributeValue(x, "strength")));
                    Set(UInt16.Parse(f.GetAttributeValue(a, "location")), strength);
                }
            }
        }
    }

    public class eu4_LandForcesPerCountry : SingleValueTable
    {
        public eu4_LandForcesPerCountry() : base("LandForcesPerCountry", TableType.Country)
        {
            Caption = "Land forces (per country)";
            Category = "Military";
            Section = 7;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var armies = f.GetSubnodes(n.Value, "army");
                foreach (var a in armies)
                {
                    if (!f.HasAnAttributeWithName(a, "location")) continue;
                    var regiments = f.GetSubnodes(a, "regiment");
                    double strength = 0;
                    regiments.ForEach(x => strength += Ext.ParseDouble(f.GetAttributeValue(x, "strength")));
                    Set(n.Key, strength);
                }
            }
        }
    }

    public class eu4_NavyForcesPerCountry : SingleValueTable
    {
        public eu4_NavyForcesPerCountry() : base("NavyForcesPerCountry", TableType.Country)
        {
            Caption = "Naval forces (per country)";
            Category = "Military";
            Section = 7;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var armies = f.GetSubnodes(n.Value, "navy");
                foreach (var a in armies)
                {
                    if (!f.HasAnAttributeWithName(a, "location")) continue;
                    var regiments = f.GetSubnodes(a, "ship");
                    double strength = 0;
                    regiments.ForEach(x => strength += Ext.ParseDouble(f.GetAttributeValue(x, "strength")));
                    Set(n.Key, strength);
                }
            }
        }
    }

    public class eu4_FortLevel : SingleValueTable
    {
        public eu4_FortLevel() : base("FortLevel", TableType.Province)
        {
            Caption = "Fort level";
            Category = "Military";
            Section = 8;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "fort_level"));
            }
        }
    }
}
