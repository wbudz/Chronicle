using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    [Serializable]
    public class Mod : IEquatable<Mod>
    {
        public string DirName { get; set; }
        public string Name { get; set; }
        public string ModDir { get; set; }
        public string SaveDir { get; set; }
        public bool Zipped { get; set; }

        public Mod(string dirname)
        {
            DirName = dirname;
        }

        public static Mod CreateMod(Game game, string moddir, string defpath)
        {
            CEParser.TextFile file = new CEParser.TextFile(defpath);
            file.Parse();

            Mod mod = new Mod(Path.GetFileNameWithoutExtension(defpath));
            if (file.HasAnAttributeWithName(file.Root, "name"))
                mod.Name = file.GetAttributeValue(file.Root, "name");
            mod.ModDir = Path.Combine(moddir, mod.Name);

            if (file.HasAnAttributeWithName(file.Root, "path"))
            {
                mod.ModDir = file.GetAttributeValue(file.Root, "path").Replace("/", "\\");
                if (!Path.IsPathRooted(mod.ModDir)) mod.ModDir = Path.Combine(Path.GetDirectoryName(moddir), mod.ModDir);
            }
            if (file.HasAnAttributeWithName(file.Root, "archive"))
            {
                mod.ModDir = file.GetAttributeValue(file.Root, "archive").Replace("/", "\\");
                if (!Path.IsPathRooted(mod.ModDir)) mod.ModDir = Path.Combine(Path.GetDirectoryName(moddir), mod.ModDir);
            }

            mod.Zipped = Path.GetExtension(mod.ModDir).ToLowerInvariant() == ".zip";

            mod.SaveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", game.OfficialName).Replace("Victoria 2", "Victoria II");
            if (file.HasAnAttributeWithName(file.Root, "user_dir"))
            {
                mod.SaveDir = Path.Combine(mod.SaveDir, file.GetAttributeValue(file.Root, "user_dir").Replace("/", "\\"), game.SavegameFolderName);
            }
            else
            {
                mod.SaveDir = Path.Combine(mod.SaveDir, game.SavegameFolderName);
            }

            return mod;
        }

        public bool Equals(Mod other)
        {
            return DirName.Equals(other.DirName);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
