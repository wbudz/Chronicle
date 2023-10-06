using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class eu4_ProvinceBuildings : SingleValueTable
    {
        public eu4_ProvinceBuildings() : base("ProvinceBuildings", TableType.Province)
        {
            Caption = "Province buildings";
            Category = "Development";
            Section = 0;
            AggregateValues = true;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            var buildings = DynamicDefs.GetNumDict("buildings");
            foreach (var n in f["provinces"])
            {
                for (int i = 0; i < buildings.Count; i++)
                {
                    if (f.HasAnAttribute(n.Value, buildings.Keys.ElementAt(i), "yes"))
                        Set(n.Key, buildings[buildings.Keys.ElementAt(i)]);
                }
            }
        }
    }

    [Serializable]
    public class eu4_ProvinceManufactoryBuildings : SingleValueTable
    {
        public eu4_ProvinceManufactoryBuildings() : base("ProvinceManufactoryBuildings", TableType.Province)
        {
            Caption = "Province manufactories";
            Category = "Development";
            Section = 0;
            AggregateValues = true;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            var buildings = DynamicDefs.GetNumDict("manufactories");
            foreach (var n in f["provinces"])
            {
                for (int i = 0; i < buildings.Count; i++)
                {
                    if (f.HasAnAttribute(n.Value, buildings.Keys.ElementAt(i), "yes"))
                        Set(n.Key, buildings[buildings.Keys.ElementAt(i)]);
                }
            }
        }
    }

    
}
