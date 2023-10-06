using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu3_ProvinceReligion : SingleDatakeyTable
    {
        public eu3_ProvinceReligion() : base("ProvinceReligion", TableType.Province)
        {
            Caption = "Province religion";
            Category = "Religion";
            RequiresSavegame = true;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "religion"));
            }
        }
    }

    [Serializable]
    public class eu3_CountryReligion : MultiValueTable
    {
        public eu3_CountryReligion() : base("CountryReligion", TableType.Country)
        {
            Caption = "Country religions";
            Category = "Religion";
            RequiresSavegame = true;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            SingleDatakeyTable t = SelectTable("ProvinceReligion") as SingleDatakeyTable;
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
    public class eu3_StateReligion : SingleDatakeyTable
    {
        public eu3_StateReligion() : base("StateReligion", TableType.Country)
        {
            Caption = "State religion";
            Category = "Religion";
            RequiresSavegame = true;

            SetColorscale("religions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "religion"));
            }
        }
    }

    [Serializable]
    public class eu3_ReligiousUnity : SingleValueTable
    {
        public eu3_ReligiousUnity() : base("ReligiousUnity", TableType.Country)
        {
            Caption = "Religious unity";
            Category = "Religion";
            RequiresSavegame = true;
            ParsingOrder = 2;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            MultiValueTable t1 = SelectTable("CountryReligion") as MultiValueTable;
            SingleDatakeyTable t2 = SelectTable("StateReligion") as SingleDatakeyTable;
            SingleValueTable t3 = SelectTable("ProvincesCount") as SingleValueTable;
            if (t1 == null || t2 == null || t3 == null) return;

            string[] data2 = t2.GetVector();
            double[] data3 = t3.GetVector();

            for (ushort i = 1; i < data3.Length; i++)
            {
                if (data2[i] == "") continue;
                double data1 = t1.Get(i, data2[i]);
                Set(i, data3[i] == 0 ? 0 : data1 / data3[i]);
            }
        }
    }

    [Serializable]
    public class eu3_CardinalsCount : SingleValueTable
    {
        public eu3_CardinalsCount() : base("CardinalsCount", TableType.Country)
        {
            Caption = "Number of cardinals controlled";
            Category = "Religion";
            Section = 2;
            RequiresSavegame = true;
            AggregateValues = true;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var papacy = f.GetSubnode(f.Root, "papacy");

            foreach (var cardinal in f.GetSubnodes(papacy, "cardinal"))
            {
                Set(f.GetAttributeValue(cardinal, "controller"), 1);
            }
        }
    }

    [Serializable]
    public class eu3_CardinalsSeats : SingleValueTable
    {
        public eu3_CardinalsSeats() : base("CardinalsSeats", TableType.Province)
        {
            Caption = "Cardinal seats";
            Category = "Religion";
            Section = 2;
            RequiresSavegame = true;
            AggregateValues = true;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            var papacy = f.GetSubnode(f.Root, "papacy");

            foreach (var cardinal in f.GetSubnodes(papacy, "cardinal"))
            {
                Set(f.GetAttributeValue(cardinal, "location"), 1);
            }
        }
    }

    [Serializable]
    public class eu3_PapalInfluence : SingleValueTable
    {
        public eu3_PapalInfluence() : base("PapalInfluence", TableType.Country)
        {
            Caption = "Papal influence";
            Category = "Religion";
            Section = 2;
            RequiresSavegame = true;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "papal_influence"));
            }
        }
    }

    [Serializable]
    public class eu3_MissionariesAvailable : SingleValueTable
    {
        public eu3_MissionariesAvailable() : base("MissionariesAvailable", TableType.Country)
        {
            Caption = "Missionaries available";
            Category = "Religion";
            Section = 3;
            RequiresSavegame = true;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "missionaries"));
            }
        }
    }

    [Serializable]
    public class eu3_MissionariesMaintenance : SingleValueTable
    {
        public eu3_MissionariesMaintenance() : base("MissionariesMaintenance", TableType.Country)
        {
            Caption = "Missionaries maintenance";
            Category = "Religion";
            Section = 3;
            RequiresSavegame = true;

            SetColorscale(GetColor("GoldYellow"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "missionary_maintenance"));
            }
        }
    }
}
