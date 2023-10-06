using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    public static class Blend
    {
        public static int BlendColors(int fg, int bg)
        {
            float fga = (byte)(fg >> 24) / 255f / 2f;
            byte r = (byte)((byte)(fg >> 16) + ((byte)(bg >> 16) - (byte)(fg >> 16)) * fga);
            byte g = (byte)((byte)(fg >> 8) + ((byte)(bg >> 8) - (byte)(fg >> 8)) * fga);
            byte b = (byte)((byte)(fg) + ((byte)(bg) - (byte)(fg)) * fga);
            return 255 << 24 | r << 16 | g << 8 | b;
        }

        public static int Brightness(int color, float factor)
        {
            return 255 << 24 | ((byte)((byte)(color >> 16) * factor) << 16 | (byte)((byte)(color >> 8) * factor) << 8 | (byte)((byte)(color) * factor));
        }

    }
}
