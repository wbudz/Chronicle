using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_WorldmarketPoolAmounts : MultiValueTable
    {
        public vic2_WorldmarketPoolAmounts() : base("WorldmarketPoolAmounts", TableType.Special)
        {
            Caption = "World market pool (amounts)";
            Category = "Trade";
            Section = 0;
        }

        public override void Parse(CEParser.File f)
        {
            var worldmarket = f.GetSubnode(f.Root, "worldmarket", "worldmarket_pool");
            var goods = f.GetAttributes(worldmarket);
            foreach (var n in goods)
            {
                Set(0, n.Key, n.Value);
            }
        }
    }

    [Serializable]
    public class vic2_WorldmarketPoolPrices : MultiValueTable
    {
        public vic2_WorldmarketPoolPrices() : base("WorldmarketPoolPrices", TableType.Special)
        {
            Caption = "World market pool (prices)";
            Category = "Trade";
            Section = 0;
        }

        public override void Parse(CEParser.File f)
        {
            var worldmarket = f.GetSubnode(f.Root, "worldmarket", "price_pool");
            var goods = f.GetAttributes(worldmarket);
            foreach (var n in goods)
            {
                Set(0, n.Key, n.Value);
            }
        }
    }

    [Serializable]
    public class vic2_WorldmarketPoolValues : MultiValueTable
    {
        public vic2_WorldmarketPoolValues() : base("WorldmarketPoolValues", TableType.Special)
        {
            Caption = "World market pool (values)";
            Category = "Trade";
            Section = 0;
        }

        public override void Parse(CEParser.File f)
        {
            MultiValueTable t1 = SelectTable("WorldmarketPoolAmounts") as MultiValueTable;
            MultiValueTable t2 = SelectTable("WorldmarketPoolPrices") as MultiValueTable;

            double[,] data1 = t1.Get(0);
            double[,] data2 = t2.Get(0);

            for (int i = 0; i < data1.GetLength(1); i++)
            {
                Set(0, (ushort)data1[0, i], data1[1, i] * data2[1, i]);
            }
        }
    }

    [Serializable]
    public class vic2_StockpileAmounts : MultiValueTable
    {
        public vic2_StockpileAmounts() : base("StockpileAmounts", TableType.Country)
        {
            Caption = "Stockpile (amounts)";
            Category = "Trade";
            Section = 1;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var stockpile = f.GetSubnode(n.Value, "stockpile");
                var goods = f.GetAttributes(stockpile);
                foreach (var g in goods)
                {
                    Set(n.Key, g.Key, g.Value);
                }
            }
        }
    }

    [Serializable]
    public class vic2_StockpileValues : MultiValueTable
    {
        public vic2_StockpileValues() : base("StockpileValues", TableType.Country)
        {
            Caption = "Stockpile (values)";
            Category = "Trade";
            Section = 1;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            MultiValueTable t = SelectTable("WorldmarketPoolPrices") as MultiValueTable;
            double[,] data = t.Get(0);
            string[] datakeys = t.GetAllDatakeys(true);

            foreach (var n in f["countries"])
            {
                var stockpile = f.GetSubnode(n.Value, "stockpile");
                var goods = f.GetAttributes(stockpile);
                foreach (var g in goods)
                {
                    try
                    {
                        double price = data[Array.FindIndex(datakeys, x => x == g.Key), 1];
                        Set(n.Key, g.Key, Ext.ParseDouble(g.Value) * price);
                    }
                    catch { }
                }
            }
        }
    }

    [Serializable]
    public class vic2_DomesticSupplyPool : MultiValueTable
    {
        public vic2_DomesticSupplyPool() : base("DomesticSupplyPool", TableType.Country)
        {
            Caption = "Domestic supply pool (by amount)";
            Category = "Trade";
            Section = 2;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var stockpile = f.GetSubnode(n.Value, "domestic_supply_pool");
                var goods = f.GetAttributes(stockpile);
                foreach (var g in goods)
                {
                    Set(n.Key, g.Key, g.Value);
                }
            }
        }
    }

    [Serializable]
    public class vic2_DomesticSupplyPoolValues : MultiValueTable
    {
        public vic2_DomesticSupplyPoolValues() : base("DomesticSupplyPoolValues", TableType.Country)
        {
            Caption = "Domestic supply pool (by value)";
            Category = "Trade";
            Section = 2;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            MultiValueTable t = SelectTable("WorldmarketPoolPrices") as MultiValueTable;
            double[,] data = t.Get(0);
            string[] datakeys = t.GetAllDatakeys(true);

            foreach (var n in f["countries"])
            {
                var stockpile = f.GetSubnode(n.Value, "domestic_supply_pool");
                var goods = f.GetAttributes(stockpile);
                foreach (var g in goods)
                {
                    try
                    {
                        double price = data[Array.FindIndex(datakeys, x => x == g.Key), 1];
                        Set(n.Key, g.Key, Ext.ParseDouble(g.Value) * price);
                    }
                    catch { }
                }
            }
        }
    }
}
