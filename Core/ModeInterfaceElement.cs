using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public class ModeInterfaceElement
    {
        public string Name;
        public string Caption;
        public string Category;
        public int Section;
        public TableType Type;
        public bool Special;
        public bool Multivalue;
        public bool Multi;

        public ModeInterfaceElement(Table table)
        {
            Name = table.Name;
            Caption = table.Caption;
            Category = table.Category;
            Section = table.Section;
            Type = table.Type;
            Multi = table is IMultiTable;
            Multivalue = table is MultiValueTable;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
