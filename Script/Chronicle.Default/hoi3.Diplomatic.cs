using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class hoi3_DiplomaticInfluence : SingleValueTable
    {
        public hoi3_DiplomaticInfluence() : base("DiplomaticInfluence", TableType.Country)
        {
            Caption = "Diplomatic influence";
            Category = "Diplomatic";
            Section = 0;

            SetColorscale(GetColor("Violet"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "diplo_influence"));
            }
        }
    }

    [Serializable]
    public class hoi3_Faction : SingleDatakeyTable
    {
        public hoi3_Faction() : base("Faction", TableType.Country)
        {
            Caption = "Faction membership";
            Category = "Diplomatic";
            Section = 1;

            SetColorscale("factions");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var faction in f.GetSubnodes(f.Root, "faction", "*"))
            {
                var countries = f.GetAttributeValues(faction, "country");
                for (int j = 0; j < countries.Length; j++)
                {
                    Set(TagToIndex(countries[j]), f.GetNodeName(faction));
                }
            }
        }
    }


}
