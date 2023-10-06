using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_Money : SingleValueTable
    {
        public vic2_Money() : base("Money", TableType.Country)
        {
            Caption = "Money";
            Category = "Economic";
            Section = 0;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "money"));
            }
        }
    }

    [Serializable]
    public class vic2_NationalBankMoney : SingleValueTable
    {
        public vic2_NationalBankMoney() : base("NationalBankMoney", TableType.Country)
        {
            Caption = "National bank money";
            Category = "Economic";
            Section = 0;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "bank"), "money"));
            }
        }
    }

    [Serializable]
    public class vic2_NationalBankMoneyLent : SingleValueTable
    {
        public vic2_NationalBankMoneyLent() : base("NationalBankMoneyLent", TableType.Country)
        {
            Caption = "Money lent by national bank";
            Category = "Economic";
            Section = 0;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "bank"), "money_lent"));
            }
        }
    }

    [Serializable]
    public class vic2_TotalDebt : SingleValueTable
    {
        public vic2_TotalDebt() : base("TotalDebt", TableType.Country)
        {
            Caption = "Total debt";
            Category = "Economic";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var creditors = f.GetSubnodes(n.Value, "creditor");
                double sum = 0;
                foreach (var c in creditors)
                {
                    sum += Ext.ParseDouble(f.GetAttributeValue(c, "debt"));
                }
                Set(n.Key, sum);
            }
        }
    }

    [Serializable]
    public class vic2_EducationSpendingLevel : SingleValueTable
    {
        public vic2_EducationSpendingLevel() : base("EducationSpendingLevel", TableType.Country)
        {
            Caption = "Education spending level";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "education_spending"), "settings"));
            }
        }
    }

    [Serializable]
    public class vic2_CrimeFightingLevel : SingleValueTable
    {
        public vic2_CrimeFightingLevel() : base("CrimeFightingLevel", TableType.Country)
        {
            Caption = "Crime fighting level";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "crime_fighting"), "settings"));
            }
        }
    }

    [Serializable]
    public class vic2_SocialSpendingLevel : SingleValueTable
    {
        public vic2_SocialSpendingLevel() : base("SocialSpendingLevel", TableType.Country)
        {
            Caption = "Social spending level";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "social_spending"), "settings"));
            }
        }
    }

    [Serializable]
    public class vic2_MilitarySpendingLevel : SingleValueTable
    {
        public vic2_MilitarySpendingLevel() : base("MilitarySpendingLevel", TableType.Country)
        {
            Caption = "Military spending level";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "military_spending"), "settings"));
            }
        }
    }

    [Serializable]
    public class vic2_RGOIncome : SingleValueTable
    {
        public vic2_RGOIncome() : base("RGOIncome", TableType.Province)
        {
            Caption = "RGO income";
            Category = "Economic";
            Section = 2;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "rgo"), "last_income"));
            }
        }
    }
}
