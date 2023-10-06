using System;
using System.Linq;

namespace Chronicle
{
    [Serializable]
    public class ck2_TitleHolders : SingleDatakeyTable
    {
        public ck2_TitleHolders() : base("TitleHolders", TableType.Country)
        {
            Caption = "Title holders";
            Category = "Political";
            Section = 0;
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["titleholders"])
            {
                var name = f.GetAttributeValue(n.Value, "birth_name");
                var dynastyid = f.GetAttributeValue(n.Value, "dnt");
                string surname = DynamicDefs.GetTextValue("dynasties", dynastyid);
                if (surname == "")
                {
                    var dynastynode = f.GetSubnode(f.Root, "dynasties", dynastyid);
                    surname = f.GetAttributeValue(dynastynode, "name");
                }
                Set(n.Key, name + (surname == "" ? "" : " " + surname));
            }
        }
    }

    [Serializable]
    public class ck2_Political : MultiDatakeyTable
    {
        public ck2_Political() : base("Political", TableType.Province)
        {
            Caption = "Political";
            Category = "Political";
            DisplayOnlyForSelectedCountry = false;
            ColorByValue = true;
            ValueEncoding = ValueEncoding.Country;

            SetColorscale("countries");
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["baronies"])
            {
                // Find barony title
                CEParser.Node ownertitle = f.GetSubnode(f.Root, "title", f.GetNodeName(n.Value));
                // Traverse through liege trees
                if (ownertitle == null) continue;
                while (f.HasAContainer(ownertitle, "liege"))
                {
                    string t = f.GetAttributeValue(f.GetSubnode(ownertitle, "liege"), "title");
                    ownertitle = f.GetSubnode(f.Root, "title", t);
                }

                Set(n.Key, "owner", TagToIndex(f.GetNodeName(ownertitle)));

                // Find barony title
                string controller = f.GetAttributeValue(n.Value, "controller");
                CEParser.Node controllertitle;
                if (controller != "")
                {
                    controllertitle = f.GetSubnode(f.Root, "title", controller);
                }
                else
                {
                    controllertitle = f.GetSubnode(f.Root, "title", f.GetNodeName(n.Value));
                }
                // Traverse through liege trees
                if (controllertitle != null)
                {
                    while (f.HasAContainer(controllertitle, "liege"))
                    {
                        string t = f.GetAttributeValue(f.GetSubnode(controllertitle, "liege"), "title");
                        controllertitle = f.GetSubnode(f.Root, "title", t);
                    }
                }
                Set(n.Key, "controller", TagToIndex(f.GetNodeName(controllertitle)));
            }
        }
    }

    [Serializable]
    public class ck2_ProvincesCount : SingleValueTable
    {
        public ck2_ProvincesCount() : base("ProvincesCount", TableType.Country)
        {
            Caption = "Number of provinces";
            Category = "Political";
            Section = 1;
            ParsingOrder = 1;
            AggregateValues = true;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            ushort[] masters = GetProvinceMasters();

            for (int i = 0; i < masters.Length; i++)
            {
                Set(masters[i], 1);
            }
        }
    }

    [Serializable]
    public class ck2_RebelTerritory : SingleValueTable
    {
        public ck2_RebelTerritory() : base("RebelTerritory", TableType.Country)
        {
            Caption = "Rebel territory";
            Category = "Political";
            Section = 1;

            SetColorscale(GetColor("DeepRed"), 1);
        }

        public override void Parse(CEParser.File f)
        {
            foreach (var n in f["countries"])
            {
                Set(n.Key, (f.HasAnAttribute(n.Value, "rebels", "yes") || f.HasAnAttribute(n.Value, "major_revolt", "yes")) ? 1 : 0);
            }
        }
    }

}
