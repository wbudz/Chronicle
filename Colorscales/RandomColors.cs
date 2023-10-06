using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Chronicle
{
    public static class RandomColors
    {
        static int[] landColors;
        static int landIndex = -1;
        static int[] waterColors;
        static int waterIndex = -1;

        public static int GetRandomColor(bool water)
        {
            if (landColors == null || waterColors == null)
            {
                GenerateUniqueColors();
                Randomize();
            }

            if (landIndex == landColors.Length - 1) landIndex = -1;
            if (waterIndex == waterColors.Length - 1) waterIndex = -1;
            return water ? waterColors[++waterIndex] : landColors[++landIndex];
        }

        public static void Randomize()
        {
            landColors = landColors.OrderBy(x => Core.RNG.Next()).ToArray();
            waterColors = waterColors.OrderBy(x => Core.RNG.Next()).ToArray();
            landIndex = -1;
            waterIndex = -1;
        }

        private static void GenerateUniqueColors()
        {
            List<int> landColorsList = new List<int>();
            List<int> waterColorsList = new List<int>();

            int factor = 32;

            for (int r = 0; r < 256; r += factor)
            {
                for (int g = 0; g < 256; g += factor)
                {
                    for (int b = 0; b < 256; b += factor)
                    {
                        if (r < 48 && g < 48 && b < 48) continue;
                        if (r > 212 && g > 212 && b > 212) continue;
                        if ((r > 112 && r < 138) || (g > 112 && g < 138) || (b > 112 && b < 138)) continue;

                        if (b > r && b > g)
                            waterColorsList.Add(CEBitmap.Bitmap.ColorToInt32((byte)r, (byte)g, (byte)b));
                        else
                            landColorsList.Add(CEBitmap.Bitmap.ColorToInt32((byte)r, (byte)g, (byte)b));
                    }
                }
            }

            landColors = landColorsList.ToArray();
            waterColors = waterColorsList.ToArray();
        }
    }
}
