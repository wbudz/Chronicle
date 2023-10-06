using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    /// <summary>
    /// Constains data defining color scale used by a map mode.
    /// </summary>
    [Serializable]
    public class Colorscale
    {
        /// <summary>
        /// Simple rainbow gradient for land-based provinces
        /// </summary>
        public GradientDefinition LandGradient
        {
            get; private set;
        }

        /// <summary>
        /// Simple rainbow gradient for water-based provinces
        /// </summary>
        public GradientDefinition WaterGradient
        {
            get; private set;
        }

        /// <summary>
        /// Color definition where each data entry gets a separate color
        /// </summary>
        public DiscretesDefinition DatakeysColors
        {
            get; private set;
        }

        /// <summary>
        /// Color definition where each data entry gets a separate color which can be shown at varying intensity
        /// </summary>
        public MultiGradientDefinition MultiGradient
        {
            get; private set;
        }

        public bool IsRandom {
            get
            {
                return LandGradient.IsRandom || WaterGradient.IsRandom || DatakeysColors.IsRandom || MultiGradient.IsRandom;
            }
        }

        #region Constructors

        public Colorscale()
        {
            LandGradient = new GradientDefinition(false);
            WaterGradient = new GradientDefinition(true);
            DatakeysColors = new DiscretesDefinition();
            MultiGradient = new MultiGradientDefinition();
        }

        public Colorscale(Colorscale source)
        {
            this.LandGradient = new GradientDefinition(source.LandGradient);
            this.WaterGradient = new GradientDefinition(source.WaterGradient);
            this.DatakeysColors = new DiscretesDefinition(source.DatakeysColors);
            this.MultiGradient = new MultiGradientDefinition(source.MultiGradient);
        }

        public Colorscale(_Color low, _Color mid, _Color high, float exp) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), mid.GetColor(), high.GetColor(), exp);
        }

        public Colorscale(_Color low, _Color mid, _Color high) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), mid.GetColor(), high.GetColor(), 1);
        }

        public Colorscale(_Color low, _Color high, float exp) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), new Color(), high.GetColor(), exp);
        }

        public Colorscale(_Color low, _Color high) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), new Color(), high.GetColor(), 1);
        }

        public Colorscale(_Color color, float exp) : this()
        {
            LandGradient = new GradientDefinition(CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor), new Color(), color.GetColor(), exp);
        }

        public Colorscale(_Color color) : this()
        {
            LandGradient = new GradientDefinition(CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor), new Color(), color.GetColor(), 1);
        }

        public Colorscale(int low, int high, float exp) : this()
        {
            LandGradient = new GradientDefinition(CEBitmap.Bitmap.Int32ToColor(low), new Color(), CEBitmap.Bitmap.Int32ToColor(high), exp);
        }

        public Colorscale(_Color low, _Color mid, _Color high, float exp, _Color lowW, _Color midW, _Color highW, float expW) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), mid.GetColor(), high.GetColor(), exp);
            WaterGradient = new GradientDefinition(lowW.GetColor(), midW.GetColor(), highW.GetColor(), expW);
        }

        public Colorscale(_Color low, _Color mid, _Color high, _Color lowW, _Color midW, _Color highW) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), mid.GetColor(), high.GetColor(), 1);
            WaterGradient = new GradientDefinition(lowW.GetColor(), midW.GetColor(), highW.GetColor(), 1);
        }

        public Colorscale(_Color low, _Color high, float exp, _Color lowW, _Color highW, float expW) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), new Color(), high.GetColor(), exp);
            WaterGradient = new GradientDefinition(lowW.GetColor(), new Color(), highW.GetColor(), expW);
        }

        public Colorscale(_Color low, _Color high, _Color lowW, _Color highW) : this()
        {
            LandGradient = new GradientDefinition(low.GetColor(), new Color(), high.GetColor(), 1);
            WaterGradient = new GradientDefinition(lowW.GetColor(), new Color(), highW.GetColor(), 1);
        }

        public Colorscale(DynamicDefs ext, string identifier, float exp) : this()
        {
            DatakeysColors = new DiscretesDefinition(ext, identifier);
        }

        public Colorscale(DynamicDefs ext, string identifier) : this()
        {
            DatakeysColors = new DiscretesDefinition(ext, identifier);
            MultiGradient = new MultiGradientDefinition(ext, identifier);
        }

        #endregion

        public void AddColor(string id, _Color color)
        {
            DatakeysColors.AddColor(id, color);
            MultiGradient.AddColor(id, color);
        }

        public int GetColor(bool water, double value, double min, double max)
        {
            if (!water && LandGradient == null) return Core.Settings.LandColor;
            if (water && WaterGradient == null) return Core.Settings.WaterColor;

            if (double.IsNaN(value) || double.IsNaN(min) || double.IsNaN(max) || max <= min) return water ? WaterGradient.GetColor(0) : LandGradient.GetColor(0);
            if (value < min) value = min;
            if (value > max) value = max;

            // Calculate the proper color
            double x = (value - min) / (max - min);
            return water ? WaterGradient.GetColor(x) : LandGradient.GetColor(x);
        }

        //public int[] GetColor(bool water, string datakey)
        //{
        //    if (!water && DatakeysColors == null) return new int[] { Core.Settings.LandColor };
        //    if (water && DatakeysColors == null) return new int[] { Core.Settings.WaterColor };

        //    if (datakey == null || datakey == "") return new int[] { water ? Core.Settings.WaterColor : Core.Settings.EmptyColor };
        //    return new int[] { DatakeysColors.GetColor(water, datakey) };
        //}

        public int GetColor(bool water, string datakey)
        {
            if (!water && DatakeysColors == null) return Core.Settings.LandColor;
            if (water && DatakeysColors == null) return Core.Settings.WaterColor;

            if (datakey == null || datakey == "") return water ? Core.Settings.WaterColor : Core.Settings.EmptyColor;

            if (datakey == "*selected")
                return Core.Settings.SelectedCountryColor;
            else
                return DatakeysColors.GetColor(water, datakey);
        }

        public int GetColor(bool water, double value, double min, double max, string datakey)
        {
            if (water) return Core.Settings.WaterColor; // Multivalue colorscales have no water coloring
            if (MultiGradient == null) return Core.Settings.LandColor;

            if (double.IsNaN(value) || double.IsNaN(min) || double.IsNaN(max)) return water ? Core.Settings.WaterColor : Core.Settings.LandColor;
            if (value < min) value = min;
            if (value > max) value = max;

            if (datakey == null || datakey == "") return water ? Core.Settings.WaterColor : Core.Settings.EmptyColor;

            // Calculate the proper color
            double x = (value - min) / (max - min);

            return MultiGradient.GetColor(datakey, x);
        }

        #region Mean color calculation

        public static int CalculateMeanColor(int color1, double weigh1, int color2, double weigh2)
        {
            return CalculateMeanColor(new int[] { color1, color2 }, new double[] { weigh1, weigh2 });
        }

        public static int CalculateMeanColor(Color color1, double weigh1, Color color2, double weigh2)
        {
            return CalculateMeanColor(new Color[] { color1, color2 }, new double[] { weigh1, weigh2 });
        }

        public static int CalculateMeanColor(int[] colors, double[] weighs)
        {
            if (weighs.Length == 0 || colors.Length == 0) return Core.Settings.EmptyColor;

            double r = 0;
            double g = 0;
            double b = 0;
            for (int i = 0; i < Math.Min(weighs.Length, colors.Length); i++)
            {
                Color clr = CEBitmap.Bitmap.Int32ToColor(colors[i]);
                r += clr.R * weighs[i];
                g += clr.G * weighs[i];
                b += clr.B * weighs[i];
            }
            double sum = weighs.Sum();

            return CEBitmap.Bitmap.ColorToInt32((byte)(r / sum), (byte)(g / sum), (byte)(b / sum));
        }

        public static int CalculateMeanColor(Color[] colors, double[] weighs)
        {
            if (weighs.Length == 0 || colors.Length == 0) return Core.Settings.EmptyColor;

            double r = 0;
            double g = 0;
            double b = 0;
            for (int i = 0; i < Math.Min(weighs.Length, colors.Length); i++)
            {
                r += colors[i].R * weighs[i];
                g += colors[i].G * weighs[i];
                b += colors[i].B * weighs[i];
            }
            double sum = weighs.Sum();

            return CEBitmap.Bitmap.ColorToInt32((byte)(r / sum), (byte)(g / sum), (byte)(b / sum));
        }

        #endregion
    }
}

