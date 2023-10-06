using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    [Serializable]
    public class GradientDefinition
    {
        List<KeyValuePair<float, int>> def = new List<KeyValuePair<float, int>>();
        float exponent;

        int[] rainbow = new int[256];

        public bool IsEmpty { get { return def.Count == 0; } }

        public bool IsRandom { get { return def.Count > 0; } }

        public GradientDefinition(GradientDefinition source)
        {
            this.def = new List<KeyValuePair<float, int>>(source.def);
            this.exponent = source.exponent;
            Array.Copy(source.rainbow, this.rainbow, this.rainbow.Length);
        }

        public GradientDefinition(Color low, Color mid, Color high, float exp)
        {
            def.Clear();
            if (low.A != 0) def.Add(new KeyValuePair<float, int>(0, CEBitmap.Bitmap.ColorToInt32(low)));
            if (mid.A != 0) def.Add(new KeyValuePair<float, int>(0.5f, CEBitmap.Bitmap.ColorToInt32(mid)));
            if (high.A != 0) def.Add(new KeyValuePair<float, int>(1, CEBitmap.Bitmap.ColorToInt32(high)));
            exponent = exp;

            PrecalculateColors();
        }

        public GradientDefinition(bool water)
        {
            def.Clear();
            exponent = 1;
            int c = water ? CEBitmap.Bitmap.MakeOpaque(Core.Settings.WaterColor) : CEBitmap.Bitmap.MakeOpaque(Core.Settings.LandColor);
            for (int i = 0; i < rainbow.Length; i++)
            {
                rainbow[i] = c;
            }
        }

        public int GetColor(double x)
        {
            x = Math.Min(x, 1);
            x = Math.Max(x, 0);
            if (exponent > 0 && exponent % 2 == 0)
                x = -Math.Pow((x - 1), exponent) + 1;
            if (exponent < 0 && exponent % 2 == 0)
                x = Math.Pow(x, -exponent);

            // Align the result to closest 1/255
            return rainbow[(int)Math.Round(x * 255f)];
        }

        private void PrecalculateColors()
        {
            if (def.Count > 1)
            {
                for (int i = 1; i < def.Count; i++)
                {
                    float lPoint = def[i - 1].Key;
                    float hPoint = def[i].Key;
                    float diff = Math.Max(hPoint - lPoint, 0.001f); // so it's never 0
                    Color lColor = CEBitmap.Bitmap.Int32ToColor(def[i - 1].Value);
                    Color hColor = CEBitmap.Bitmap.Int32ToColor(def[i].Value);

                    int rSpan = hColor.R - lColor.R;
                    int gSpan = hColor.G - lColor.G;
                    int bSpan = hColor.B - lColor.B;

                    int lIndex = (int)(lPoint * 256);
                    int hIndex = (int)(hPoint * 256);
                    float step = diff / (hIndex - lIndex);
                    float v = lPoint;

                    for (int j = lIndex; j < hIndex; j++)
                    {
                        rainbow[j] = CEBitmap.Bitmap.ColorToInt32(255,
                          (byte)(lColor.R + ((v - lPoint) / diff) * rSpan),
                          (byte)(lColor.G + ((v - lPoint) / diff) * gSpan),
                          (byte)(lColor.B + ((v - lPoint) / diff) * bSpan));
                        v += step;
                    }
                }
            }
            else if (def.Count == 1)
            {
                for (int i = 0; i < rainbow.Length; i++)
                {
                    rainbow[i] = def[0].Value;
                }
            }
            else
            {
                for (int i = 0; i < rainbow.Length; i++)
                {
                    rainbow[i] = Core.Settings.EmptyColor;
                }
            }
        }

        public GradientStopCollection GetGradientStops()
        {
            GradientStopCollection output = new GradientStopCollection();
            for (int i = 0; i < 256; i++)
            {
                double x = i / 255f;
                if (exponent < 0 && exponent % 2 == 0)
                    x = -Math.Pow((x - 1), exponent) + 1;
                if (exponent > 0 && exponent % 2 == 0)
                    x = Math.Pow(x, -exponent);
                output.Add(new GradientStop(CEBitmap.Bitmap.Int32ToColor(rainbow[i]), x));
            }
            return output;
        }

        public Color GetLowColor()
        {
            try
            {
                return CEBitmap.Bitmap.Int32ToColor(def[0].Value);
            }
            catch
            {
                return CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor);
            }
        }

        public Color GetMidColor()
        {
            try
            {
                return def.Count > 2 ? CEBitmap.Bitmap.Int32ToColor(def[1].Value) : Color.FromArgb(0, 0, 0, 0);
            }
            catch
            {
                return CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor);
            }
        }

        public Color GetHighColor()
        {
            try
            {
                return CEBitmap.Bitmap.Int32ToColor(def.Last().Value);
            }
            catch
            {
                return CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor);
            }
        }

        public int GetExponentIndex()
        {
            switch ((int)exponent)
            {
                case -128: return 0;
                case -64: return 1;
                case -32: return 2;
                case -16: return 3;
                case -8: return 4;
                case -4: return 5;
                case -2: return 6;
                case 1: return 7;
                case 2: return 8;
                case 4: return 9;
                case 8: return 10;
                case 16: return 11;
                case 32: return 12;
                case 64: return 13;
                case 128: return 14;
                default: return 7;
            }
        }

        public static float GetExponentFromIndex(int index)
        {
            switch (index)
            {
                case 0: return -128;
                case 1: return -64;
                case 2: return -32;
                case 3: return -16;
                case 4: return -8;
                case 5: return -4;
                case 6: return -2;
                case 7: return 1;
                case 8: return 2;
                case 9: return 4;
                case 10: return 8;
                case 11: return 16;
                case 12: return 32;
                case 13: return 64;
                case 14: return 128;
                default: return 1;
            }
        }

        public void SetColors(params Color[] color)
        {
            def.Clear();
            for (int i = 0; i < color.Length; i++)
            {
                def.Add(new KeyValuePair<float, int>(i / (float)(color.Length - 1), CEBitmap.Bitmap.ColorToInt32(color[i])));
            }
        }

        public void SetExponent(float exp)
        {
            exponent = exp;
        }

        public void Refresh()
        {
            PrecalculateColors();
        }
    }
}
