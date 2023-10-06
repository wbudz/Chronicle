using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_LandRegimentsPerProvince : SingleValueTable
    {
        public vic2_LandRegimentsPerProvince() : base("ProvinceLandRegiments", TableType.Province)
        {
            Caption = "Land regiments (per province)";
            Category = "Military";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "regiment");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "army", 1)) continue;
                Set(f.GetAttributeValue(f.GetParent(regiment), "location"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class vic2_LandRegimentsPerCountry : SingleValueTable
    {
        public vic2_LandRegimentsPerCountry() : base("LandRegimentsPerCountry", TableType.Country)
        {
            Caption = "Land regiments (per country)";
            Category = "Military";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "regiment");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "army", 1)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class vic2_LandRegimentsPerCountryComposition : MultiValueTable
    {
        public vic2_LandRegimentsPerCountryComposition() : base("LandRegimentsPerCountryComposition", TableType.Country)
        {
            Caption = "Land regiments composition (per country)";
            Category = "Military";
            Section = 0;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "regiment");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "army", 1)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "type"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class vic2_SeaRegimentsPerProvince : SingleValueTable
    {
        public vic2_SeaRegimentsPerProvince() : base("ProvinceSeaRegiments", TableType.Province)
        {
            Caption = "Ships or squadrons (per province)";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "ship");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "navy", 1)) continue;
                Set(f.GetAttributeValue(f.GetParent(regiment), "location"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class vic2_SeaRegimentsPerCountry : SingleValueTable
    {
        public vic2_SeaRegimentsPerCountry() : base("SeaRegimentsPerCountry", TableType.Country)
        {
            Caption = "Ships or squadrons (per country)";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "ship");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "navy", 1)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class vic2_SeaRegimentsPerCountryComposition : MultiValueTable
    {
        public vic2_SeaRegimentsPerCountryComposition() : base("SeaRegimentsPerCountryComposition", TableType.Country)
        {
            Caption = "Ships or squadrons composition (per country)";
            Category = "Military";
            Section = 1;
            AggregateValues = true;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "ship");
            foreach (var regiment in regiments)
            {
                if (!f.HasAParent(regiment, "navy", 1)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "type"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }
}
