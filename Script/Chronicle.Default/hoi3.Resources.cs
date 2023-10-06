using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_Money : SingleValueTable
    {
        public hoi3_Money() : base("Money", TableType.Country)
        {
            Caption = "Money";
            Category = "Resources";
            Section = 0;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "money"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceEnergyCurrent : SingleValueTable
    {
        public hoi3_ProvinceEnergyCurrent() : base("ProvinceEnergyCurrent", TableType.Province)
        {
            Caption = "Province energy production (current)";
            Category = "Resources";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "current_producing"), "energy"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceEnergyMax : SingleValueTable
    {
        public hoi3_ProvinceEnergyMax() : base("ProvinceEnergyMax", TableType.Province)
        {
            Caption = "Province energy production (maximum)";
            Category = "Resources";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "max_producing"), "energy"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryEnergyCurrent : SingleValueTable
    {
        public hoi3_CountryEnergyCurrent() : base("CountryEnergyCurrent", TableType.Country)
        {
            Caption = "Country energy production (current)";
            Category = "Resources";
            Section = 1;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceEnergyCurrent") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_CountryEnergyMaximum : SingleValueTable
    {
        public hoi3_CountryEnergyMaximum() : base("CountryEnergyMaximum", TableType.Country)
        {
            Caption = "Country energy production (maximum)";
            Category = "Resources";
            Section = 1;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceEnergyMaximum") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_EnergyPool : SingleValueTable
    {
        public hoi3_EnergyPool() : base("EnergyPool", TableType.Country)
        {
            Caption = "Energy stockpile";
            Category = "Resources";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "energy"));
            }
        }
    }


    [Serializable]
    public class hoi3_ProvinceMetalCurrent : SingleValueTable
    {
        public hoi3_ProvinceMetalCurrent() : base("ProvinceMetalCurrent", TableType.Province)
        {
            Caption = "Province metal production (current)";
            Category = "Resources";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "current_producing"), "metal"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceMetalMax : SingleValueTable
    {
        public hoi3_ProvinceMetalMax() : base("ProvinceMetalMax", TableType.Province)
        {
            Caption = "Province metal production (maximum)";
            Category = "Resources";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "max_producing"), "metal"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryMetalCurrent : SingleValueTable
    {
        public hoi3_CountryMetalCurrent() : base("CountryMetalCurrent", TableType.Country)
        {
            Caption = "Country metal production (current)";
            Category = "Resources";
            Section = 2;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceMetalCurrent") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_CountryMetalMaximum : SingleValueTable
    {
        public hoi3_CountryMetalMaximum() : base("CountryMetalMaximum", TableType.Country)
        {
            Caption = "Country metal production (maximum)";
            Category = "Resources";
            Section = 2;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceMetalMaximum") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_MetalPool : SingleValueTable
    {
        public hoi3_MetalPool() : base("MetalPool", TableType.Country)
        {
            Caption = "Metal stockpile";
            Category = "Resources";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "metal"));
            }
        }
    }


    [Serializable]
    public class hoi3_ProvinceRareMaterialsCurrent : SingleValueTable
    {
        public hoi3_ProvinceRareMaterialsCurrent() : base("ProvinceRareMaterialsCurrent", TableType.Province)
        {
            Caption = "Province rare materials production (current)";
            Category = "Resources";
            Section = 3;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "current_producing"), "rare_materials"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceRareMaterialsMax : SingleValueTable
    {
        public hoi3_ProvinceRareMaterialsMax() : base("ProvinceRareMaterialsMax", TableType.Province)
        {
            Caption = "Province rare materials production (maximum)";
            Category = "Resources";
            Section = 3;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "max_producing"), "rare_materials"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryRareMaterialsCurrent : SingleValueTable
    {
        public hoi3_CountryRareMaterialsCurrent() : base("CountryRareMaterialsCurrent", TableType.Country)
        {
            Caption = "Country rare materials production (current)";
            Category = "Resources";
            Section = 3;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceRareMaterialsCurrent") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_CountryRareMaterialsMaximum : SingleValueTable
    {
        public hoi3_CountryRareMaterialsMaximum() : base("CountryRareMaterialsMaximum", TableType.Country)
        {
            Caption = "Country rare materials production (maximum)";
            Category = "Resources";
            ParsingOrder = 1;
            Section = 3;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceRareMaterialsMaximum") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_RareMaterialsPool : SingleValueTable
    {
        public hoi3_RareMaterialsPool() : base("RareMaterialsPool", TableType.Country)
        {
            Caption = "Rare materials stockpile";
            Category = "Resources";
            Section = 3;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "rare_materials"));
            }
        }
    }



    [Serializable]
    public class hoi3_ProvinceCrudeOilCurrent : SingleValueTable
    {
        public hoi3_ProvinceCrudeOilCurrent() : base("ProvinceCrudeOilCurrent", TableType.Province)
        {
            Caption = "Province crude oil production (current)";
            Category = "Resources";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "current_producing"), "crude_oil"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceCrudeOilMax : SingleValueTable
    {
        public hoi3_ProvinceCrudeOilMax() : base("ProvinceCrudeOilMax", TableType.Province)
        {
            Caption = "Province crude oil production (maximum)";
            Category = "Resources";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "max_producing"), "crude_oil"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryCrudeOilCurrent : SingleValueTable
    {
        public hoi3_CountryCrudeOilCurrent() : base("CountryCrudeOilCurrent", TableType.Country)
        {
            Caption = "Country crude oil production (current)";
            Category = "Resources";
            Section = 4;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceCrudeOilCurrent") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_CountryCrudeOilMaximum : SingleValueTable
    {
        public hoi3_CountryCrudeOilMaximum() : base("CountryCrudeOilMaximum", TableType.Country)
        {
            Caption = "Country crude oil production (maximum)";
            Category = "Resources";
            Section = 4;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceCrudeOilMaximum") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_CrudeOilPool : SingleValueTable
    {
        public hoi3_CrudeOilPool() : base("CrudeOilPool", TableType.Country)
        {
            Caption = "Crude oil stockpile";
            Category = "Resources";
            Section = 4;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "crude_oil"));
            }
        }
    }

    [Serializable]
    public class hoi3_SuppliesPool : SingleValueTable
    {
        public hoi3_SuppliesPool() : base("SuppliesPool", TableType.Country)
        {
            Caption = "Supplies stockpile";
            Category = "Resources";
            Section = 5;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "home"), "supplies"));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceManpower : SingleValueTable
    {
        public hoi3_ProvinceManpower() : base("ProvinceManpower", TableType.Province)
        {
            Caption = "Province manpower inflow";
            Category = "Resources";
            Section = 6;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "manpower"));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryManpower : SingleValueTable
    {
        public hoi3_CountryManpower() : base("CountryManpower", TableType.Country)
        {
            Caption = "Country manpower inflow";
            Category = "Resources";
            Section = 6;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceManpower") as SingleValueTable;
            ushort[] master = GetProvinceMasters();
            if (t == null) return;

            double[] data = t.GetVector();
            for (int i = 0; i < data.Length; i++)
            {
                Set(master[i], data[i]);
            }
        }
    }

    [Serializable]
    public class hoi3_ManpowerPool : SingleValueTable
    {
        public hoi3_ManpowerPool() : base("ManpowerPool", TableType.Country)
        {
            Caption = "Manpower pool";
            Category = "Resources";
            Section = 6;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "manpower"));
            }
        }
    }

}
