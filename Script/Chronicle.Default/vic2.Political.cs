using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class vic2_Political : MultiDatakeyTable
    {
        public vic2_Political() : base("Political", TableType.Province)
        {
            Caption = "Political";
            Category = "Political";
            DisplayOnlyForSelectedCountry = false;
            ColorByValue = true;
            ValueEncoding = ValueEncoding.Country;

            SetColorscale("countries");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, "owner", TagToIndex(f.GetAttributeValue(n.Value, "owner")));
                Set(n.Key, "controller", TagToIndex(f.GetAttributeValue(n.Value, "controller")));
                if (f.HasAnAttributeWithName(n.Value, "controller") && !f.HasAnAttributeWithName(n.Value, "owner"))
                    Set(n.Key, "owner", TagToIndex(f.GetAttributeValue(n.Value, "controller")));
            }
        }
    }

    [Serializable]
    public class vic2_ProvincesCount : SingleValueTable
    {
        public vic2_ProvincesCount() : base("ProvincesCount", TableType.Country)
        {
            Caption = "Number of provinces";
            Category = "Political";
            Section = 1;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("Master") as SingleValueTable;
            if (t == null) return;

            double[] data = t.GetVector();

            for (int i = 0; i < data.Length; i++)
            {
                Set((ushort)data[i], 1);
            }
        }
    }

    [Serializable]
    public class vic2_Cores : MultiDatakeyTable
    {
        public vic2_Cores() : base("Cores", TableType.Province)
        {
            Caption = "Core provinces";
            Category = "Political";
            DisplayOnlyForSelectedCountry = true;
            ValueEncoding = ValueEncoding.Country;

            SetColorscale();
            AddColor("owned", CreateColor(60, 40, 200));
            AddColor("core", CreateColor(200, 40, 180));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                string[] cores = f.GetAttributeValues(n.Value, "core");
                Array.ForEach(cores, country => Set(n.Key, "core", TagToIndex(country)));

                Set(n.Key, "owned", TagToIndex(f.GetAttributeValue(n.Value, "owner")));
                if (f.HasAnAttributeWithName(n.Value, "controller") && !f.HasAnAttributeWithName(n.Value, "owner"))
                    Set(n.Key, "owned", TagToIndex(f.GetAttributeValue(n.Value, "controller")));
            }
        }
    }
}
