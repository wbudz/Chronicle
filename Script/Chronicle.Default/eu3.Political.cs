using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu3_Political : MultiDatakeyTable
    {
        public eu3_Political() : base("Political", TableType.Province)
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
    public class eu3_ProvincesCount : SingleValueTable
    {
        public eu3_ProvincesCount() : base("ProvincesCount", TableType.Country)
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
            ushort[] masters = GetProvinceMasters();

            for (int i = 0; i < masters.Length; i++)
            {
                Set(masters[i], 1);
            }
        }
    }

    [Serializable]
    public class eu3_Cores : MultiDatakeyTable
    {
        public eu3_Cores() : base("Cores", TableType.Province)
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

    [Serializable]
    public class eu3_Dynasty : SingleDatakeyTable
    {
        public eu3_Dynasty() : base("Dynasty", TableType.Country)
        {
            Caption = "Dynasty";
            Category = "Political";
            Section = 2;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach(var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "dynasty"));
            }
        }
    }
}
