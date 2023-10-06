using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_Political : MultiDatakeyTable
    {
        public eu4_Political() : base("Political", TableType.Province)
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
    public class eu4_ProvincesCount : SingleValueTable
    {
        public eu4_ProvincesCount() : base("ProvincesCount", TableType.Country)
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
    public class eu4_Cores : MultiDatakeyTable
    {
        public eu4_Cores() : base("Cores", TableType.Province)
        {
            Caption = "Core and claimed provinces";
            Category = "Political";
            DisplayOnlyForSelectedCountry = true;
            ValueEncoding = ValueEncoding.Country;

            SetColorscale();
            AddColor("owned", CreateColor(60, 40, 200));
            AddColor("core", CreateColor(200, 40, 180));
            AddColor("claim", CreateColor(150, 180, 60));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                string[] cores = f.GetAttributeValues(n.Value, "core");
                Array.ForEach(cores, country => Set(n.Key, "core", TagToIndex(country)));

                string[] claims = f.GetAttributeValues(n.Value, "claim");
                Array.ForEach(cores, country => Set(n.Key, "claim", TagToIndex(country)));

                Set(n.Key, "owned", TagToIndex(f.GetAttributeValue(n.Value, "owner")));
                if (f.HasAnAttributeWithName(n.Value, "controller") && !f.HasAnAttributeWithName(n.Value, "owner"))
                    Set(n.Key, "owned", TagToIndex(f.GetAttributeValue(n.Value, "controller")));
            }
        }
    }

    [Serializable]
    public class eu4_HRE : MultiDatakeyTable
    {
        public eu4_HRE() : base("HRE", TableType.Province)
        {
            Caption = "Holy Roman Empire";
            Category = "Political";

            SetColorscale();
            AddColor("hre", CreateColor(140, 60, 160));
            AddColor("elector", CreateColor(180, 40, 230));
            AddColor("emperor", CreateColor(240, 180, 250));
        }

        public override void Parse(CEParser.File f)
        {
            ushort[] master = GetProvinceMasters();

            var emperor = f.GetAttributeValue(f.Root, "emperor");
            for (ushort i = 0; i < master.Length; i++)
            {
                if (master[i] == TagToIndex(emperor))
                    Set(i, "emperor", 1);
            }

            foreach (var n in f["countries"])
            {
                if (f.HasAnAttribute(n.Value, "is_elector", "yes"))
                {
                    for (ushort i = 0; i < master.Length; i++)
                    {
                        if (master[i] == n.Key)
                            Set(i, "elector", 1);
                    }
                }
            }

            foreach (var n in f["provinces"])
            {
                if (f.HasAnAttribute(n.Value, "hre", "yes"))
                    Set(n.Key, "hre", 1);
            }
        }
    }
    
}
