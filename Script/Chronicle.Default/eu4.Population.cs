using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_ProvinceCulture : SingleDatakeyTable
    {
        public eu4_ProvinceCulture() : base("ProvinceCulture", TableType.Province)
        {
            Caption = "Province culture";
            Category = "Population";
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
    public class eu4_CountryCultures : MultiValueTable
    {
        public eu4_CountryCultures() : base("CountryCultures", TableType.Country)
        {
            Caption = "Country cultures";
            Category = "Population";
            Section = 0;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceCulture");
        }
    }

    public class eu4_NativeSize : SingleValueTable
    {
        public eu4_NativeSize() : base("NativeSize", TableType.Province)
        {
            Caption = "Natives count";
            Category = "Population";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "native_size"));
            }
        }
    }

    public class eu4_NativeHostileness : SingleValueTable
    {
        public eu4_NativeHostileness() : base("NativeHostileness", TableType.Province)
        {
            Caption = "Natives hostileness";
            Category = "Population";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "native_hostileness"));
            }
        }
    }

    public class eu4_NativeFerocity : SingleValueTable
    {
        public eu4_NativeFerocity() : base("NativeFerocity", TableType.Province)
        {
            Caption = "Natives ferocity";
            Category = "Population";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "native_ferocity"));
            }
        }
    }

    [Serializable]
    public class eu4_CountryPrimaryCulture : SingleDatakeyTable
    {
        public eu4_CountryPrimaryCulture() : base("CountryPrimaryCulture", TableType.Country)
        {
            Caption = "Primary culture (country)";
            Category = "Population";
            Section = 2;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "primary_culture"));
            }
        }
    }

    [Serializable]
    public class eu4_CountryAcceptedCultures : MultiDatakeyTable
    {
        public eu4_CountryAcceptedCultures() : base("CountryAcceptedCultures", TableType.Country)
        {
            Caption = "Accepted cultures (country)";
            Category = "Population";
            Section = 2;

            SetColorscale("cultures");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                string[] cultures = f.GetAttributeValues(n.Value, "accepted_culture");
                Array.ForEach(cultures, x => Set(n.Key, x));
            }
        }
    }
}
