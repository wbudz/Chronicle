using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_GenderSuccession : SingleDatakeyTable
    {
        public ck2_GenderSuccession() : base("GenderSuccession", TableType.Country)
        {
            Caption = "Gender succession";
            Category = "Laws";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "gender"));
            }
        }
    }

    [Serializable]
    public class ck2_SuccessionOrder : SingleDatakeyTable
    {
        public ck2_SuccessionOrder() : base("SuccessionOrder", TableType.Country)
        {
            Caption = "Succession order";
            Category = "Laws";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "succession"));
            }
        }
    }

    [Serializable]
    public class ck2_CrownAuthority : SingleValueTable
    {
        public ck2_CrownAuthority() : base("CrownAuthority", TableType.Country)
        {
            Caption = "Crown authority";
            Category = "Laws";
            Section = 0;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("crown_authority_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("crown_authority_", ""), out score);
                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_Centralization : SingleValueTable
    {
        public ck2_Centralization() : base("Centralization", TableType.Country)
        {
            Caption = "Centralization";
            Category = "Laws";
            Section = 0;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("centralization_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("centralization_", ""), out score);
                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_FeudalContract : SingleValueTable
    {
        public ck2_FeudalContract() : base("FeudalContract", TableType.Country)
        {
            Caption = "Feudal authority";
            Category = "Laws";
            Section = 1;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("feudal_contract_"));
                if (law == null)
                {
                    law = laws.FirstOrDefault(x => x.StartsWith("iqta_contract_"));
                }
                if (law == null) continue;
                Int32.TryParse(law.Replace("feudal_contract_", "").Replace("iqta_contract_", ""), out score);
                
                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_FeudalTax : SingleValueTable
    {
        public ck2_FeudalTax() : base("FeudalTax", TableType.Country)
        {
            Caption = "Feudal tax";
            Category = "Laws";
            Section = 2;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("feudal_tax_"));
                if (law == null)
                {
                    law = laws.FirstOrDefault(x => x.StartsWith("iqta_tax_"));
                }
                if (law == null) continue;
                Int32.TryParse(law.Replace("feudal_tax_", "").Replace("iqta_tax_", ""), out score);

                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_CityContract : SingleValueTable
    {
        public ck2_CityContract() : base("CityContract", TableType.Country)
        {
            Caption = "City authority";
            Category = "Laws";
            Section = 1;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("city_contract_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("city_contract_", ""), out score);

                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_CityTax : SingleValueTable
    {
        public ck2_CityTax() : base("CityTax", TableType.Country)
        {
            Caption = "City tax";
            Category = "Laws";
            Section = 2;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("city_tax_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("city_tax_", ""), out score);

                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_TempleContract : SingleValueTable
    {
        public ck2_TempleContract() : base("TempleContract", TableType.Country)
        {
            Caption = "Temple authority";
            Category = "Laws";
            Section = 1;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("temple_contract_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("temple_contract_", ""), out score);

                Set(n.Key, score);
            }
        }
    }

    [Serializable]
    public class ck2_TempleTax : SingleValueTable
    {
        public ck2_TempleTax() : base("TempleTax", TableType.Country)
        {
            Caption = "Temple tax";
            Category = "Laws";
            Section = 2;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                int score = 0;
                string[] laws = f.GetAttributeValues(n.Value, "law");
                string law = laws.FirstOrDefault(x => x.StartsWith("temple_tax_"));
                if (law == null) continue;
                Int32.TryParse(law.Replace("temple_tax_", ""), out score);

                Set(n.Key, score);
            }
        }
    }
}
