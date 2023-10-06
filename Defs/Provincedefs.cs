using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chronicle
{
    /// <summary>
    /// Contains information about provinces.
    /// </summary>
    public class Provincedefs : MarshalByRefObject
    {
        List<Province> provinces = new List<Province>();
        bool[] water;

        public int Count { get { return provinces.Count; } }

        public Provincedefs(InstalledGame game, string[] paths)
        {
            List<string> consumedFilenames = new List<string>();
            Core.Log.Write("Reading province definitions from definition files...");

            for (int i = paths.Length - 1; i >= 0; i--) // we start from the last entry because mod files are preferred over vanilla ones
            {
                string path = paths[i];
                if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                try
                {
                    // Get number of provinces
                    string[] lines = File.ReadAllLines(path, System.Text.Encoding.GetEncoding(1250));

                    provinces.Add(new Province(0)); // add special empty province

                    // Initialize and load provinces
                    foreach (var line in lines)
                    {
                        ushort id;
                        if (line.Trim() == "" || !Char.IsDigit(line[0])) continue;
                        var s = line.Split(';');
                        UInt16.TryParse(s[0], out id);
                        if (id < 1) continue;

                        Province p = new Province(id);

                        AddProvince(p);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error reading province definitions from <" + path + "> file.", ex);
                    return;
                }

                consumedFilenames.Add(Path.GetFileName(path));
            }
        }

        public int GetOverriddenTerrain(int i)
        {
            return provinces[i].OverriddenTerrain;
        }

        public int GetFirstPixel(ushort id)
        {
            return provinces[id].firstPx;
        }

        public int GetLastPixel(ushort id)
        {
            return provinces[id].lastPx;
        }

        public Provincedefs(byte[] raw)
        {
            Load(raw);
        }

        public void LoadLabelPositions(InstalledGame game, string[] paths, int width, int height)
        {
            List<string> consumedFilenames = new List<string>();
            Core.Log.Write("Reading province label positions from definition files...");

            // Load positions of province labels. ("map\\positions.txt");
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                //if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                try
                {
                    CEParser.File file = new CEParser.TextFile(path);
                    file.Parse();

                    if (game.Game.ClausewitzEngineVersion == 1)
                    {
                        foreach (var n in file.GetSubnodes(file.Root))
                        {
                            ushort id;
                            UInt16.TryParse(file.GetNodeName(n), out id);
                            if (id == 0 || id >= provinces.Count) continue;

                            // Get icon position
                            if (file.HasAContainer(n, "city"))
                                provinces[id].IconPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "city"), "x")), height - Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "city"), "y")));
                            else if (file.HasAContainer(n, "unit"))
                                provinces[id].IconPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "unit"), "x")), height - Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "unit"), "y")));
                            else
                            {
                                CEParser.Node node = file.GetSubnode(n, child => file.HasAnAttributeWithName(child, "x"));
                                if (node == null)
                                    provinces[id].IconPosition = new Point(0, 0);
                                else
                                    provinces[id].IconPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(node, "x")), height - Ext.ParseDouble(file.GetAttributeValue(node, "y")));
                            }

                            // Get text position
                            if (file.HasAContainer(n, "text_position"))
                                provinces[id].TextPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "text_position"), "x")), height - Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "text_position"), "y")));
                            else if (file.HasAContainer(n, "city"))
                                provinces[id].TextPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "city"), "x")), height - Ext.ParseDouble(file.GetAttributeValue(file.GetSubnode(n, "city"), "y")));
                            else
                            {
                                CEParser.Node node = file.GetSubnode(n, child => file.HasAnAttributeWithName(child, "x"));
                                if (node == null)
                                    provinces[id].TextPosition = new Point(0, 0);
                                else
                                    provinces[id].TextPosition = new Point(Ext.ParseDouble(file.GetAttributeValue(node, "x")), height - Ext.ParseDouble(file.GetAttributeValue(node, "y")));
                            }

                            // Get text rotation
                            if (file.HasAnAttributeWithName(n, "text_rotation"))
                                provinces[id].TextRotation = (float)Ext.ParseDouble(file.GetAttributeValue(n, "text_rotation"));

                            // Get text scale
                            if (file.HasAnAttributeWithName(n, "text_scale"))
                                provinces[id].TextScale = (float)Ext.ParseDouble(file.GetAttributeValue(n, "text_scale"));
                        }
                    }
                    else
                    {
                        foreach (var n in file.GetSubnodes(file.Root))
                        {
                            ushort id;
                            UInt16.TryParse(file.GetNodeName(n), out id);
                            if (id == 0 || id >= provinces.Count) continue;

                            // Get text position
                            if (file.HasAContainer(n, "position"))
                                provinces[id].TextPosition = new Point(Ext.ParseDouble(file.GetEntry(file.GetSubnode(n, "position"), 2)), height - Ext.ParseDouble(file.GetEntry(file.GetSubnode(n, "position"), 3)));

                            // Get text rotation
                            if (file.HasAContainer(n, "rotation"))
                                provinces[id].TextRotation = (float)Ext.ParseDouble(file.GetEntry(file.GetSubnode(n, "rotation"), 4));

                            // Get text scale
                            provinces[id].TextScale = 3.2f;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.ReportError("Error parsing province positions from file: <" + path + ">.", ex);
                    return;
                }

                consumedFilenames.Add(Path.GetFileName(path));
            }
        }

        public void SetOverriddenTerrain(string[] paths, Terraindefs terrainDefs)
        {
            if (terrainDefs == null) return;
            List<string> consumedFilenames = new List<string>();

            for (int i = paths.Length - 1; i >= 0; i--) // we start from the last entry because mod files are preferred over vanilla ones
            {
                string path = paths[i];
                if (consumedFilenames.Contains(Path.GetFileName(path))) continue;

                try
                {
                    if (Path.GetFileName(path) == "terrain.txt")
                    {

                        CEParser.File file = new CEParser.TextFile(path); // map\terrain.txt
                        file.Parse();

                        foreach (CEParser.Node n in file.GetSubnodes(file.Root, "categories", "*"))
                        {
                            string name = file.GetNodeName(n);

                            if (file.HasAContainer(n, "terrain_override"))
                            {
                                string[] def = file.GetEntries(file.GetSubnode(n, "terrain_override"));
                                for (int j = 0; j < def.Length; j++)
                                {
                                    int prov;
                                    if (Int32.TryParse(def[j], out prov))
                                    {
                                        if (prov > 0 && prov < provinces.Count) provinces[prov].OverriddenTerrain = terrainDefs.GetIndex(name);
                                        // Now set terrain pixels too
                                        if (provinces[prov].TerrainPixels != null)
                                        {
                                            int sum = provinces[prov].TerrainPixels.Sum();
                                            for (int k = 0; k < provinces[prov].TerrainPixels.Length; k++)
                                            {
                                                provinces[prov].TerrainPixels[k] = 0;
                                            }
                                            provinces[prov].TerrainPixels[provinces[prov].OverriddenTerrain] = sum;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Path.GetFileName(path) == "definition.csv")
                    {
                        string[] lines = File.ReadAllLines(path, System.Text.Encoding.GetEncoding(1250));
                        // Initialize and load provinces
                        foreach (var line in lines)
                        {
                            ushort id;
                            if (line.Trim() == "" || !Char.IsDigit(line[0])) continue;
                            var s = line.Split(';');
                            UInt16.TryParse(s[0], out id);
                            if (id < 1) continue;
                            provinces[id].OverriddenTerrain = terrainDefs.GetIndex(s[6]);
                            // Now set terrain pixels too
                            if (provinces[id].TerrainPixels != null)
                            {
                                int sum = provinces[id].TerrainPixels.Sum();
                                for (int k = 0; k < provinces[id].TerrainPixels.Length; k++)
                                {
                                    provinces[id].TerrainPixels[k] = 0;
                                }
                                provinces[id].TerrainPixels[provinces[id].OverriddenTerrain] = sum;
                            }
                        }
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

        int[] provinceBaseRows;
        public int[] GetProvinceBaseRows(float width)
        {
            if (provinceBaseRows == null)
            {
                provinceBaseRows = new int[provinces.Count];
                for (int i = 0; i < provinceBaseRows.Length; i++)
                {
                    provinceBaseRows[i] = (int)(provinces[i].lastPx / width);
                }
            }
            return provinceBaseRows;
        }

        float[] provinceHeights;
        public float[] GetProvinceHeights(float width)
        {
            if (provinceHeights == null)
            {
                provinceHeights = new float[provinces.Count];
                for (int i = 0; i < provinceHeights.Length; i++)
                {
                    provinceHeights[i] = ((provinces[i].lastPx - provinces[i].firstPx) / width);
                }
            }
            return provinceHeights;
        }

        void AddProvince(Province p)
        {
            if (p.ID < provinces.Count)
            {
                provinces[p.ID] = p;
                provinces.Add(new Province(ushort.MinValue)); // To prevent provinces count mismatch
            }
            else
            {
                provinces.Add(p);
            }
        }

        public void SetWaterTerrains(int[] terrains)
        {
            Province.WaterTerrains = terrains;
        }

        public void SetTerrainTypes(int count)
        {
            foreach (var p in provinces)
            {
                p.TerrainPixels = new int[count];
            }
        }

        public double[] GetTerrainTypes(int index)
        {
            double[] output = new double[provinces[index].TerrainPixels.Length];
            double sum = provinces[index].TerrainPixels.Sum();
            if (sum <= 0) return output;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = provinces[index].TerrainPixels[i] / sum;
            }
            return output;
        }

        /// <summary>
        /// Adds terrain pixels to the given province
        /// </summary>
        /// <param name="terrain">Terrain type, given as int identifier</param>
        /// <param name="area">Area of the added terrain, in pixels</param>
        /// <param name="currentPos">Origin (absolute) of the added terrain</param>
        public void AddTerrainPixels(int province, int terrain, int area, int currentPos)
        {
            provinces[province].AddTerrainPixels(terrain, area, currentPos);
        }

        public TextBlock[] InitializeLabels(out RotateTransform[] rotation, out Point[] position)
        {
            TextBlock[] labels = new TextBlock[provinces.Count];
            bool[] water = WaterProvinces;
            Color clr = CEBitmap.Bitmap.Int32ToColor(Core.Settings.LabelsColor);

            rotation = new RotateTransform[provinces.Count];
            position = new Point[provinces.Count];

            for (int i = 0; i < provinces.Count; i++)
            {
                labels[i] = new TextBlock();
                labels[i].FontSize = Math.Max(Math.Min(provinces[i].TextScale * 2f, 4f), 2f);
                labels[i].Text = "";
                labels[i].Foreground = new SolidColorBrush(clr);
                labels[i].RenderTransformOrigin = new Point(0.5, 0.5);
                position[i] = provinces[i].TextPosition;

                if (water[i]) labels[i].Visibility = Visibility.Hidden;

                if (provinces[i].TextRotation != 0)
                {
                    double r;
                    if (provinces[i].TextRotation < 3) r = -45;
                    else r = 45;
                    RotateTransform rt = new RotateTransform();
                    rt.Angle = r;
                    rotation[i] = rt;
                }
            }

            return labels;
        }

        public bool IsWater(int id)
        {
            return provinces[id].IsWater;
        }

        public bool[] WaterProvinces
        {
            get
            {
                if (water == null)
                {
                    water = provinces.Select(x => x.IsWater).ToArray();
                }
                return water;
            }
        }

        public ushort[] ListWaterProvinces()
        {
            return provinces.FindAll(x => x.IsWater).Select(x => x.ID).ToArray();
        }

        private void Load(byte[] raw)
        {
            provinces = new List<Province>();

            int index = 0;
            int strlen = 0;
            //int arrlen = 0;
            int terrainTypesCount = BitConverter.ToInt32(raw, index); index += 4;
            Province p;

            while (index < raw.Length)
            {
                p = new Province(BitConverter.ToUInt16(raw, index)); index += 2;
                p.TerrainPixels = new int[terrainTypesCount];
                for (int i = 0; i < terrainTypesCount; i++)
                {
                    p.TerrainPixels[i] = BitConverter.ToInt32(raw, index); index += 4;
                }
                p.TotalArea = BitConverter.ToInt32(raw, index); index += 4;
                strlen = raw[index++];
                p.Name = Encoding.UTF8.GetString(raw, index, strlen); index += strlen;
                p.TextPosition = new Point(BitConverter.ToDouble(raw, index), BitConverter.ToDouble(raw, index + 8)); index += 16;
                p.IconPosition = new Point(BitConverter.ToDouble(raw, index), BitConverter.ToDouble(raw, index + 8)); index += 16;
                p.TextRotation = BitConverter.ToSingle(raw, index); index += 4;
                p.TextScale = BitConverter.ToSingle(raw, index); index += 4;

                p.SetBounds(BitConverter.ToInt32(raw, index), BitConverter.ToInt32(raw, index + 4)); index += 8;

                // Added in 3.1
                //strlen = raw[index++];
                //p.OverriddenTerrain = Encoding.UTF8.GetString(raw, index, strlen); index += strlen;
                p.OverriddenTerrain = BitConverter.ToInt32(raw, index); index += 4;

                provinces.Add(p);
            }
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter b = new BinaryWriter(ms))
                {
                    int terrainTypes = provinces[0].TerrainPixels.Length;
                    b.Write(terrainTypes);

                    byte[] buffer = new byte[1048576];

                    foreach (var p in provinces)
                    {
                        b.Write(p.ID);
                        for (int i = 0; i < terrainTypes; i++) b.Write(p.TerrainPixels[i]);
                        b.Write(p.TotalArea);
                        b.Write((byte)Encoding.UTF8.GetByteCount(p.Name));
                        b.Write(Encoding.UTF8.GetBytes(p.Name));
                        b.Write(p.TextPosition.X);
                        b.Write(p.TextPosition.Y);
                        b.Write(p.IconPosition.X);
                        b.Write(p.IconPosition.Y);
                        b.Write(p.TextRotation);
                        b.Write(p.TextScale);

                        b.Write(p.firstPx);
                        b.Write(p.lastPx);

                        // Added in 3.1
                        //b.Write((byte)Encoding.UTF8.GetByteCount(p.OverriddenTerrain));
                        b.Write(p.OverriddenTerrain);
                    }

                    return ms.ToArray();
                }
            }
        }

        public void SetMissingPositions(int width, int height)
        {
            for (int i = 0; i < provinces.Count; i++)
            {
                if (provinces[i].TextPosition.X < 1 && provinces[i].TextPosition.Y < 1)
                    provinces[i].TextPosition = provinces[i].GetGeometricCenter(width, height);
                if (provinces[i].IconPosition.X < 1 && provinces[i].IconPosition.Y < 1)
                    provinces[i].IconPosition = provinces[i].GetGeometricCenter(width, height);
            }
        }

        public void LoadNames(InstalledGame game, List<string> locale)
        {
            if (game.Game.ClausewitzEngineVersion < 3)
            {
                for (int i = 0; i < locale.Count; i++)
                {
                    if (locale[i].StartsWith("PROV"))
                    {
                        string[] def = locale[i].Split(';');
                        if (def == null || def.Length < 2 || def[0].Length < 5) continue;
                        ushort id;
                        UInt16.TryParse(def[0].Substring(4), out id);
                        if (id > 0 && id < provinces.Count - 1) provinces[id].Name = def[1];
                    }
                }
            }
            else if (game.Game.ClausewitzEngineVersion == 3)
            {
                for (int i = 0; i < locale.Count; i++)
                {
                    if (locale[i].StartsWith(" PROV"))
                    {
                        string[] def = locale[i].Split(':');
                        if (def == null || def.Length < 2 || def[0].Length < 6) continue;
                        ushort id;
                        UInt16.TryParse(def[0].Substring(5), out id);
                        if (id > 0 && id < provinces.Count - 1) provinces[id].Name = def[1].Trim().Replace("\"", "").Replace("0 ", "");
                    }
                }
            }
        }

        public string GetName(ushort index, string format)
        {
            return String.Format(format, index, provinces[index].Name);
        }

        public IEnumerable<string> List(string format)
        {
            for (int i = 0; i < provinces.Count; i++)
            {
                yield return String.Format(format, i, provinces[i].Name);
            }
        }
    }

    public class Province
    {
        /// <summary>
        /// ID number of the province.
        /// </summary>
        public ushort ID;

        public int OverriddenTerrain = -1;

        /// <summary>
        /// Specifies how many pixels of the province belong to each terrain type. Terrain types are given in the same order as TerrainInformation array stores them.
        /// </summary>
        public int[] TerrainPixels;

        /// <summary>
        /// Gives the total area of a province, in pixels.
        /// </summary>
        public int TotalArea;

        /// <summary>
        /// Name of the province.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gives the (x,y) top-left origin of the province name to be displayed on the map overlay.
        /// </summary>
        public Point TextPosition;

        /// <summary>
        /// Gives the (x,y) top-left position of the icon, if it is to be shown.
        /// </summary>
        public Point IconPosition;

        /// <summary>
        /// Gives the text rotation (in grades), where 0 = no rotation.
        /// </summary>
        public float TextRotation;

        /// <summary>
        /// Gives the text scale, where 1 = 100% scale.
        /// </summary>
        public float TextScale;

        internal int firstPx { get; private set; }
        internal int lastPx { get; private set; }
        private Point geometricCenter = new Point(-1, -1);

        private int majorityTerrain = -1;
        private bool? isWater = null;

        public static int[] WaterTerrains;

        /// <summary>
        /// Gets terrain identifier which forms the majority of the province terrain.
        /// </summary>
        /// <param name="terrain">Terrain to be checked</param>
        /// <returns>True if the province is of given terrain, otherwise false.</returns>
        public int MajorityTerrain
        {
            get
            {
                if (majorityTerrain < 0)
                    majorityTerrain = Array.FindIndex(TerrainPixels, x => x == TerrainPixels.Max());
                return majorityTerrain;
            }
        }

        /// <summary>
        /// Gets the Water property, i.e. if the main province terrain is water.
        /// </summary>
        /// <param name="waterTerrains">List of terrains (their string identifiers) that qualify as water</param>
        public bool IsWater
        {
            get
            {
                if (isWater == null)
                {
                    isWater = false;
                    if (ID == 0)
                    {
                        isWater = true;
                        return true;
                    }
                    foreach (int terrain in WaterTerrains)
                    {
                        if (IsTerrain(terrain))
                        {
                            isWater = true;
                            break;
                        }
                    }
                }
                return (bool)isWater;
            }
        }

        /// <summary>
        /// Initializes the new province information.
        /// </summary>
        /// <param name="id">Province id</param>
        public Province(ushort id)
        {
            ID = id;
            TextScale = 1;
            Name = "(none)";
        }

        /// <summary>
        /// Adds terrain pixels to the given province
        /// </summary>
        /// <param name="terrain">Terrain type, given as int identifier</param>
        /// <param name="area">Area of the added terrain, in pixels</param>
        /// <param name="currentPos">Origin (absolute) of the added terrain</param>
        public void AddTerrainPixels(int terrain, int area, int currentPos)
        {
            if (TerrainPixels == null || terrain >= TerrainPixels.Length) return;

            if (TotalArea == 0) firstPx = currentPos;
            lastPx = currentPos + area;

            TerrainPixels[OverriddenTerrain >= 0 ? OverriddenTerrain : terrain] += area;
            TotalArea += area;
        }

        /// <summary>
        /// Check if the province is of the given terrain (it forms the majority of its terrain).
        /// </summary>
        /// <param name="terrain">Terrain to be checked</param>
        /// <returns>True if the province is of given terrain, otherwise false.</returns>
        public bool IsTerrain(int terrain)
        {
            if (OverriddenTerrain >= 0)
            {
                return OverriddenTerrain == terrain;
            }
            return MajorityTerrain == terrain;
        }

        public bool IsTerrain(int terrain, double threshold)
        {
            if (OverriddenTerrain >= 0)
            {
                return OverriddenTerrain == terrain;
            }
            return (TerrainPixels[terrain] / (double)TotalArea) >= threshold;
        }

        /// <summary>
        /// Gets the geometric center of the province.
        /// </summary>
        /// <param name="mapWidth">Absolute width of the map</param>
        /// <param name="mapHeight">Absolute height of the map</param>
        /// <returns>Geometric center</returns>
        public Point GetGeometricCenter(int width, int height)
        {
            if (geometricCenter.X < 0)
            {
                if (width == 0 || height == 0) return new Point(0, 0);
                int x = (lastPx % width - firstPx % width) / 2 + firstPx % width;
                int y = (((int)(lastPx / width) - (int)(firstPx / width)) + (int)(firstPx / width));
                geometricCenter = new Point(x, y);
            }
            return geometricCenter;
        }

        internal void SetBounds(int f, int l)
        {
            firstPx = f;
            lastPx = l;
        }
    }
}
