using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_ProvinceCulture : SingleDatakeyTable
    {
        public ck2_ProvinceCulture() : base("ProvinceCulture", TableType.Province)
        {
            Caption = "Province culture";
            Category = "Social";
            Section = 0;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "culture"));
            }
        }
    }

    [Serializable]
    public class ck2_CountryCulture : SingleDatakeyTable
    {
        public ck2_CountryCulture() : base("CountryCulture", TableType.Country)
        {
            Caption = "Country ruler culture";
            Category = "Social";
            Section = 0;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                var culture = f.GetAttributeValue(n.Value, "cul");
                if (culture == "")
                {
                    var dynasty = f.GetAttributeValue(n.Value, "dnt");
                    culture = f.GetAttributeValue(f.GetSubnode(f.Root, "dynasties", dynasty), "culture");
                    if (culture == "")
                    {
                        culture = DynamicDefs.GetTextValue("dynastiescultures", dynasty);
                    }
                }
                Set(n.Key, culture);
            }
        }
    }

    [Serializable]
    public class ck2_CountryCultures : MultiValueTable
    {
        public ck2_CountryCultures() : base("CountryCultures", TableType.Country)
        {
            Caption = "Country cultures";
            Category = "Social";
            Section = 0;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceCulture");
        }
    }

    [Serializable]
    public class ck2_ProvinceReligion : SingleDatakeyTable
    {
        public ck2_ProvinceReligion() : base("ProvinceReligion", TableType.Province)
        {
            Caption = "Province religion";
            Category = "Social";
            Section = 1;

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
    public class ck2_CountryReligion : SingleDatakeyTable
    {
        public ck2_CountryReligion() : base("CountryReligion", TableType.Country)
        {
            Caption = "Country ruler religion";
            Category = "Social";
            Section = 1;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                var religion = f.GetAttributeValue(n.Value, "rel");
                if (religion == "")
                {
                    var dynasty = f.GetAttributeValue(n.Value, "dnt");
                    religion = f.GetAttributeValue(f.GetSubnode(f.Root, "dynasties", dynasty), "religion");
                    if (religion == "")
                    {
                        religion = f.GetAttributeValue(f.GetSubnode(f.Root, "dynasties", dynasty, "coat_of_arms"), "religion");
                    }
                }
                Set(n.Key, religion);
            }
        }
    }

    [Serializable]
    public class ck2_CountryReligions : MultiValueTable
    {
        public ck2_CountryReligions() : base("CountryReligions", TableType.Country)
        {
            Caption = "Country religions";
            Category = "Social";
            Section = 1;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceReligion");
        }
    }
}
