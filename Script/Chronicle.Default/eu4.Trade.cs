using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_TradeNode : SingleDatakeyTable
    {
        public eu4_TradeNode() : base("TradeNode", TableType.Province)
        {
            Caption = "Trade node";
            Category = "Trade";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "trade"));
            }
        }
    }

    [Serializable]
    public class eu4_TradeGoods : SingleDatakeyTable
    {
        public eu4_TradeGoods() : base("TradeGoods", TableType.Province)
        {
            Caption = "Trade goods produced";
            Category = "Trade";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "trade_goods"));
            }
        }
    }

    public class eu4_TradePower : SingleValueTable
    {
        public eu4_TradePower() : base("TradePower", TableType.Province)
        {
            Caption = "Trade power";
            Category = "Trade";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "trade_power"));
            }
        }
    }

    public class eu4_TradeEmbargoes : SingleValueTable
    {
        public eu4_TradeEmbargoes() : base("TradeEmbargoes", TableType.Country)
        {
            Caption = "Number of trade embargoes";
            Category = "Trade";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "num_of_trade_embargos"));
            }
        }
    }

    public class eu4_Mercantilism : SingleValueTable
    {
        public eu4_Mercantilism() : base("Mercantilism", TableType.Country)
        {
            Caption = "Mercantilism";
            Category = "Trade";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "mercantilism"));
            }
        }
    }
}
