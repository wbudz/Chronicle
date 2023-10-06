using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu3_CoTValue : SingleValueTable
    {
        public eu3_CoTValue() : base("CoTValue", TableType.Province)
        {
            Caption = "Center of trade: value";
            Category = "Economic";
            Section = 0;
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var cot in f.GetSubnodes(f.Root, "trade", "cot"))
            {
                Set(f.GetAttributeValue(cot, "location"), f.GetAttributeValue(cot, "value"));
            }
        }
    }

    [Serializable]
    public class eu3_CoTControl : MultiValueTable
    {
        public eu3_CoTControl() : base("CoTControl", TableType.Province)
        {
            Caption = "Center of trade: control";
            Category = "Economic";
            Section = 0;
            RequiresSavegame = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var cot in f.GetSubnodes(f.Root, "trade", "cot"))
            {
                var countries = f.GetSubnodes(cot);
                foreach (var c in countries)
                {
                    Set(f.GetAttributeValue(cot, "location"), f.GetNodeName(c), f.GetAttributeValue(c, "value"));
                }
            }
        }
    }

    [Serializable]
    public class eu3_ProvinceTradeGoods : SingleDatakeyTable
    {
        public eu3_ProvinceTradeGoods() : base("ProvinceTradeGoods", TableType.Province)
        {
            Caption = "Province trade goods";
            Category = "Economic";
            Section = 1;
            RequiresSavegame = true;

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

    [Serializable]
    public class eu3_CountryTradeGoods : MultiValueTable
    {
        public eu3_CountryTradeGoods() : base("CountryTradeGoods", TableType.Country)
        {
            Caption = "Country trade goods";
            Category = "Economic";
            Section = 1;
            RequiresSavegame = true;
            AggregateValues = true;
            ParsingOrder = 1;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            SingleDatakeyTable t = SelectTable("ProvinceTradeGoods") as SingleDatakeyTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            string[] data = t.GetVector();

            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i], 1);
            }
        }
    }

    [Serializable]
    public class eu3_MerchantsAvailable : SingleValueTable
    {
        public eu3_MerchantsAvailable() : base("MerchantsAvailable", TableType.Country)
        {
            Caption = "Merchants available";
            Category = "Economic";
            Section = 2;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "merchants"));
            }
        }
    }
}
