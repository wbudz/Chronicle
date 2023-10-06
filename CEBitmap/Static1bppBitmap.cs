using System;
using System.Collections.Generic;
using System.IO;

namespace CEBitmap
{
    public class Static1bppBitmap : Bitmap
    {
        /// <summary>
        /// Raw array of pixels, with each pixel represented by 8bit integer denoting its location.
        /// </summary>
        public List<int> Pixels { get; private set; }

        public List<int> RowsOrigins;

        public Static1bppBitmap(string path, byte[] ignoredColors, bool flip)
        {
            if (!(File.Exists(path))) throw new Exception("The file at the location: <" + path + "> does not exist.");
            int width;
            int height;

            // Read Raw contents
            Pixels = Reader.Read1bppBitmap(path, out width, out height, ignoredColors, flip, out RowsOrigins);
            Width = width;
            Height = height;
        }

        public Static1bppBitmap(List<int> pixels, List<int> rowsOrigins, int width, int height)
        {
            Pixels = pixels;
            RowsOrigins = rowsOrigins;
            Width = width;
            Height = height;
        }
    }
}
