using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu3_Gold : SingleValueTable
    {
        public eu3_Gold() : base("Gold", TableType.Country)
        {
            Caption = "Gold";
            Category = "Economic";
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 4);
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
    public class eu3_BaseTax : SingleValueTable
    {
        public eu3_BaseTax() : base("BaseTax", TableType.Province)
        {
            Caption = "Base tax";
            Category = "Economic";
            Section = 1;
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 1);
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
    public class eu3_EstimatedMonthlyIncome : SingleValueTable
    {
        public eu3_EstimatedMonthlyIncome() : base("EstimatedMonthlyIncome", TableType.Country)
        {
            Caption = "Estimated monthly income";
            Category = "Economic";
            Section = 2;
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 1);
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
    public class eu3_LastMonthIncome : SingleValueTable
    {
        public eu3_LastMonthIncome() : base("LastMonthIncome", TableType.Country)
        {
            Caption = "Last month income";
            Category = "Economic";
            Section = 2;
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "ledger"), "lastmonthincome"));
            }
        }
    }

    [Serializable]
    public class eu3_LastMonthExpense : SingleValueTable
    {
        public eu3_LastMonthExpense() : base("LastMonthExpense", TableType.Country)
        {
            Caption = "Last month expense";
            Category = "Economic";
            Section = 2;
            RequiresSavegame = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "ledger"), "lastmonthexpense"));
            }
        }
    }

    [Serializable]
    public class eu3_LastMonthNetGain : SingleValueTable
    {
        public eu3_LastMonthNetGain() : base("LastMonthNetGain", TableType.Country)
        {
            Caption = "Last month net gain";
            Category = "Economic";
            Section = 2;
            ParsingOrder = 1;
            RequiresSavegame = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t1 = SelectTable("LastMonthIncome") as SingleValueTable;
            SingleValueTable t2 = SelectTable("LastMonthExpense") as SingleValueTable;
            if (t1 == null || t2 == null) return;

            double[] data1 = t1.GetVector();
            double[] data2 = t2.GetVector();

            for (ushort i = 1; i < data1.Length && i < data2.Length; i++)
            {
                Set(i, data1[i] - data2[i]);
            }
        }
    }

    [Serializable]
    public class eu3_Inflation : SingleValueTable
    {
        public eu3_Inflation() : base("Inflation", TableType.Country)
        {
            Caption = "Inflation";
            Category = "Economic";
            Section = 3;
            RequiresSavegame = true;

            SetColorscale(GetColor("RichGreen"),GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value,"inflation"));
            }
        }
    }
}
