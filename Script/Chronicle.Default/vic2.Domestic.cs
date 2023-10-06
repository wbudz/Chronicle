using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class vic2_LifeRating : SingleValueTable
    {
        public vic2_LifeRating() : base("LifeRating", TableType.Province)
        {
            Caption = "Life rating";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "life_rating"));
            }
        }
    }

    [Serializable]
    public class vic2_Crime : SingleValueTable
    {
        public vic2_Crime() : base("Crime", TableType.Province)
        {
            Caption = "Crime";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "crime"));
            }
        }
    }

    [Serializable]
    public class vic2_Nationalism : SingleValueTable
    {
        public vic2_Nationalism() : base("Nationalism", TableType.Province)
        {
            Caption = "Nationalism";
            Category = "Domestic";
            Section = 0;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "nationalism"));
            }
        }
    }

    [Serializable]
    public class vic2_Revanchism : SingleValueTable
    {
        public vic2_Revanchism() : base("Revanchism", TableType.Country)
        {
            Caption = "Revanchism";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "revanchism"));
            }
        }
    }

    [Serializable]
    public class vic2_Badboy : SingleValueTable
    {
        public vic2_Badboy() : base("Badboy", TableType.Country)
        {
            Caption = "Badboy";
            Category = "Domestic";
            Section = 1;

            SetColorscale(GetColor("RichGreen"), GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "badboy"));
            }
        }
    }

    [Serializable]
    public class vic2_GovernmentType : SingleDatakeyTable
    {
        public vic2_GovernmentType() : base("GovernmentType", TableType.Country)
        {
            Caption = "Type of government";
            Category = "Domestic";
            Section = 2;

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
    public class vic2_Plurality : SingleValueTable
    {
        public vic2_Plurality() : base("Plurality", TableType.Country)
        {
            Caption = "Plurality";
            Category = "Domestic";
            Section = 2;

            SetColorscale(GetColor("DeepRed"), GetColor("RichGreen"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "plurality"));
            }
        }
    }

    [Serializable]
    public class vic2_PoliticalReforms : SingleValueTable
    {
        public vic2_PoliticalReforms() : base("PoliticalReforms", TableType.Country)
        {
            Caption = "Political reforms";
            Category = "Domestic";
            Section = 3;

            SetColorscale(GetColor("DeepBlue"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var reforms = DynamicDefs.GetNumDict("political_reforms");
                foreach (var r in reforms.Keys.ToArray())
                {
                    string reformName = r.Substring(0, r.IndexOf('|'));
                    string reformLevel = r.Substring(r.IndexOf('|') + 1);

                    if (f.HasAnAttribute(n.Value, reformName, reformLevel))
                        Set(n.Key, reforms[r]);
                }
            }
        }
    }


    [Serializable]
    public class vic2_SocialReforms : SingleValueTable
    {
        public vic2_SocialReforms() : base("SocialReforms", TableType.Country)
        {
            Caption = "Social reforms";
            Category = "Domestic";
            Section = 3;

            SetColorscale(GetColor("DeepBlue"));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var reforms = DynamicDefs.GetNumDict("social_reforms");
                foreach (var r in reforms.Keys.ToArray())
                {
                    string reformName = r.Substring(0, r.IndexOf('|'));
                    string reformLevel = r.Substring(r.IndexOf('|') + 1);

                    if (f.HasAnAttribute(n.Value, reformName, reformLevel))
                        Set(n.Key, reforms[r]);
                }
            }
        }
    }

    [Serializable]
    public class vic2_Prestige : SingleValueTable
    {
        public vic2_Prestige() : base("Prestige", TableType.Country)
        {
            Caption = "Prestige";
            Category = "Domestic";
            Section = 4;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "prestige"));
            }
        }
    }

    [Serializable]
    public class vic2_DiplomaticPoints : SingleValueTable
    {
        public vic2_DiplomaticPoints() : base("DiplomaticPoints", TableType.Country)
        {
            Caption = "Diplomatic points";
            Category = "Domestic";
            Section = 4;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "diplomatic_points"));
            }
        }
    }

    [Serializable]
    public class vic2_CountryLeadership : SingleValueTable
    {
        public vic2_CountryLeadership() : base("CountryLeadership", TableType.Country)
        {
            Caption = "Leadership points";
            Category = "Domestic";
            Section = 4;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "leadership"));
            }
        }
    }

    [Serializable]
    public class vic2_ProvinceNationalFocus : SingleDatakeyTable
    {
        public vic2_ProvinceNationalFocus() : base("ProvinceNationalFocus", TableType.Province)
        {
            Caption = "National focus in provinces";
            Category = "Domestic";
            Section = 5;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var foci = f.GetAttributes(f.GetSubnode(n.Value, "national_focus"));
                for (int i = 0; i < foci.Length; i++)
                {
                    Set(UInt16.Parse(foci[i].Key), foci[i].Value);
                }
            }
        }
    }

    [Serializable]
    public class vic2_UpperhouseComposition : MultiValueTable
    {
        public vic2_UpperhouseComposition() : base("UpperhouseComposition", TableType.Country)
        {
            Caption = "Ideology support (upper house)";
            Category = "Domestic";
            Section = 6;

            SetColorscale("ideologies");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var ideologies = f.GetAttributes(f.GetSubnode(n.Value, "upper_house"));
                for (int i = 0; i < ideologies.Length; i++)
                {
                    Set(n.Key, ideologies[i].Key, ideologies[i].Value);
                }
            }
        }
    }
}
