using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_ProvinceTechnology : SingleValueTable
    {
        public ck2_ProvinceTechnology() : base("ProvinceTechnology", TableType.Province)
        {
            Caption = "Province technology level";
            Category = "Technology";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                string[] levels = f.GetEntries(f.GetSubnode(n.Value, "technology", "tech_levels"));
                double sum = 0;
                for (int i = 0; i < levels.Length; i++)
                {
                    sum += Ext.ParseDouble(levels[i]);
                }
                Set(n.Key, sum);
            }
        }
    }
}
