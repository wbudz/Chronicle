using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    [Serializable]
    public abstract class DynamicDefs
    {
        public abstract bool HasDatakeyColors(string identifier);

        public abstract Dictionary<string, int> GetDatakeysColors(string identifier);

        public abstract Dictionary<string, string> GetTextDict(string identifier);

        public abstract Dictionary<string, double> GetNumDict(string identifier);

        public abstract string GetTextValue(string identifier, string key);

        public abstract double GetNumValue(string identifier, string key);

        public abstract bool IsOnList(string identifier, string key);

        public DynamicDefs()
        {
        }
    }
}
