using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chronicle
{
    [Serializable]
    public class MultiGradientDefinition
    {
        float exponent = 1;
        public DiscretesDefinition DatakeysColors
        {
            get; private set;
        }
        Dictionary<string, GradientDefinition> Gradients = new Dictionary<string, GradientDefinition>();

        public bool IsEmpty { get { return DatakeysColors.IsEmpty; } }

        public bool IsRandom { get { return DatakeysColors.IsRandom; } }

        public MultiGradientDefinition(MultiGradientDefinition source)
        {
            DatakeysColors = new DiscretesDefinition(source.DatakeysColors);
            this.exponent = source.exponent;
            this.Gradients = new Dictionary<string, GradientDefinition>(source.Gradients);
        }

        public MultiGradientDefinition(DynamicDefs ext, string identifier, float exp)
        {
            DatakeysColors = new DiscretesDefinition(ext, identifier);
            exponent = exp;
        }

        public MultiGradientDefinition(DynamicDefs ext, string identifier) : this(ext, identifier, 1)
        { }

        public MultiGradientDefinition(float exp)
        {
            DatakeysColors = new DiscretesDefinition();
            exponent = exp;
        }

        public MultiGradientDefinition() : this(1)
        { }

        public void AddColor(string key, _Color color)
        {
            DatakeysColors.AddColor(key, color);
        }

        public int GetColor(string datakey, double x)
        {
            if (!Gradients.ContainsKey(datakey))
            {
                int color = DatakeysColors.GetColor(false, datakey);
                Gradients.Add(datakey, new GradientDefinition(CEBitmap.Bitmap.Int32ToColor(Core.Settings.EmptyColor), new Color(), CEBitmap.Bitmap.Int32ToColor(color), exponent));
            }

            GradientDefinition gradient = Gradients[datakey];
            return gradient.GetColor(x);
        }
    }
}
