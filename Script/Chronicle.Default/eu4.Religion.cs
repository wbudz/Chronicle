using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_ProvinceReligion : SingleDatakeyTable
    {
        public eu4_ProvinceReligion() : base("ProvinceReligion", TableType.Province)
        {
            Caption = "Province religion";
            Category = "Religion";
            Section = 0;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "religion"));
            }
        }
    }

    [Serializable]
    public class eu4_CountryReligions : MultiValueTable
    {
        public eu4_CountryReligions() : base("CountryReligions", TableType.Country)
        {
            Caption = "Country religions";
            Category = "Religion";
            Section = 0;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceReligion");
        }
    }

    [Serializable]
    public class eu4_CountryStateReligion : SingleDatakeyTable
    {
        public eu4_CountryStateReligion() : base("CountryStateReligion", TableType.Country)
        {
            Caption = "Country state religion";
            Category = "Religion";
            Section = 0;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "religion"));
            }
        }
    }

    [Serializable]
    public class eu4_CardinalsLocation : SingleValueTable
    {
        public eu4_CardinalsLocation() : base("CardinalsLocation", TableType.Province)
        {
            Caption = "Cardinals locations";
            Category = "Diplomatic";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            var cardinals = f.GetSubnodes(f.Root, "religions", "catholic", "papacy", "active_cardinals", "*");

            foreach (var n in cardinals)
            {
                Set(UInt16.Parse(f.GetAttributeValue(n, 0)), 1);
            }
        }
    }

    [Serializable]
    public class eu4_PapalInfluence : SingleValueTable
    {
        public eu4_PapalInfluence() : base("PapalInfluence", TableType.Province)
        {
            Caption = "Papal influence";
            Category = "Religion";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "papal_influence"));
            }
        }
    }

    [Serializable]
    public class eu4_ReligiousUnity : SingleValueTable
    {
        public eu4_ReligiousUnity() : base("ReligiousUnity", TableType.Country)
        {
            Caption = "Religious unity";
            Category = "Religion";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "religious_unity"));
            }
        }
    }
}
