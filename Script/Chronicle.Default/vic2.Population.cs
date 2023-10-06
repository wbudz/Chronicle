using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_ProvincePopulation : SingleValueTable
    {
        public vic2_ProvincePopulation() : base("ProvincePopulation", TableType.Province)
        {
            Caption = "Population (per province)";
            Category = "Population";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetAttributeValue(pop, "size"));
                }
            }
        }
    }

    [Serializable]
    public class vic2_CountryPopulation : SingleValueTable
    {
        public vic2_CountryPopulation() : base("CountryPopulation", TableType.Country)
        {
            Caption = "Country population";
            Category = "Population";
            Section = 0;
            AggregateValues = true;
            ParsingOrder = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            //SingleValueTable t = SelectTable("ProvincePopulation") as SingleValueTable;
            //ushort[] master = GetProvinceMasters();
            //double[] data = t.GetVector();
            //for (int i = 0; i < data.Length; i++)
            //{
            //    Set(master[i], data[i]);
            //}
            Aggregate("ProvincePopulation");
        }
    }

    public class vic2_ProvinceOccupations : MultiValueTable
    {
        public vic2_ProvinceOccupations() : base("ProvinceOccupations", TableType.Province)
        {
            Caption = "Occupations (per province)";
            Category = "Population";
            Section = 1;
            AggregateValues = true;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetNodeName(pop), f.GetAttributeValue(pop, "size"));
                }
            }
        }
    }

    public class vic2_CountryOccupations : MultiValueTable
    {
        public vic2_CountryOccupations() : base("CountryOccupations", TableType.Country)
        {
            Caption = "Occupations (per country)";
            Category = "Population";
            Section = 1;
            ParsingOrder = 1;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceOccupations");
        }
    }

    public class vic2_ProvinceOccupationsWealth : MultiValueTable
    {
        public vic2_ProvinceOccupationsWealth() : base("ProvinceOccupationsWealth", TableType.Province)
        {
            Caption = "Personal wealth across occupations (per province)";
            Category = "Population";
            Section = 1;
            AggregateValues = true;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetNodeName(pop), f.GetAttributeValue(pop, "money"));
                }
            }
        }
    }

    public class vic2_CountryOccupationsWealth : MultiValueTable
    {
        public vic2_CountryOccupationsWealth() : base("CountryOccupationsWealth", TableType.Country)
        {
            Caption = "Personal wealth across occupations (per country)";
            Category = "Population";
            Section = 1;
            ParsingOrder = 1;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceOccupationsWealth");
        }
    }

    public class vic2_ProvinceOccupationsSavings : MultiValueTable
    {
        public vic2_ProvinceOccupationsSavings() : base("ProvinceOccupationsSavings", TableType.Province)
        {
            Caption = "Personal savings across occupations (per province)";
            Category = "Population";
            Section = 1;
            AggregateValues = true;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetNodeName(pop), f.GetAttributeValue(pop, "bank"));
                }
            }
        }
    }

    public class vic2_CountryOccupationsSavings : MultiValueTable
    {
        public vic2_CountryOccupationsSavings() : base("CountryOccupationsSavings", TableType.Country)
        {
            Caption = "Personal savings across occupations (per country)";
            Category = "Population";
            Section = 1;
            ParsingOrder = 1;

            SetColorscale("occupations");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceOccupationsSavings");
        }
    }

    public class vic2_ProvinceReligion : MultiValueTable
    {
        public vic2_ProvinceReligion() : base("ProvinceReligion", TableType.Province)
        {
            Caption = "Province religions";
            Category = "Population";
            Section = 2;
            AggregateValues = true;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetAttributeValue(pop, 2), f.GetAttributeValue(pop, "size"));
                }
            }
        }
    }

    public class vic2_CountryReligion : MultiValueTable
    {
        public vic2_CountryReligion() : base("CountryReligion", TableType.Country)
        {
            Caption = "Country religions";
            Category = "Population";
            Section = 2;
            ParsingOrder = 1;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceReligion");
        }
    }

    public class vic2_ProvinceNationality : MultiValueTable
    {
        public vic2_ProvinceNationality() : base("ProvinceNationality", TableType.Province)
        {
            Caption = "Province nationalities";
            Category = "Population";
            Section = 3;
            AggregateValues = true;

            SetColorscale("nationalities");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                foreach (var pop in pops)
                {
                    Set(n.Key, f.GetAttributeName(pop, 2), f.GetAttributeValue(pop, "size"));
                }
            }
        }
    }

    public class vic2_CountryNationality : MultiValueTable
    {
        public vic2_CountryNationality() : base("CountryNationality", TableType.Country)
        {
            Caption = "Country nationalities";
            Category = "Population";
            Section = 3;
            AggregateValues = true;

            SetColorscale("nationalities");
        }

        public override void Parse(CEParser.File f)
        {
            Aggregate("ProvinceNationality");
        }
    }

    public class vic2_AverageConsciousness : SingleValueTable
    {
        public vic2_AverageConsciousness() : base("AverageConsciousness", TableType.Province)
        {
            Caption = "Average consciousness";
            Category = "Population";
            Section = 4;
            AggregateValues = true;

            SetColorscale(GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                double num = 0;
                double den = 0;
                foreach (var pop in pops)
                {
                    num += Ext.ParseDouble(f.GetAttributeValue(pop, "con")) * Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                    den += Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                }
                if (den != 0) Set(n.Key, num / den);
            }
        }
    }

    public class vic2_AverageMilitancy : SingleValueTable
    {
        public vic2_AverageMilitancy() : base("AverageMilitancy", TableType.Province)
        {
            Caption = "Average militancy";
            Category = "Population";
            Section = 4;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                double num = 0;
                double den = 0;
                foreach (var pop in pops)
                {
                    num += Ext.ParseDouble(f.GetAttributeValue(pop, "mil")) * Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                    den += Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                }
                if (den != 0) Set(n.Key, num / den);
            }
        }
    }

    public class vic2_AverageLiteracy : SingleValueTable
    {
        public vic2_AverageLiteracy() : base("AverageLiteracy", TableType.Province)
        {
            Caption = "Average literacy";
            Category = "Population";
            Section = 4;
            AggregateValues = true;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var pops = f.GetSubnodes(n.Value).Where(x => DynamicDefs.IsOnList("pops", f.GetNodeName(x)));
                double num = 0;
                double den = 0;
                foreach (var pop in pops)
                {
                    num += Ext.ParseDouble(f.GetAttributeValue(pop, "literacy")) * Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                    den += Ext.ParseDouble(f.GetAttributeValue(pop, "size"));
                }
                if (den != 0) Set(n.Key, num / den);
            }
        }
    }
}
