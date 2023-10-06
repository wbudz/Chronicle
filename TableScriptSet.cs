using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    public class TableScriptSet
    {
        public string Name { get; set; }    
        public string Path { get; set; } 
        public int PreferredOrder { get; set; }
        public bool IsEnabled { get; set; }
        public List<TableScript> Scripts { get; set; }
        public List<String> Files { get; set; }

        public TableScriptSet(string name, string path)
        {
            Name = name;
            Path = path;
            IsEnabled = true;
            Scripts = new List<TableScript>();
            Files = new List<string>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
