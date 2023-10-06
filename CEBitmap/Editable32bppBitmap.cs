using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CEBitmap
{
    public class Editable32bppBitmap : Bitmap
    {
        public ushort[] IDMap;
        public int[] PosMap;
        public int[] RowsOrigins;

        public Editable32bppBitmap(string path, string idConvTablePath, bool flip)
        {
            // Read conversion table
            Dictionary<int, ushort> idConverter = GetIDConversionTable(idConvTablePath);
            int width;
            int height;

            Reader.Read32bppBitmap(path, out width, out height, idConverter, flip, out IDMap, out PosMap, out RowsOrigins);
            Width = width;
            Height = height;
        }

        public Editable32bppBitmap(byte[] raw)
        {
            Load(raw);
        }

        /// <summary>
        /// Returns province ID at the specified position, where position is specified in a two-dimensional array of bytes,
        /// counted from top-left (0,0).
        /// </summary>
        /// <param name="coords">Coordinates</param>
        /// <returns>Province ID</returns>
        public ushort GetIDByCoords(int x, int y)
        {
            int pos = y * Width + x;
            int min = 0;
            int max = PosMap.Length - 1;

            // Binary search
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (pos >= PosMap[mid] && pos < PosMap[mid + 1])
                {
                    return IDMap[PosMap[mid]];
                }
                else if (pos < PosMap[mid])
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Takes the source raw array and edits provinces colors so that each province gets a new color. 
        /// Colors are specified within an array which must have as many children as there are provinces.
        /// </summary>
        /// <param name="colors">Colors of provinces shapes</param>
        public void EditBitmap(int[] buffer, int[][] colors, int row)
        {
            int origin = PosMap[RowsOrigins[row]];
            for (int i = RowsOrigins[row] + 1; i < RowsOrigins[row + 1] + 1; i++)
            {
                ushort id = IDMap[i - 1];
                int start = PosMap[i - 1] - origin;
                int end = PosMap[i] - origin;

                if (colors[id].Length > 1)
                {
                    for (int j = start; j < end; j++)
                        buffer[j] = colors[id][((j + j / Width) / 2) % colors[id].Length];
                }
                else
                {
                    int c = colors[id][0];
                    for (int j = start; j < end; j++)
                        buffer[j] = c;
                }
            }
        }

        private void Load(byte[] raw)
        {
            int index = 0;
            Width = BitConverter.ToInt32(raw, index); index += 4;
            Height = BitConverter.ToInt32(raw, index); index += 4;

            int len = BitConverter.ToInt32(raw, index); index += 4;
            IDMap = new ushort[len];
            for (int i = 0; i < len; i++)
            {
                IDMap[i] = (BitConverter.ToUInt16(raw, index));
                index += 2;
            }

            len = BitConverter.ToInt32(raw, index); index += 4;
            PosMap = new int[len];
            for (int i = 0; i < len; i++)
            {
                PosMap[i] = (BitConverter.ToInt32(raw, index));
                index += 4;
            }

            len = BitConverter.ToInt32(raw, index); index += 4;
            RowsOrigins = new int[len];
            for (int i = 0; i < len; i++)
            {
                RowsOrigins[i] = (BitConverter.ToInt32(raw, index));
                index += 4;
            }
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter b = new BinaryWriter(ms))
                {
                    b.Write(Width);
                    b.Write(Height);
                    b.Write(IDMap.Length);
                    for (int i = 0; i < IDMap.Length; i++)
                    {
                        b.Write(IDMap[i]);
                    }

                    b.Write(PosMap.Length);
                    for (int i = 0; i < PosMap.Length; i++)
                    {
                        b.Write(PosMap[i]);
                    }

                    b.Write(RowsOrigins.Length);
                    for (int i = 0; i < RowsOrigins.Length; i++)
                    {
                        b.Write(RowsOrigins[i]);
                    }
                    return ms.ToArray();
                }
            }
        }

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
                Byte.TryParse(items[1].Trim(' ','.'), out r);
                Byte.TryParse(items[2].Trim(' ', '.'), out g);
                Byte.TryParse(items[3].Trim(' ','.'), out b);
                int color = Bitmap.ColorToInt32(r, g, b);
                if (!conv.ContainsKey(color))
                    conv.Add(color, id);
            }

            return conv;
        }
    }
}
