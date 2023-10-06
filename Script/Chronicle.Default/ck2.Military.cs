using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_ProvinceLevySize : SingleValueTable
    {
        public ck2_ProvinceLevySize() : base("ProvinceLevySize", TableType.Province)
        {
            Caption = "Province levy size";
            Category = "Military";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                double sum = 0;
                foreach (CEParser.Node o in f.GetSubnodes(n.Value, x => f.GetNodeName(x).StartsWith("b_")))
                {
                    f.GetSubnodes(o, "levy", "*").ForEach(x => sum += Ext.ParseDouble(f.GetEntries(x)[0]));
                }
                Set(n.Key, sum);
            }
        }
    }

    [Serializable]
    public class ck2_CountryLevySize : SingleValueTable
    {
        public ck2_CountryLevySize() : base("CountryLevySize", TableType.Country)
        {
            Caption = "Country levy size";
            Category = "Military";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceLevySize");
        }
    }

    [Serializable]
    public class ck2_ProvinceLevyComposition : MultiValueTable
    {
        public ck2_ProvinceLevyComposition() : base("ProvinceLevyComposition", TableType.Province)
        {
            Caption = "Province levy composition";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var baronies = f.GetSubnodes(n.Value, x => f.GetNodeName(x).StartsWith("b_"));
                foreach (CEParser.Node b in baronies)
                {
                    foreach (var l in f.GetSubnodes(b, "levy", "*"))
                    {
                        Set(n.Key, f.GetNodeName(l), f.GetEntry(l, 0));
                    }
                }
            }
        }
    }

    [Serializable]
    public class ck2_CountryLevyComposition : MultiValueTable
    {
        public ck2_CountryLevyComposition() : base("CountryLevyComposition", TableType.Country)
        {
            Caption = "Country levy composition";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceLevyComposition");
        }
    }
}
