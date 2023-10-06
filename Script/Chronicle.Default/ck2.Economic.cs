using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_RulersGold : SingleValueTable
    {
        public ck2_RulersGold() : base("RulersGold", TableType.Country)
        {
            Caption = "Ruler's gold";
            Category = "Economic";
            Section = 0;

            SetColorscale(GetColor("GoldYellow"), 4);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "wealth"));
            }
        }
    }

    [Serializable]
    public class ck2_MaxSettlementsCount : SingleValueTable
    {
        public ck2_MaxSettlementsCount() : base("MaxSettlementsCount", TableType.Province)
        {
            Caption = "Maximum number of settlements";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                int max_settlements = f.GetSubnodesCount(n.Value, x => f.GetNodeName(x).StartsWith("b_"));
                if (f.HasAnAttributeWithName(n.Value, "max_settlements"))
                {
                    max_settlements = (int)Math.Max(max_settlements, Ext.ParseDouble(f.GetAttributeValue(n.Value, "max_settlements")));
                }
                Set(n.Key, max_settlements);
            }
        }
    }

    [Serializable]
    public class ck2_SettlementsCount : SingleValueTable
    {
        public ck2_SettlementsCount() : base("SettlementsCount", TableType.Province)
        {
            Caption = "Current number of settlements";
            Category = "Economic";
            Section = 1;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetSubnodesCount(n.Value, x => f.GetNodeName(x).StartsWith("b_")));
            }
        }
    }

    [Serializable]
    public class ck2_MaxLootableGold : SingleValueTable
    {
        public ck2_MaxLootableGold() : base("MaxLootableGold", TableType.Province)
        {
            Caption = "Maximum lootable gold";
            Category = "Economic";
            Section = 2;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "max_lootable_gold"));
            }
        }
    }   
}
