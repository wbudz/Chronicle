using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_GovernmentRank : SingleValueTable
    {
        public eu4_GovernmentRank() : base("GovernmentRank", TableType.Country)
        {
            Caption = "Government rank";
            Category = "Domestic";
            Section = 0;

            SetColorscale("DeepRed", "Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "government_rank"));
            }
        }
    }

    [Serializable]
    public class eu4_Government : SingleDatakeyTable
    {
        public eu4_Government() : base("Government", TableType.Country)
        {
            Caption = "Government type";
            Category = "Domestic";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "government"));
            }
        }
    }

    [Serializable]
    public class eu4_LocalAutonomy : SingleValueTable
    {
        public eu4_LocalAutonomy() : base("LocalAutonomy", TableType.Province)
        {
            Caption = "Local autonomy";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "local_autonomy"));
            }
        }
    }

    [Serializable]
    public class eu4_AverageAutonomy : SingleValueTable
    {
        public eu4_AverageAutonomy() : base("AverageAutonomy", TableType.Country)
        {
            Caption = "Average province autonomy";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "average_autonomy"));
            }
        }
    }

    [Serializable]
    public class eu4_LibertyDesire : SingleValueTable
    {
        public eu4_LibertyDesire() : base("LibertyDesire", TableType.Country)
        {
            Caption = "Liberty desire";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "liberty_desire"));
            }
        }
    }

    public class eu4_Unrest : SingleValueTable
    {
        public eu4_Unrest() : base("Unrest", TableType.Province)
        {
            Caption = "Unrest";
            Category = "Domestic";
            Section = 2;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "unrest"));
            }
        }
    }

    public class eu4_LikelyRebels : SingleDatakeyTable
    {
        public eu4_LikelyRebels() : base("LikelyRebels", TableType.Province)
        {
            Caption = "Likely rebels";
            Category = "Domestic";
            Section = 2;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "likely_rebels"));
            }
        }
    }

    public class eu4_Prestige : SingleValueTable
    {
        public eu4_Prestige() : base("Prestige", TableType.Country)
        {
            Caption = "Prestige";
            Category = "Domestic";
            Section = 3;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "prestige"));
            }
        }
    }

    public class eu4_Stability : SingleValueTable
    {
        public eu4_Stability() : base("Stability", TableType.Country)
        {
            Caption = "Stability";
            Category = "Domestic";
            Section = 3;
            ForcedMin = -3;
            ForcedMax = 3;

            SetColorscale("DeepRed", "Yellow", "RichGreen");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "stability"));
            }
        }
    }

    public class eu4_Legitimacy : SingleValueTable
    {
        public eu4_Legitimacy() : base("Legitimacy", TableType.Country)
        {
            Caption = "Legitimacy";
            Category = "Domestic";
            Section = 3;

            SetColorscale("DeepRed", "RichGreen");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "legitimacy"));
            }
        }
    }

    public class eu4_Overextension : SingleValueTable
    {
        public eu4_Overextension() : base("Overextension", TableType.Country)
        {
            Caption = "Overextension";
            Category = "Domestic";
            Section = 3;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "overextension"));
            }
        }
    }

    public class eu4_States : SingleValueTable
    {
        public eu4_States() : base("States", TableType.Province)
        {
            Caption = "States";
            Category = "Domestic";
            Section = 4;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            List<string> states = new List<string>();
            foreach (var n in f["countries"])
            {
                var countryStates = f.GetAttributeValues(n.Value, "state");
                states.AddRange(countryStates);
            }

            Core.Log.Write("Amount of states: " + states.Count);

            for (int i = 0; i < Core.Data.Defs.Provinces.Count; i++)
            {
                for (int j = 0; j < states.Count; j++)
                {
                    if (DynamicDefs.GetTextValue("states", i.ToString()) == states[j])
                        Set((ushort)i, 1);
                }
            }
        }
    }
}
