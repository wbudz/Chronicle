using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class eu4_TechnologyGroup : SingleDatakeyTable
    {
        public eu4_TechnologyGroup() : base("TechnologyGroup", TableType.Country)
        {
            Caption = "Technology group";
            Category = "Technology";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "technology_group"));
            }
        }
    }

    [Serializable]
    public class eu4_UnitType : SingleDatakeyTable
    {
        public eu4_UnitType() : base("UnitType", TableType.Country)
        {
            Caption = "Units type";
            Category = "Technology";
            Section = 0;

            SetColorscale();
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "unit_type"));
            }
        }
    }

    [Serializable]
    public class eu4_InstitutionsAdopted : SingleValueTable
    {
        public eu4_InstitutionsAdopted() : base("InstitutionsAdopted", TableType.Country)
        {
            Caption = "Institutions adopted";
            Category = "Technology";
            Section = 1;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetEntries(f.GetSubnode(n.Value, "institutions")).Count(x => x == "1").ToString());
            }
        }
    }

    [Serializable]
    public class eu4_ProvinceInstitutionSpread : MultiValueTable
    {
        public eu4_ProvinceInstitutionSpread() : base("ProvinceInstitutionSpread", TableType.Province)
        {
            Caption = "Province institution spread";
            Category = "Technology";
            Section = 1;

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["provinces"])
            {
                var entries = f.GetEntries(f.GetSubnode(n.Value, "institutions"));
                for (int i = 0; i < entries.Length; i++)
                {
                    Set(n.Key, DynamicDefs.GetTextValue("institutions", i.ToString()), entries[i]);
                }
            }
        }
    }

    [Serializable]
    public class eu4_AdministrativeTechnologyLevel : SingleValueTable
    {
        public eu4_AdministrativeTechnologyLevel() : base("AdministrativeTechnologyLevel", TableType.Country)
        {
            Caption = "Administrative technology level";
            Category = "Technology";
            Section = 2;
            ForcedMin = 0;
            ForcedMax = DynamicDefs.GetNumValue("technologiescount", "ADM");

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "technology"), "adm_tech"));
            }
        }
    }

    [Serializable]
    public class eu4_DiplomaticTechnologyLevel : SingleValueTable
    {
        public eu4_DiplomaticTechnologyLevel() : base("DiplomaticTechnologyLevel", TableType.Country)
        {
            Caption = "Diplomatic technology level";
            Category = "Technology";
            Section = 2;
            ForcedMin = 0;
            ForcedMax = DynamicDefs.GetNumValue("technologiescount", "DIP");

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "technology"), "dip_tech"));
            }
        }
    }

    [Serializable]
    public class eu4_MilitaryTechnologyLevel : SingleValueTable
    {
        public eu4_MilitaryTechnologyLevel() : base("MilitaryTechnologyLevel", TableType.Country)
        {
            Caption = "Military technology level";
            Category = "Technology";
            Section = 2;
            ForcedMin = 0;
            ForcedMax = DynamicDefs.GetNumValue("technologiescount", "MIL");

            SetColorscale("DeepBlue");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(f.GetSubnode(n.Value, "technology"), "mil_tech"));
            }
        }
    }


}
