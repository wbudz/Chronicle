using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_ProvinceBaseTax : SingleValueTable
    {
        public eu4_ProvinceBaseTax() : base("ProvinceBaseTax", TableType.Province)
        {
            Caption = "Province base tax";
            Category = "Economic";
            Section = 0;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "base_tax"));
            }
        }
    }

    [Serializable]
    public class eu4_CountryBaseTax : SingleValueTable
    {
        public eu4_CountryBaseTax() : base("CountryBaseTax", TableType.Country)
        {
            Caption = "Country base tax";
            Category = "Economic";
            Section = 0;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "base_tax"));
            }
        }
    }

    public class eu4_ProvinceBaseProduction : SingleValueTable
    {
        public eu4_ProvinceBaseProduction() : base("ProvinceBaseProduction", TableType.Province)
        {
            Caption = "Province base production";
            Category = "Economic";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "base_production"));
            }
        }
    }

    public class eu4_Tariff : SingleValueTable
    {
        public eu4_Tariff() : base("Tariff", TableType.Country)
        {
            Caption = "Tariff";
            Category = "Economic";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "tariff"));
            }
        }
    }

    public class eu4_Treasury : SingleValueTable
    {
        public eu4_Treasury() : base("Treasury", TableType.Province)
        {
            Caption = "Treasury";
            Category = "Economic";
            Section = 2;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "treasury"));
            }
        }
    }

    [Serializable]
    public class eu4_EstimatedMonthlyIncome : SingleValueTable
    {
        public eu4_EstimatedMonthlyIncome() : base("EstimatedMonthlyIncome", TableType.Country)
        {
            Caption = "Estimated monthly income";
            Category = "Economic";
            Section = 2;

            SetColorscale("RichGreen");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "estimated_monthly_income"));
            }
        }
    }

    [Serializable]
    public class eu4_Inflation : SingleValueTable
    {
        public eu4_Inflation() : base("Inflation", TableType.Country)
        {
            Caption = "Inflation";
            Category = "Economic";
            Section = 2;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "inflation"));
            }
        }
    }

    [Serializable]
    public class eu4_ColonialMaintenance : SingleValueTable
    {
        public eu4_ColonialMaintenance() : base("ColonialMaintenance", TableType.Country)
        {
            Caption = "Colonial maintenance";
            Category = "Economic";
            Section = 3;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "colonial_maintenance"));
            }
        }
    }

    [Serializable]
    public class eu4_MissionaryMaintenance : SingleValueTable
    {
        public eu4_MissionaryMaintenance() : base("MissionaryMaintenance", TableType.Country)
        {
            Caption = "Missionary maintenance";
            Category = "Economic";
            Section = 3;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "missionary_maintenance"));
            }
        }
    }

    [Serializable]
    public class eu4_EstimatedLoanSize : SingleValueTable
    {
        public eu4_EstimatedLoanSize() : base("EstimatedLoanSize", TableType.Country)
        {
            Caption = "Estimated loan size";
            Category = "Economic";
            Section = 4;

            SetColorscale("GoldYellow");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "estimated_loan"));
            }
        }
    }
}
