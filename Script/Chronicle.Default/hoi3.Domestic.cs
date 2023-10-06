using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_Dissent : SingleValueTable
    {
        public hoi3_Dissent() : base("Dissent", TableType.Country)
        {
            Caption = "Dissent";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "dissent"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceRevoltrisk : SingleValueTable
    {
        public hoi3_ProvinceRevoltrisk() : base("ProvinceRevoltrisk", TableType.Province)
        {
            Caption = "Province revolt risk";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "revolt_risk"));
            }
        }
    }

    [Serializable]
    public class hoi3_NationalUnity : SingleValueTable
    {
        public hoi3_NationalUnity() : base("NationalUnity", TableType.Country)
        {
            Caption = "National unity";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "national_unity"));
            }
        }
    }

    [Serializable]
    public class hoi3_Ideology : SingleDatakeyTable
    {
        public hoi3_Ideology() : base("Ideology", TableType.Country)
        {
            Caption = "Ideology";
            Category = "Domestic";
            Section = 1;

            SetColorscale("ideologies");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "ideology"));
            }
        }
    }
}
