using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CEBitmap
{
    public class Traced32bppBitmap : Bitmap
    {
        string path;

        internal ConcurrentStack<Polygon> Polygons { get; private set; } = new ConcurrentStack<Polygon>();
        //internal ConcurrentStack<Border> Borders { get; private set; } = new ConcurrentStack<Border>();

        public Traced32bppBitmap(string path, string idConvTablePath, bool flip, ConcurrentStack<PointCollection> heap)
        {
            this.path = path;
            Dictionary<int, ushort> idConverter = GetIDConversionTable(idConvTablePath);
            var map = Read(idConverter, flip);

            //Parallel.For(0, 16, i =>
            //  {
            //      TraceMapBlock(i % 4, i / 4, map);
            //  });

            TraceMapBlock(0, 0, map);

            Debug.WriteLine("The ladybird made " + Polygons.Sum(x => x.Points.Count) + " steps.");
        }

        public Traced32bppBitmap(string path)
        {
            //ReadBIN(path);
        }

        public ushort[,] Read(Dictionary<int, ushort> idConverter, bool flip)
        {
            if (!(File.Exists(path))) throw new Exception("The source file at the location: <" + path + "> does not exist.");

            ushort[,] map;

            // Read raw contents and create an id map.
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] header = new byte[54];
                fs.Read(header, 0, header.Length);
                Width = BitConverter.ToInt32(header, 18);
                Height = BitConverter.ToInt32(header, 22);
                if (BitConverter.ToInt16(header, 28) != 24) throw new Exception("The source file at the location: <" + path + "> has incorrect color depth.");

                map = new ushort[Width, Height];

                byte[] buffer = new byte[Width * 3];
                int y = 0;
                int x = 0;
                ushort id;

                if (flip)
                {
                    fs.Seek(-(fs.Length % (54 + Width * Height * 3)) - Width * 3, SeekOrigin.End);
                    while (fs.Position >= 54)
                    {
                        x = 0;
                        fs.Read(buffer, 0, buffer.Length);
                        idConverter.TryGetValue(Bitmap.ColorToInt32(buffer[2], buffer[1], buffer[0]), out id);
                        map[x++, y] = id;
                        for (int i = 3; i < buffer.Length - 2; i += 3)
                        {
                            if ((buffer[i - 3] != buffer[i]) || (buffer[i - 2] != buffer[i + 1]) || (buffer[i - 1] != buffer[i + 2]))
                            {
                                idConverter.TryGetValue(Bitmap.ColorToInt32(buffer[i + 2], buffer[i + 1], buffer[i]), out id);
                            }
                            map[x++, y] = id;
                        }
                        y++;
                        if (fs.Position < Width * 3 * 2)
                            break;
                        else
                            fs.Seek(-Width * 3 * 2, SeekOrigin.Current);
                    }
                }
                else
                {
                    while (fs.Position < 54 + Width * Height * 3)
                    {
                        x = 0;
                        fs.Read(buffer, 0, buffer.Length);
                        idConverter.TryGetValue(Bitmap.ColorToInt32(buffer[2], buffer[1], buffer[0]), out id);
                        map[x++, y] = id;
                        for (int i = 3; i < buffer.Length - 2; i += 3)
                        {
                            if ((buffer[i - 3] != buffer[i]) || (buffer[i - 2] != buffer[i + 1]) || (buffer[i - 1] != buffer[i + 2]))
                            {
                                idConverter.TryGetValue(Bitmap.ColorToInt32(buffer[i + 2], buffer[i + 1], buffer[i]), out id);
                            }
                            map[x++, y] = id;
                        }
                        y++;
                    }
                }
            }

            return map;
        }

        public void TraceMapBlock(int xStart, int yStart, ushort[,] map)
        {
            int xLen = Width;
            int yLen = Height;
            bool insideShape = false;

            bool[,] visited = new bool[xLen, yLen];

            for (int y = 0; y < yLen; y++) // for (int y = yStart; y < yStart + Height / 4; y++)
            {
                insideShape = false;
                for (int x = 0; x < xLen; x++)
                {
                    if (visited[x, y]) continue;
                    if (map[xStart + x, yStart + y] == 0) continue;
                    if (x > 0 && map[xStart + x, yStart + y] != map[xStart + x - 1, yStart + y]) insideShape = false;
                    if (insideShape) continue;

                    Polygons.Push(TraceFromStart(x, y, new Int32Rect(xStart, yStart, xLen, yLen), map, ref visited));
                    insideShape = true;
                }
            }
        }

        private Polygon TraceFromStart(int x, int y, Int32Rect bounds, ushort[,] map, ref bool[,] visited)
        {
            // We start from the given point
            int xStart = x;
            int yStart = y;
            ushort id = map[x + bounds.X, y + bounds.Y];

            Direction dir = Direction.N;

            List<Point> points = new List<Point>();
            Point lastPoint = new Point(x, y);
            points.Add(lastPoint);

            int directionChanges = 0;

            while (true)
            {
                visited[x, y] = true;

                switch (dir)
                {
                    case Direction.N:
                        if (y == 0)
                        {
                            if (x < bounds.Width - 1 && id == map[x + 1 + bounds.X, y + bounds.Y])
                            {
                                dir = Direction.E;
                                directionChanges++;
                            }
                            else if (id == map[x + bounds.X, y + 1 + bounds.Y])
                            {
                                dir = Direction.S;
                                directionChanges++;
                            }
                            else
                            {
                                directionChanges = 4;
                                break;
                            }
                            directionChanges = 0;
                        }
                        else if (x > 0 && id == map[x - 1 + bounds.X, y - 1 + bounds.Y])
                        {
                            lastPoint = new Point(--x, --y);
                            points.Add(lastPoint);
                            dir = Direction.W;
                            directionChanges = 0;
                        }
                        else if (id == map[x + bounds.X, y - 1 + bounds.Y])
                        {
                            points.Add(new Point(x, --y));
                            directionChanges = 0;
                        }
                        else if (x < bounds.Width - 1 && id == map[x + 1 + bounds.X, y - 1 + bounds.Y])
                        {
                            lastPoint = new Point(++x + 1, --y);
                            points.Add(lastPoint);
                            dir = Direction.N;
                            directionChanges = 0;
                        }
                        else
                        {
                            dir = Direction.E;
                            directionChanges++;
                        }
                        break;
                    case Direction.E:
                        if (x == bounds.Width - 1)
                        {
                            if (y < bounds.Height - 1 && id == map[x + bounds.X, y + 1 + bounds.Y])
                            {
                                dir = Direction.S;
                                directionChanges++;
                            }
                            else if (id == map[x - 1 + bounds.X, y + bounds.Y])
                            {
                                dir = Direction.W;
                                directionChanges++;
                            }
                            else
                            {
                                directionChanges = 4;
                                break;
                            }
                            directionChanges = 0;
                        }
                        else if (y > 0 && id == map[x + 1 + bounds.X, y - 1 + bounds.Y])
                        {
                            lastPoint = new Point(++x + 1, --y);
                            points.Add(lastPoint);
                            dir = Direction.N;
                            directionChanges = 0;
                        }
                        else if (id == map[x + 1 + bounds.X, y + bounds.Y])
                        {
                            points.Add(new Point(++x + 1, y));
                            directionChanges = 0;
                        }
                        else if (y < bounds.Height - 1 && id == map[x + 1 + bounds.X, y + 1 + bounds.Y])
                        {
                            lastPoint = new Point(++x + 1, ++y + 1);
                            points.Add(lastPoint);
                            dir = Direction.E;
                            directionChanges = 0;
                        }
                        else
                        {
                            dir = Direction.S;
                            directionChanges++;
                        }
                        break;
                    case Direction.S:
                        if (y == bounds.Height - 1)
                        {
                            if (x > 0 && id == map[x - 1 + bounds.X, y + bounds.Y])
                            {
                                dir = Direction.W;
                                directionChanges++;
                            }
                            else if (id == map[x + bounds.X, y - 1 + bounds.Y])
                            {
                                dir = Direction.N;
                                directionChanges++;
                            }
                            else
                            {
                                directionChanges = 4;
                                break;
                            }
                            directionChanges = 0;
                        }
                        else if (x < bounds.Width - 1 && id == map[x + 1 + bounds.X, y + 1 + bounds.Y])
                        {
                            lastPoint = new Point(++x + 1, ++y + 1);
                            points.Add(lastPoint);
                            dir = Direction.E;
                            directionChanges = 0;
                        }
                        else if (id == map[x + bounds.X, y + 1 + bounds.Y])
                        {
                            points.Add(new Point(x, ++y + 1));
                            directionChanges = 0;
                        }
                        else if (x > 0 && id == map[x - 1 + bounds.X, y + 1 + bounds.Y])
                        {
                            lastPoint = new Point(--x, ++y + 1);
                            points.Add(lastPoint);
                            dir = Direction.S;
                            directionChanges = 0;
                        }
                        else
                        {
                            dir = Direction.W;
                            directionChanges++;
                        }
                        break;
                    case Direction.W:
                        if (x == 0)
                        {
                            if (y > 0 && id == map[x + bounds.X, y - 1 + bounds.Y])
                            {
                                dir = Direction.N;
                                directionChanges++;
                            }
                            else if (id == map[x + 1 + bounds.X, y + bounds.Y])
                            {
                                dir = Direction.E;
                                directionChanges++;
                            }
                            else
                            {
                                directionChanges = 4;
                                break;
                            }
                            directionChanges = 0;
                        }
                        else if (y < bounds.Height - 1 && id == map[x - 1 + bounds.X, y + 1 + bounds.Y])
                        {
                            lastPoint = new Point(--x, ++y + 1);
                            points.Add(lastPoint);
                            dir = Direction.S;
                            directionChanges = 0;
                        }
                        else if (id == map[x - 1 + bounds.X, y + bounds.Y])
                        {
                            points.Add(new Point(x--, y));
                            directionChanges = 0;
                        }
                        else if (y > 0 && id == map[x - 1 + bounds.X, y - 1 + bounds.Y])
                        {
                            lastPoint = new Point(--x, --y);
                            points.Add(lastPoint);
                            dir = Direction.W;
                            directionChanges = 0;
                        }
                        else
                        {
                            dir = Direction.N;
                            directionChanges++;
                        }
                        break;
                }

                if (points.Count > 2000)
                    break;
                if (directionChanges == 0 && x == xStart && y == yStart) break;
                if (directionChanges > 3) break;
            }

            Polygon p = new Polygon();
            p.ID = id;
            p.Points = points;
            return p;
        }

        enum Direction { N, E, S, W }

        ///// <summary>
        ///// Saves the definitions to an external BIN file.
        ///// </summary>
        ///// <param name="path">Path of the file</param>
        //public void SaveToFile(string path)
        //{
        //    WriteBIN(path);
        //}

        //private void ReadBIN(string path)
        //{
        //    using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        //    {
        //        b.BaseStream.Seek(0, SeekOrigin.Begin);
        //        Width = b.ReadInt32();
        //        Height = b.ReadInt32();

        //        int len = b.ReadInt32();
        //        for (int i = 0; i < len; i++)
        //        {
        //            IDMap.Add(b.ReadUInt16());
        //        }

        //        len = b.ReadInt32();
        //        for (int i = 0; i < len; i++)
        //        {
        //            PosMap.Add(b.ReadInt32());
        //        }
        //    }
        //}

        //private void WriteBIN(string path)
        //{
        //    using (BinaryWriter b = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
        //    {
        //        b.Write(Width);
        //        b.Write(Height);
        //        b.Write(IDMap.Count);
        //        for (int i = 0; i < IDMap.Count; i++)
        //        {
        //            b.Write(IDMap[i]);
        //        }
        //        b.Write(PosMap.Count);
        //        for (int i = 0; i < PosMap.Count; i++)
        //        {
        //            b.Write(PosMap[i]);
        //        }
        //    }
        //}

        /// <summary>
        /// Assigns a new id to color conversion table. Conversion table must be given as a file path and provided in a id;r;g;b format.
        /// </summary>
        /// <param name="path">Path to the conversion table file</param>
        protected Dictionary<int, ushort> GetIDConversionTable(string path)
        {
            Dictionary<int, ushort> conv = new Dictionary<int, ushort>();
            string[] lines = System.IO.File.ReadAllLines(path);
            byte r;
            byte g;
            byte b;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] items = lines[i].Split(';');
                if (items[0].Trim() == "") break;
                ushort id = UInt16.Parse(items[0]);
                Byte.TryParse(items[1], out r);
                Byte.TryParse(items[2], out g);
                Byte.TryParse(items[3], out b);
                conv.Add(Bitmap.ColorToInt32(r, g, b), id);
            }

            return conv;
        }
    }

    internal struct Polygon
    {
        public List<Point> Points;
        public ushort ID;
    }

    //internal struct Border
    //{
    //    public List<Point> Points;
    //    public ushort ID1;
    //    public ushort ID2;
    //}
}
