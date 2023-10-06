using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chronicle
{
    [Serializable]
    public class DiscretesDefinition
    {
        Dictionary<string, int> colors = new Dictionary<string, int>();
        Dictionary<string, int> rcolors = new Dictionary<string, int>();

        public bool IsEmpty { get { return colors.Count == 0 && rcolors.Count == 0; } }

        public bool IsRandom { get { return colors.Count == 0 && rcolors.Count > 0; } }

        public int Count { get { return colors.Count + rcolors.Count; } }

        public int CountNonEmpty { get { return colors.Count(x => x.Value != 0) + rcolors.Count(x => x.Value != 0); } }

        public DiscretesDefinition()
        {
            rcolors.Clear();
            colors.Clear();
        }

        public DiscretesDefinition(DiscretesDefinition source)
        {
            colors = new Dictionary<string, int>(source.colors);
            rcolors = new Dictionary<string, int>(source.rcolors);
        }

        public DiscretesDefinition(DynamicDefs ext, string identifier) : base()
        {
            colors.Clear();

            Dictionary<string, int> colorsdef = null;
            if (identifier != null)
                colorsdef = ext.GetDatakeysColors(identifier);

            if (colorsdef != null)
            {
                for (ushort i = 0; i < colorsdef.Count; i++)
                {
                    colors.Add(colorsdef.Keys.ElementAt(i), colorsdef.Values.ElementAt(i));
                }
            }
        }

        public void AddColor(string key, _Color color)
        {
            if (colors.ContainsKey(key))
                colors[key] = CEBitmap.Bitmap.ColorToInt32(color.GetColor());
            else
                colors.Add(key, CEBitmap.Bitmap.ColorToInt32(color.GetColor()));
        }

        public int GetColor(bool water, string datakey)
        {
            int output;
            if (colors.TryGetValue(datakey, out output) && output != 0)
            {
                return output;
            }
            else if (rcolors.TryGetValue(datakey, out output) && output != 0)
            {
                return output;
            }
            else
            {
                output = RandomColors.GetRandomColor(water);
                rcolors.Add(datakey, output);
                return output;
            }
        }

        public IEnumerable<StackPanel> GenerateColorLegend()
        {
            for (int i = 0; i < colors.Count; i++)
            {
                if (colors.Values.ElementAt(i) == 0) continue;

                StackPanel sp = new StackPanel();
                sp.Margin = new System.Windows.Thickness(2);
                sp.Orientation = Orientation.Horizontal;
                Rectangle rect = new Rectangle();
                rect.Width = 50;
                rect.Height = 20;
                rect.Fill = new SolidColorBrush(CEBitmap.Bitmap.Int32ToColor(colors.Values.ElementAt(i)));
                sp.Children.Add(rect);
                TextBlock tb = new TextBlock();
                tb.Margin = new System.Windows.Thickness(10, 0, 0, 0);
                tb.Text = colors.Keys.ElementAt(i);
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                sp.Children.Add(tb);

                yield return sp;
            }

            for (int i = 0; i < rcolors.Count; i++)
            {
                if (rcolors.Values.ElementAt(i) == 0) continue;

                StackPanel sp = new StackPanel();
                sp.Margin = new System.Windows.Thickness(2);
                sp.Orientation = Orientation.Horizontal;
                Rectangle rect = new Rectangle();
                rect.Width = 50;
                rect.Height = 20;
                rect.Fill = new SolidColorBrush(CEBitmap.Bitmap.Int32ToColor(rcolors.Values.ElementAt(i)));
                sp.Children.Add(rect);
                TextBlock tb = new TextBlock();
                tb.Margin = new System.Windows.Thickness(10, 0, 0, 0);
                tb.Text = rcolors.Keys.ElementAt(i);
                sp.Children.Add(tb);

                yield return sp;
            }
        }

        public void UpdateColorLegend(UIElementCollection uiColors)
        {
            int index = -1;

            for (int i = 0; i < colors.Count; i++)
            {
                if (++index >= uiColors.Count) return;
                if (colors.Values.ElementAt(i) == 0) continue;
                ((uiColors[index] as StackPanel).Children[0] as Rectangle).Fill = new SolidColorBrush(CEBitmap.Bitmap.Int32ToColor(colors.Values.ElementAt(i)));
                ((uiColors[index] as StackPanel).Children[1] as TextBlock).Text = colors.Keys.ElementAt(i);
            }

            for (int i = 0; i < rcolors.Count; i++)
            {
                if (++index >= uiColors.Count) return;
                if (rcolors.Values.ElementAt(i) == 0) continue;
                ((uiColors[index] as StackPanel).Children[0] as Rectangle).Fill = new SolidColorBrush(CEBitmap.Bitmap.Int32ToColor(rcolors.Values.ElementAt(i)));
                ((uiColors[index] as StackPanel).Children[1] as TextBlock).Text = rcolors.Keys.ElementAt(i);
            }
        }

        public bool ContainsRandomColors()
        {
            return rcolors.Count > 0;
        }
    }
}

