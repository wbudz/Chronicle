using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CEBitmap
{
    public class Bitmap
    {
        static System.Globalization.CultureInfo CI = System.Globalization.CultureInfo.InvariantCulture;

        /// <summary>
        /// Width of the underlying bitmap.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Height of the underlying bitmap.
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Converts a color defined by red, green and blue values into a single Int32 value.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>Int32 representation of the color</returns>
        public static int ColorToInt32(string r, string g, string b)
        {
            float rd = Single.Parse(r, CI);
            float gd = Single.Parse(g, CI);
            float bd = Single.Parse(b, CI);
            byte rb = (byte)(rd % 256);
            byte gb = (byte)(gd % 256);
            byte bb = (byte)(bd % 256);
            return ColorToInt32(rb, gb, bb);
        }

        public static int ColorToInt32(string[] c)
        {
            if (c.Length < 3) return 0;
            return ColorToInt32(c[0], c[1], c[2]);
        }

        public static int MakeOpaque(int color)
        {
            return 255 << 24 | color;
        }

        /// <summary>
        /// Converts a color defined by red, green and blue values into a single Int32 value.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>Int32 representation of the color</returns>
        public static int ColorToInt32(byte r, byte g, byte b)
        {
            return 255 << 24 | r << 16 | g << 8 | b << 0;
        }

        public static Color StringToColor(string[] c)
        {
            if (c.Length < 3) return new Color();
            float rd = Single.Parse(c[0], CI);
            float gd = Single.Parse(c[1], CI);
            float bd = Single.Parse(c[2], CI);
            byte rb = (byte)(rd % 256);
            byte gb = (byte)(gd % 256);
            byte bb = (byte)(bd % 256);
            return Color.FromRgb(rb, gb, bb);
        }

        /// <summary>
        /// Converts a color defined by red, green and blue values into a single Int32 value, given alpha channel setting.
        /// </summary>
        /// <param name="a">Alpha</param>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>Int32 representation of the color</returns>
        public static int ColorToInt32(byte a, byte r, byte g, byte b)
        {
            return a << 24 | r << 16 | g << 8 | b << 0;
        }

        /// <summary>
        /// Converts a color into a single Int32 value, given alpha channel setting.
        /// </summary>
        /// <param param name="color">Color</param>
        /// <returns>Int32 representation of the color</returns>
        public static int ColorToInt32(System.Windows.Media.Color color)
        {
            return color.A << 24 | color.R << 16 | color.G << 8 | color.B << 0;
        }

        /// <summary>
        /// Converts a color represented as Int32 value to System.Windows.Media.Color.
        /// </summary>
        /// <param name="color">Color as Int32</param>
        /// <returns>Color as System.Windows.Media.</returns>
        public static System.Windows.Media.Color Int32ToColor(int color)
        {
            return System.Windows.Media.Color.FromArgb((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color);
        }
        
        /// <summary>
        /// Paints the pixels with their location specified in pixels array to the given color.
        /// </summary>
        /// <param name="background">Image pixel array to be changed</param>
        /// <param name="pixels">Locations of pixels to be painted</param>
        /// <param name="color">Color to be used when painting</param>
        /// <returns>Resulting picture</returns>
        public static int[] Impose(int[] background, int[] pixels, int color)
        {
            Parallel.For(0, pixels.Length, i =>
              {
                  background[pixels[i]] = color;
              });

            return background;
        }
    }
}
