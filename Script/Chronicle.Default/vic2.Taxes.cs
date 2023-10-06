using System;

namespace Chronicle
{
    [Serializable]
    public class vic2_TaxBase : SingleValueTable
    {
        public vic2_TaxBase() : base("TaxBase", TableType.Country)
        {
            Caption = "Tax base";
            Category = "Taxes";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "tax_base"));
            }
        }
    }

    [Serializable]
    public class vic2_RichTaxLevel : SingleValueTable
    {
        public vic2_RichTaxLevel() : base("RichTaxLevel", TableType.Country)
        {
            Caption = "Rich tax level";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "rich_tax"), "current"));
            }
        }
    }

    [Serializable]
    public class vic2_RichTaxIncome : SingleValueTable
    {
        public vic2_RichTaxIncome() : base("RichTaxIncome", TableType.Country)
        {
            Caption = "Rich tax income";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "rich_tax"), "total"));
            }
        }
    }

    [Serializable]
    public class vic2_MiddleTaxLevel : SingleValueTable
    {
        public vic2_MiddleTaxLevel() : base("MiddleTaxLevel", TableType.Country)
        {
            Caption = "Middle tax level";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "middle_tax"), "current"));
            }
        }
    }

    [Serializable]
    public class vic2_MiddleTaxIncome : SingleValueTable
    {
        public vic2_MiddleTaxIncome() : base("MiddleTaxIncome", TableType.Country)
        {
            Caption = "Middle tax income";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "middle_tax"), "total"));
            }
        }
    }

    [Serializable]
    public class vic2_PoorTaxLevel : SingleValueTable
    {
        public vic2_PoorTaxLevel() : base("PoorTaxLevel", TableType.Country)
        {
            Caption = "Poor tax level";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "poor_tax"), "current"));
            }
        }
    }

    [Serializable]
    public class vic2_PoorTaxIncome : SingleValueTable
    {
        public vic2_PoorTaxIncome() : base("PoorTaxIncome", TableType.Country)
        {
            Caption = "Poor tax income";
            Category = "Taxes";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "poor_tax"), "total"));
            }
        }
    }

    [Serializable]
    public class vic2_TaxLevels : MultiValueTable
    {
        public vic2_TaxLevels() : base("TaxLevels", TableType.Country)
        {
            Caption = "Tax levels";
            Category = "Taxes";
            Section = 2;
            ParsingOrder = 1;

            SetColorscale("tax", 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t1 = SelectTable("RichTaxLevel") as SingleValueTable;
            SingleValueTable t2 = SelectTable("MiddleTaxLevel") as SingleValueTable;
            SingleValueTable t3 = SelectTable("PoorTaxLevel") as SingleValueTable;

            double[] data1 = t1.GetVector();
            double[] data2 = t2.GetVector();
            double[] data3 = t3.GetVector();

            for (ushort i = 0; i < data1.Length; i++)
            {
                Set(i, "rich", data1[i]);
                Set(i, "middle", data2[i]);
                Set(i, "poor", data3[i]);
            }
        }
    }

    [Serializable]
    public class vic2_TaxIncome : MultiValueTable
    {
        public vic2_TaxIncome() : base("TaxIncome", TableType.Country)
        {
            Caption = "Tax income composition";
            Category = "Taxes";
            Section = 2;
            ParsingOrder = 1;

            SetColorscale("tax", 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t1 = SelectTable("RichTaxIncome") as SingleValueTable;
            SingleValueTable t2 = SelectTable("MiddleTaxIncome") as SingleValueTable;
            SingleValueTable t3 = SelectTable("PoorTaxIncome") as SingleValueTable;

            double[] data1 = t1.GetVector();
            double[] data2 = t2.GetVector();
            double[] data3 = t3.GetVector();

            for (ushort i = 0; i < data1.Length; i++)
            {
                Set(i, "rich", data1[i]);
                Set(i, "middle", data2[i]);
                Set(i, "poor", data3[i]);
            }
        }
    }
}
