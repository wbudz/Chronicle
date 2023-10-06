using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    /// <summary>
    /// Contains information about rivers overlay.
    /// </summary>
    public class Riverdefs
    {
        CEBitmap.Static1bppBitmap map;

        /// <summary>
        /// Creates new rivers definition from original game's BMP file.
        /// </summary>
        /// <param name="path">Path to BIN file or BMP file containing rivers definition</param>
        /// <param name="flip">If true, BMP file will be flipped upside down upon reading</param>
        public Riverdefs(string path, bool flip)
        {
            Core.Log.Write("Reading river definitions from bitmap file...");
            map = new CEBitmap.Static1bppBitmap(path, new byte[] { 254, 255 }, flip);
        }

        /// <summary>
        /// Creates new rivers definition from game cache.
        /// </summary>
        public Riverdefs(byte[] raw)
        {
            Core.Log.Write("Reading river definitions from binary file...");
            Load(raw);
        }

        /// <summary>
        /// Draws rivers by turning on appropriate pixels in the array given.
        /// </summary>
        /// <param name="array">Array of Int32 color pixels</param>
        /// <param name="riverOverlayColor">Color of rivers in Int32 format</param>
        /// <returns>Array of Int32 color pixels with rivers imposed</returns>
        public int[] Overlay(int[] array, int color, int row)
        {
            int origin = row * map.Width;
            for (int i = map.RowsOrigins[row] + 1; i < map.RowsOrigins[row + 1]; i++)
            {
                array[map.Pixels[i] - origin] = color;
            }
            return array;
        }

        public void Draw(BitmapContext bc, int color)
        {
            unsafe
            {
                int* p = bc.Pixels;

                Parallel.For(0, map.Height, (row) =>
                {
                    int start = map.RowsOrigins[row] + 1;
                    int end = map.RowsOrigins[row + 1] + 1;
                    for (int i = start; i < end; i++)
                    {
                        *(p + map.Pixels[i]) = color;
                    }
                });
            }
        }

        private void Load(byte[] raw)
        {
            int index = 0;
            int width = BitConverter.ToInt32(raw, index); index += 4;
            int height = BitConverter.ToInt32(raw, index); index += 4;

            int len = BitConverter.ToInt32(raw, index); index += 4;
            List<int> pixels = new List<int>();
            List<int> rowsOrigins = new List<int>();

            for (int i = 0; i < len; i++)
            {
                pixels.Add(BitConverter.ToInt32(raw, index)); index += 4;
            }

            len = BitConverter.ToInt32(raw, index); index += 4;
            for (int i = 0; i < len; i++)
            {
                rowsOrigins.Add(BitConverter.ToInt32(raw, index)); index += 4;
            }

            map = new CEBitmap.Static1bppBitmap(pixels, rowsOrigins, width, height);
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter b = new BinaryWriter(ms))
                {
                    b.Write(map.Width);
                    b.Write(map.Height);

                    b.Write(map.Pixels.Count);
                    for (int i = 0; i < map.Pixels.Count; i++)
                    {
                        b.Write(map.Pixels[i]);
                    }
                    b.Write(map.RowsOrigins.Count);
                    for (int i = 0; i < map.RowsOrigins.Count; i++)
                    {
                        b.Write(map.RowsOrigins[i]);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
