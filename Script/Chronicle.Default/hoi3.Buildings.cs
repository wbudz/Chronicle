using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_ProvinceICCurrent : SingleValueTable
    {
        public hoi3_ProvinceICCurrent() : base("ProvinceICCurrent", TableType.Province)
        {
            Caption = "Province base IC (current)";
            Category = "Buildings";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "industry"), 0));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceICMaximum : SingleValueTable
    {
        public hoi3_ProvinceICMaximum() : base("ProvinceICMaximum", TableType.Province)
        {
            Caption = "Province base IC (maximum)";
            Category = "Buildings";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "industry"), 1));
            }
        }
    }

    [Serializable]
    public class hoi3_CountryICCurrent : SingleValueTable
    {
        public hoi3_CountryICCurrent() : base("CountryICCurrent", TableType.Country)
        {
            Caption = "Country base IC (current)";
            Category = "Buildings";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceICCurrent") as SingleValueTable;
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
    public class hoi3_CountryICMaximum : SingleValueTable
    {
        public hoi3_CountryICMaximum() : base("CountryICMaximum", TableType.Country)
        {
            Caption = "Country base IC (maximum)";
            Category = "Buildings";
            Section = 0;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            SingleValueTable t = SelectTable("ProvinceICMaximum") as SingleValueTable;
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
    public class hoi3_ProvinceInfrastructureCurrent : SingleValueTable
    {
        public hoi3_ProvinceInfrastructureCurrent() : base("ProvinceInfrastructureCurrent", TableType.Province)
        {
            Caption = "Province infrastructure (current)";
            Category = "Buildings";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "infra"), 0));
            }
        }
    }

    [Serializable]
    public class hoi3_ProvinceInfrastructureMaximum : SingleValueTable
    {
        public hoi3_ProvinceInfrastructureMaximum() : base("ProvinceInfrastructureMaximum", TableType.Province)
        {
            Caption = "Province infrastructure (maximum)";
            Category = "Buildings";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "infra"), 1));
            }
        }
    }
}