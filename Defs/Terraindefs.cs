using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chronicle
{
    public class Terraindefs
    {
        Terrain[] terrainTypes;
        Dictionary<byte, byte> palToTerr;

        public int Count
        {
            get
            {
                if (terrainTypes == null) return 0;
                return terrainTypes.Length;
            }
        }

        public Terraindefs(InstalledGame game, string[] paths)
        {
            List<string> consumedFilenames = new List<string>();
            Core.Log.Write("Reading terrain definitions from definition files...");

            for (int i = paths.Length - 1; i >= 0; i--) // we start from the last entry because mod files are preferred over vanilla ones
            {
                string path = paths[i];
                if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                try
                {
                    if (game.Game.ClausewitzEngineVersion < 3)
                    {
                        palToTerr = new Dictionary<byte, byte>();
                        CEParser.File file = new CEParser.TextFile(path); // map\terrain.txt
                        file.Parse();

                        List<Terrain> list = new List<Terrain>();

                        var nodes = file.GetSubnodes(file.Root, "categories", "*");
                        foreach (CEParser.Node n in nodes)
                        {
                            string name = file.GetNodeName(n);
                            bool water = file.HasAnAttribute(n, "is_water", "yes");
                            int color = 0;

                            if (file.HasAContainer(n, "color"))
                            {
                                string[] colorDef = file.GetEntries(file.GetSubnode(n, "color"));
                                color = CEBitmap.Bitmap.ColorToInt32(colorDef);
                            }

                            list.Add(new Terrain(name, name, color, water));
                            //Core.Log.Write("New terrain type definition added: " + name + ".");
                        }

                        foreach (CEParser.Node n in file.GetSubnodes(file.Root))
                        {
                            if (file.GetNodeName(n) == "categories") continue;

                            // Now fill in the conversion table
                            string[] colors = file.GetEntries(file.GetSubnode(n, "color"));
                            foreach (string c in colors)
                            {
                                byte terrainIndex = (byte)list.FindIndex(x => file.GetAttributeValue(n, "type") == x.Name);
                                palToTerr.Add(Byte.Parse(c), terrainIndex);
                            }
                        }

                        terrainTypes = list.ToArray();
                    }
                    else
                    {
                        palToTerr = new Dictionary<byte, byte>();
                        CEParser.File file = new CEParser.TextFile(path); // map\terrain.txt
                        file.Parse();

                        List<Terrain> list = new List<Terrain>();

                        foreach (CEParser.Node n in file.GetSubnodes(file.Root, "categories", "*"))
                        {
                            string name = file.GetNodeName(n);
                            bool water = file.HasAnAttribute(n, "is_water", "yes");
                            int color = 0;

                            if (file.HasAContainer(n, "color"))
                            {
                                string[] colorDef = file.GetEntries(file.GetSubnode(n, "color"));
                                color = CEBitmap.Bitmap.ColorToInt32(colorDef);
                            }

                            list.Add(new Terrain(name, name, color, water));
                            //Core.Log.Write("New terrain type definition added: " + name + ".");
                        }

                        CEParser.Node t = file.GetSubnode(file.Root, 1);
                        foreach (CEParser.Node n in file.GetSubnodes(t))
                        {
                            // Now fill in the conversion table
                            string[] colors = file.GetEntries(file.GetSubnode(n, "color"));
                            foreach (string c in colors)
                            {
                                byte terrainIndex = (byte)list.FindIndex(x => file.GetAttributeValue(n, "type") == x.Name);
                                palToTerr.Add(Byte.Parse(c), terrainIndex);
                            }
                        }

                        terrainTypes = list.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error reading terrain definitions from <" + path + "> file.", ex);
                    return;
                }

                consumedFilenames.Add(Path.GetFileName(path));
            }
        }

        public Terraindefs(byte[] raw)
        {
            Load(raw);
        }

        public int[] GetWaterTerrains()
        {
            // All provinces have their terrain set, now set their water status
            List<int> waterTerrainIDs = new List<int>();
            if (terrainTypes == null) return waterTerrainIDs.ToArray();

            for (int i = 0; i < terrainTypes.Length; i++)
            {
                if (terrainTypes[i].IsWater) waterTerrainIDs.Add(i);
            }

            return waterTerrainIDs.ToArray();
        }

        public byte ColorIndexToTerrainType(byte index, Dictionary<byte, byte> conv)
        {
            byte output;
            palToTerr.TryGetValue(index, out output);

            if (conv != null && conv.ContainsKey(output))
                output = conv[output];

            return output;
        }

        public byte[] GetColorIndexAggregator(Dictionary<byte, byte> conv)
        {
            byte[] output = new byte[256];
            if (palToTerr == null) return output;
            for (int i = 0; i < 256; i++)
            {
                palToTerr.TryGetValue((byte)i, out output[i]);

                if (conv != null && conv.ContainsKey(output[i]))
                    output[i] = conv[output[i]];
            }
            return output;
        }

        public int[] GetColors()
        {
            return terrainTypes.Select(x => x.DefaultColor).ToArray();
        }

        public string GetName(int index)
        {
            return terrainTypes[index].Label;
        }

        public int GetIndex(string name)
        {
            int index = Array.FindIndex(terrainTypes, x => x.Name == name);
            return index;
        }

        private void Load(byte[] raw)
        {
            List<Terrain> list = new List<Terrain>();

            int index = 0;
            int len;
            string name;
            string label;
            int color;
            bool water;

            while (index < raw.Length)
            {
                len = raw[index++];
                name = Encoding.UTF8.GetString(raw, index, len); index += len;
                len = raw[index++];
                label = Encoding.UTF8.GetString(raw, index, len); index += len;
                color = BitConverter.ToInt32(raw, index); index += 4;
                water = BitConverter.ToBoolean(raw, index++);
                list.Add(new Terrain(name, label, color, water));
            }

            terrainTypes = list.ToArray();
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter b = new BinaryWriter(ms))
                {
                    if (terrainTypes == null) return ms.ToArray();
                    for (int i = 0; i < terrainTypes.Length; i++)
                    {
                        b.Write((byte)Encoding.UTF8.GetByteCount(terrainTypes[i].Name));
                        b.Write(Encoding.UTF8.GetBytes(terrainTypes[i].Name));
                        b.Write((byte)Encoding.UTF8.GetByteCount(terrainTypes[i].Label));
                        b.Write(Encoding.UTF8.GetBytes(terrainTypes[i].Label));
                        b.Write(terrainTypes[i].DefaultColor);
                        b.Write(terrainTypes[i].IsWater);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    public struct Terrain
    {
        public string Name;
        public string Label;
        public int DefaultColor;
        public bool IsWater;

        public Terrain(string name, string label, int color, bool water)
        {
            this.Name = name;
            this.Label = label;
            this.DefaultColor = color;
            this.IsWater = water;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
