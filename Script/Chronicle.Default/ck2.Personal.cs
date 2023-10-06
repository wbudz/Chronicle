using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_Dynasties : SingleDatakeyTable
    {
        public ck2_Dynasties() : base("Dynasties", TableType.Country)
        {
            Caption = "Ruling dynasties";
            Category = "Personal";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                var dynastyid = f.GetAttributeValue(n.Value, "dnt");
                string surname = DynamicDefs.GetTextValue("dynasties", dynastyid);
                if (surname == "")
                {
                    var dynastynode = f.GetSubnode(f.Root, "dynasties", dynastyid);
                    surname = f.GetAttributeValue(dynastynode, "name");
                }
                Set(n.Key, surname);
            }
        }
    }

    [Serializable]
    public class ck2_Prestige : SingleValueTable
    {
        public ck2_Prestige() : base("Prestige", TableType.Country)
        {
            Caption = "Ruler's prestige";
            Category = "Personal";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "prs"));
            }
        }
    }

    [Serializable]
    public class ck2_Piety : SingleValueTable
    {
        public ck2_Piety() : base("Piety", TableType.Country)
        {
            Caption = "Ruler's piety";
            Category = "Personal";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "piety"));
            }
        }
    }

    [Serializable]
    public class ck2_DiplomacyAttribute : SingleValueTable
    {
        public ck2_DiplomacyAttribute() : base("DiplomacyAttribute", TableType.Country)
        {
            Caption = "Ruler's diplomacy skill";
            Category = "Personal";
            Section = 2;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "att"), 0));
            }
        }
    }

    [Serializable]
    public class ck2_MartialAttribute : SingleValueTable
    {
        public ck2_MartialAttribute() : base("MartialAttribute", TableType.Country)
        {
            Caption = "Ruler's martial skill";
            Category = "Personal";
            Section = 2;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "att"), 1));
            }
        }
    }

    [Serializable]
    public class ck2_StewardshipAttribute : SingleValueTable
    {
        public ck2_StewardshipAttribute() : base("StewardshipAttribute", TableType.Country)
        {
            Caption = "Ruler's stewardship skill";
            Category = "Personal";
            Section = 2;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "att"), 2));
            }
        }
    }

    [Serializable]
    public class ck2_IntrigueAttribute : SingleValueTable
    {
        public ck2_IntrigueAttribute() : base("IntrigueAttribute", TableType.Country)
        {
            Caption = "Ruler's intrigue skill";
            Category = "Personal";
            Section = 2;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "att"), 3));
            }
        }
    }

    [Serializable]
    public class ck2_LearningAttribute : SingleValueTable
    {
        public ck2_LearningAttribute() : base("LearningAttribute", TableType.Country)
        {
            Caption = "Ruler's learning skill";
            Category = "Personal";
            Section = 2;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetEntry(f.GetSubnode(n.Value, "att"), 4));
            }
        }
    }

    [Serializable]
    public class ck2_Health : SingleValueTable
    {
        public ck2_Health() : base("Health", TableType.Country)
        {
            Caption = "Ruler's health";
            Category = "Personal";
            Section = 3;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "health"));
            }
        }
    }

    [Serializable]
    public class ck2_Fertility : SingleValueTable
    {
        public ck2_Fertility() : base("Fertility", TableType.Country)
        {
            Caption = "Ruler's fertility";
            Category = "Personal";
            Section = 3;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "fer"));
            }
        }
    }

    [Serializable]
    public class ck2_Age : SingleValueTable
    {
        public ck2_Age() : base("Age", TableType.Country)
        {
            Caption = "Ruler's age";
            Category = "Personal";
            Section = 3;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                string birth = f.GetAttributeValue(n.Value, "b_d");
                int birth_year;
                if (!birth.Contains('.')) continue;
                Int32.TryParse(birth.Substring(0, birth.IndexOf('.')), out birth_year);
                if (birth_year > 0)
                    Set(n.Key, Timepoint.Year - birth_year);
            }
        }
    }
}
