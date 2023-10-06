using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class eu4_Score : SingleValueTable
    {
        public eu4_Score() : base("Score", TableType.Country)
        {
            Caption = "Score";
            Category = "Score";
            Section = 0;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "score"));
            }
        }
    }

    [Serializable]
    public class eu4_AdministrativeScore : SingleValueTable
    {
        public eu4_AdministrativeScore() : base("AdministrativeScore", TableType.Country)
        {
            Caption = "Administrative score";
            Category = "Score";
            Section = 0;

            SetColorscale("Violet", "Empty");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rating"), 0));
            }
        }
    }

    [Serializable]
    public class eu4_DiplomaticScore : SingleValueTable
    {
        public eu4_DiplomaticScore() : base("DiplomaticScore", TableType.Country)
        {
            Caption = "Diplomatic score";
            Category = "Score";
            Section = 0;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rating"), 1));
            }
        }
    }

    [Serializable]
    public class eu4_MilitaryScore : SingleValueTable
    {
        public eu4_MilitaryScore() : base("MilitaryScore", TableType.Country)
        {
            Caption = "Military score";
            Category = "Score";
            Section = 0;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rating"), 2));
            }
        }
    }

    [Serializable]
    public class eu4_AdministrativeScoreRank : SingleValueTable
    {
        public eu4_AdministrativeScoreRank() : base("AdministrativeScoreRank", TableType.Country)
        {
            Caption = "Administrative score rank";
            Category = "Score";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rank"), 0));
            }
        }
    }

    [Serializable]
    public class eu4_DiplomaticScoreRank : SingleValueTable
    {
        public eu4_DiplomaticScoreRank() : base("DiplomaticScoreRank", TableType.Country)
        {
            Caption = "Diplomatic score rank";
            Category = "Score";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rank"), 1));
            }
        }
    }

    [Serializable]
    public class eu4_MilitaryScoreRank : SingleValueTable
    {
        public eu4_MilitaryScoreRank() : base("MilitaryScoreRank", TableType.Country)
        {
            Caption = "Military score rank";
            Category = "Score";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "score_rank"), 2));
            }
        }
    }


}
