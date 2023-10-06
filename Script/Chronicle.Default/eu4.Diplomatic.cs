using System;
using System.Collections.Generic;

namespace Chronicle
{
    [Serializable]
    public class eu4_Rivals : MultiDatakeyTable
    {
        public eu4_Rivals() : base("Rivals", TableType.Country)
        {
            Caption = "Rivals";
            Category = "Diplomatic";
            Section = 0;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;
            MarkSelectedCountry = true;

            SetColorscale();
            AddColor("rival", CreateColor(220, 0, 0));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var rivals = f.GetSubnodes(n.Value, "rival");
                foreach (var r in rivals)
                {
                    Set(n.Key, "rival", TagToIndex(f.GetAttributeValue(r, "country")));
                }
            }
        }
    }

    [Serializable]
    public class eu4_Enemies : MultiDatakeyTable
    {
        public eu4_Enemies() : base("Enemies", TableType.Country)
        {
            Caption = "Enemies";
            Category = "Diplomatic";
            Section = 0;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;
            MarkSelectedCountry = true;

            SetColorscale();
            AddColor("enemy", CreateColor(220, 0, 0));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var enemies = f.GetAttributeValues(n.Value, "enemy");
                foreach (var e in enemies)
                {
                    Set(n.Key, "enemy", TagToIndex(e));
                }
            }
        }
    }

    [Serializable]
    public class eu4_PowerProjection : SingleValueTable
    {
        public eu4_PowerProjection() : base("PowerProjection", TableType.Country)
        {
            Caption = "Power projection";
            Category = "Diplomatic";
            Section = 1;

            SetColorscale("Violet");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "current_power_projection"));
            }
        }
    }

    [Serializable]
    public class eu4_TotalWarWorth : SingleValueTable
    {
        public eu4_TotalWarWorth() : base("TotalWarWorth", TableType.Country)
        {
            Caption = "Total war worth";
            Category = "Diplomatic";
            Section = 1;

            SetColorscale("DeepRed");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, f.GetAttributeValue(n.Value, "total_war_worth"));
            }
        }
    }

    [Serializable]
    public class eu4_Wars : MultiDatakeyTable
    {
        public eu4_Wars() : base("Wars", TableType.Country)
        {
            Caption = "Wars";
            Category = "Diplomatic";
            Section = 2;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;
            MarkSelectedCountry = true;

            SetColorscale();
            AddColor("war", CreateColor(220, 0, 0));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var wars = f.GetEntries(f.GetSubnode(n.Value, "current_at_war_with"));
                foreach (var w in wars)
                {
                    Set(n.Key, "war", TagToIndex(w));
                }
            }
        }
    }

    [Serializable]
    public class eu4_Allies : MultiDatakeyTable
    {
        public eu4_Allies() : base("Allies", TableType.Country)
        {
            Caption = "Allies";
            Category = "Diplomatic";
            Section = 2;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;
            MarkSelectedCountry = true;

            SetColorscale();
            AddColor("ally", CreateColor(100, 100, 220));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var allies = f.GetEntries(f.GetSubnode(n.Value, "allies"));
                foreach (var a in allies)
                {
                    Set(n.Key, "ally", TagToIndex(a));
                }
            }
        }
    }

    [Serializable]
    public class eu4_Friends : MultiDatakeyTable
    {
        public eu4_Friends() : base("Friends", TableType.Country)
        {
            Caption = "Friends";
            Category = "Diplomatic";
            Section = 2;
            ValueEncoding = ValueEncoding.Country;
            DisplayOnlyForSelectedCountry = true;
            MarkSelectedCountry = true;

            SetColorscale();
            AddColor("friend", CreateColor(100, 220, 100));
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                var friends = f.GetEntries(f.GetSubnode(n.Value, "friends"));
                foreach (var r in friends)
                {
                    Set(n.Key, "friend", TagToIndex(r));
                }
            }
        }
    }

}
