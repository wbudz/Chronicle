using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_Technologies : SingleValueTable
    {
        public vic2_Technologies() : base("Technologies", TableType.Country)
        {
            Caption = "Technologies researched";
            Category = "Technology";
            Section = 0;

            SetColorscale(GetColor("DeepBlue"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetSubnodesCount(f.GetSubnode(n.Value, "technology")));
            }
        }
    }

    [Serializable]
    public class vic2_Railroads : SingleValueTable
    {
        public vic2_Railroads() : base("Railroads", TableType.Province)
        {
            Caption = "Railroads level";
            Category = "Technology";
            Section = 0;

            SetColorscale(GetColor("Black"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "railroad"), 0));
            }
        }
    }
}
