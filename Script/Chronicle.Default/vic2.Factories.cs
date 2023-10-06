using System;

namespace Chronicle
{
    [Serializable]
    public class vic2_FactoriesIncome : SingleValueTable
    {
        public vic2_FactoriesIncome() : base("FactoriesIncome", TableType.Country)
        {
            Caption = "Last day's pure income";
            Category = "Factories";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "last_income"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesInvestments : SingleValueTable
    {
        public vic2_FactoriesInvestments() : base("FactoriesInvestments", TableType.Country)
        {
            Caption = "Investments";
            Category = "Factories";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "last_investment"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesSpending : SingleValueTable
    {
        public vic2_FactoriesSpending() : base("FactoriesSpending", TableType.Country)
        {
            Caption = "Spending";
            Category = "Factories";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "last_spending"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesCurrentBudget : SingleValueTable
    {
        public vic2_FactoriesCurrentBudget() : base("FactoriesCurrentBudget", TableType.Country)
        {
            Caption = "Current budget";
            Category = "Factories";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "money"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesAverageProfit : SingleValueTable
    {
        public vic2_FactoriesAverageProfit() : base("FactoriesAverageProfit", TableType.Country)
        {
            Caption = "Average profit";
            Category = "Factories";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        string[] profit = f.GetEntries(f.GetSubnode(building, "profit_history_entry"));
                        double sum = 0;
                        Array.ForEach(profit, x => sum += Ext.ParseDouble(x));
                        Set(n.Key, sum / profit.Length);
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesEmployment : MultiValueTable
    {
        public vic2_FactoriesEmployment() : base("FactoriesEmployment", TableType.Country)
        {
            Caption = "Employment";
            Category = "Factories";
            Section = 1;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        double sum = 0;
                        foreach (var employees in f.GetSubnodes(building, "employment", "employees", "*"))
                        {
                            sum += (int)Ext.ParseDouble(f.GetAttributeValue(employees, "count"));
                        }
                        Set(n.Key, f.GetAttributeValue(building, "building"), sum);
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesIncomePerType : MultiValueTable
    {
        public vic2_FactoriesIncomePerType() : base("FactoriesIncomePerType", TableType.Country)
        {
            Caption = "Income (per type)";
            Category = "Factories";
            Section = 2;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "building"), f.GetAttributeValue(building, "last_income"));
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesProfitPerType : MultiValueTable
    {
        public vic2_FactoriesProfitPerType() : base("FactoriesProfitPerType", TableType.Country)
        {
            Caption = "Profit (per type)";
            Category = "Factories";
            Section = 2;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        string[] profit = f.GetEntries(f.GetSubnode(building, "profit_history_entry"));
                        double sum = 0;
                        Array.ForEach(profit, x => sum += Ext.ParseDouble(x));
                        Set(n.Key, f.GetAttributeValue(building, "building"), sum / profit.Length);
                    }
                }
            }
        }
    }

    [Serializable]
    public class vic2_FactoriesLevel : MultiValueTable
    {
        public vic2_FactoriesLevel() : base("FactoriesLevel", TableType.Country)
        {
            Caption = "Levels";
            Category = "Factories";
            Section = 2;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                foreach (var state in f.GetSubnodes(n.Value, "state"))
                {
                    foreach (var building in f.GetSubnodes(state, "state_buildings"))
                    {
                        Set(n.Key, f.GetAttributeValue(building, "building"), f.GetAttributeValue(building, "level"));
                    }
                }
            }
        }
    }
}
