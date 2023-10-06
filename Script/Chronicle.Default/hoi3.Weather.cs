using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_Temperature : SingleValueTable
    {
        public hoi3_Temperature() : base("Temperature", TableType.Province)
        {
            Caption = "Temperature";
            Category = "Weather";
            Section = 0;

            SetColorscale(CreateColor(50, 50, 220), CreateColor(0, 220, 0), CreateColor(255, 20, 20), 1,
                CreateColor(160, 160, 250), CreateColor(0, 0, 220), CreateColor(120, 110, 220), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "weather"), "temperature"));
            }
        }
    }

    [Serializable]
    public class hoi3_Precipitation : SingleValueTable
    {
        public hoi3_Precipitation() : base("Precipitation", TableType.Province)
        {
            Caption = "Precipitation";
            Category = "Weather";
            Section = 0;

            SetColorscale(CreateColor(130, 140, 100), CreateColor(100, 170, 190), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "weather"), "precipitation"));
            }
        }
    }
}
