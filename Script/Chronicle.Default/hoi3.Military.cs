using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_ProvinceLandDivisions : SingleValueTable
    {
        public hoi3_ProvinceLandDivisions() : base("ProvinceLandDivisions", TableType.Province)
        {
            Caption = "Province land divisions";
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
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetAttributeValue(f.GetParent(regiment), "location"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryLandDivisions : SingleValueTable
    {
        public hoi3_CountryLandDivisions() : base("CountryLandDivisions", TableType.Country)
        {
            Caption = "Country land divisions";
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
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_LandDivisionsComposition : MultiValueTable
    {
        public hoi3_LandDivisionsComposition() : base("LandDivisionsComposition", TableType.Country)
        {
            Caption = "Army composition";
            Category = "Military";
            Section = 0;
            AggregateValues = true;

            SetColorscale("land_divisions");
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "regiment");
            foreach (var regiment in regiments)
            {
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "type"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }


    [Serializable]
    public class hoi3_ProvinceSeaDivisions : SingleValueTable
    {
        public hoi3_ProvinceSeaDivisions() : base("ProvinceSeaDivisions", TableType.Province)
        {
            Caption = "Province ships/squadrons";
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
                if (f.HasAParent(regiment, "unit_deployment", 2) || f.HasAParent(regiment, "convoy")) continue;
                Set(f.GetAttributeValue(f.GetParent(regiment), "location"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountrySeaDivisions : SingleValueTable
    {
        public hoi3_CountrySeaDivisions() : base("CountrySeaDivisions", TableType.Country)
        {
            Caption = "Country ships/squadrons";
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
                if (f.HasAParent(regiment, "unit_deployment", 2) || f.HasAParent(regiment, "convoy")) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_SeaDivisionsComposition : MultiValueTable
    {
        public hoi3_SeaDivisionsComposition() : base("SeaDivisionsComposition", TableType.Country)
        {
            Caption = "Navy composition";
            Category = "Military";
            Section = 0;
            AggregateValues = true;

            SetColorscale("sea_divisions");
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "ship");
            foreach (var regiment in regiments)
            {
                if (f.HasAParent(regiment, "unit_deployment", 2) || f.HasAParent(regiment, "convoy")) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "type"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }


    [Serializable]
    public class hoi3_ProvinceAirDivisions : SingleValueTable
    {
        public hoi3_ProvinceAirDivisions() : base("ProvinceAirDivisions", TableType.Province)
        {
            Caption = "Province air wings";
            Category = "Military";
            Section = 2;
            AggregateValues = true;

            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1, GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "wing");
            foreach (var regiment in regiments)
            {
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetAttributeValue(f.GetParent(regiment), "location"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryAirDivisions : SingleValueTable
    {
        public hoi3_CountryAirDivisions() : base("CountryAirDivisions", TableType.Country)
        {
            Caption = "Country air wings";
            Category = "Military"; ;
            Section = 2;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "wing");
            foreach (var regiment in regiments)
            {
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }

    [Serializable]
    public class hoi3_AirDivisionsComposition : MultiValueTable
    {
        public hoi3_AirDivisionsComposition() : base("AirDivisionsComposition", TableType.Country)
        {
            Caption = "Airforce composition";
            Category = "Military";
            Section = 2;
            AggregateValues = true;

            SetColorscale("air_divisions");
        }

        public override void Parse(CEParser.File f)
        {
            var regiments = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "wing");
            foreach (var regiment in regiments)
            {
                if (f.HasAParent(regiment, "unit_deployment", 2)) continue;
                Set(f.GetNodeName(f.GetTopParent(regiment)), f.GetAttributeValue(regiment, "type"), f.GetAttributeValue(regiment, "strength"));
            }
        }
    }



    [Serializable]
    public class hoi3_ProvinceConvoys : SingleValueTable
    {
        public hoi3_ProvinceConvoys() : base("ProvinceConvoys", TableType.Province)
        {
            Caption = "Convoy routes";
            Category = "Military";
            Section = 3;
            AggregateValues = true;
            
            SetColorscale(GetColor("Empty"), GetColor("DeepRed"), 1,
                GetColor("LightBlue"), GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var convoys = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "convoy");
            foreach (var convoy in convoys)
            {
                string[] path = f.GetEntries(f.GetSubnode(convoy, "path"));
                for (int i = 0; i < path.Length; i++)
                {
                    Set(path[i], f.GetAttributeValue(convoy, "convoys"));
                }
            }
        }
    }

    [Serializable]
    public class hoi3_CountryConvoys : SingleValueTable
    {
        public hoi3_CountryConvoys() : base("CountryConvoys", TableType.Country)
        {
            Caption = "Convoys per country";
            Category = "Military";
            Section = 3;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var convoys = f.GetSubnodes(f.Root, true).FindAll(x => f.GetNodeName(x) == "convoy");
            foreach (var convoy in convoys)
            {
                Set(f.GetNodeName(f.GetParent(convoy)), f.GetAttributeValue(convoy, "convoys"));
            }
        }
    }
}
