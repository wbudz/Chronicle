using System;

namespace Chronicle
{
    [Serializable]
    public class eu3_Prestige : SingleValueTable
    {
        public eu3_Prestige() : base("Prestige", TableType.Country)
        {
            Caption = "Prestige";
            Category = "Domestic";
            Section = 0;
            ForcedMin = -100;
            ForcedMax = 100;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, "prestige");
            }
        }
    }

    [Serializable]
    public class eu3_Stability : SingleValueTable
    {
        public eu3_Stability() : base("Stability", TableType.Country)
        {
            Caption = "Stability";
            Category = "Domestic";
            Section = 0;
            ForcedMin = -3;
            ForcedMax = 3;

            SetColorscale(GetColor("DeepRed"), GetColor("Yellow"), GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, "stability");
            }
        }
    }

    [Serializable]
    public class eu3_Badboy : SingleValueTable
    {
        public eu3_Badboy() : base("Badboy", TableType.Country)
        {
            Caption = "Badboy";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("Yellow"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, "badboy");
            }
        }
    }

}
