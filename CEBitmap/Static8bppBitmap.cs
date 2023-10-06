using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace CEBitmap
{
    public class Static8bppBitmap: Bitmap
    {
        /// <summary>
        /// Raw array of pixels, with each pixel represented by 8bit integer.
        /// </summary>
        public List<byte> IDMap;
        public List<int> PosMap;

        /// <summary>
        /// Palette data.
        /// </summary>
        public Color[] Palette { get; set; }

        /// <summary>
        /// Creates a static (uneditable) 8-bit map.
        /// </summary>
        /// <param name="path">Path of a bitmap file.</param>
        public Static8bppBitmap(string path, bool flip, int enforcedPaletteSize, byte[] aggregator)
        {
            if (!(File.Exists(path))) throw new Exception("The file at the location: <" + path + "> does not exist.");
            int width;
            int height;
            Color[] pal;

            // Read Raw contents
            Reader.Read8bppBitmap(path, out width, out height, out pal, flip, enforcedPaletteSize, out IDMap, out PosMap, aggregator);
            Width = width;
            Height = height;
            Palette = pal;
        }
    }
}
