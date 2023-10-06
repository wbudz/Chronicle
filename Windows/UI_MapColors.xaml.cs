using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Chronicle
{
    /// <summary>
    /// Interaction logic for UI_Debug.xaml
    /// </summary>
    [Serializable]
    public partial class UI_MapColors : UserControl
    {
        bool refresh = false;

        public UI_MapColors()
        {
            InitializeComponent();
        }

        public void DisplayColorscale(Colorscale cs, double min, double max)
        {
            refresh = true;
            if (!cs.LandGradient.IsEmpty || !cs.WaterGradient.IsEmpty)
            {
                GradientStopCollection gdef;
                // Land gradient
                if (!cs.LandGradient.IsEmpty)
                {
                    gdef = cs.LandGradient.GetGradientStops();
                    lMapModeMinColor.Text = min.ToString("N2");
                    lMapModeMaxColor.Text = max.ToString("N2");
                    lMapModeMinColor.Visibility = Double.IsNaN(min) ? Visibility.Collapsed : Visibility.Visible;
                    lMapModeMaxColor.Visibility = Double.IsNaN(max) ? Visibility.Collapsed : Visibility.Visible;
                    rMapModeColorscale.Fill = new LinearGradientBrush(gdef, 0);

                    Color c = cs.LandGradient.GetLowColor();
                    cpLowLandColorscale.SelectedColor = c;
                    c = cs.LandGradient.GetMidColor();
                    if (c.A > 0)
                    {
                        cpMidLandColorscale.SelectedColor = c;
                        cIntermediateColorLandColorscale.IsChecked = true;
                        cpMidLandColorscale.IsEnabled = true;
                    }
                    else
                    {
                        cpMidLandColorscale.SelectedColor = Color.FromRgb(255, 255, 255);
                        cIntermediateColorLandColorscale.IsChecked = false;
                        cpMidLandColorscale.IsEnabled = true;
                    }
                    c = cs.LandGradient.GetHighColor();
                    cpHighLandColorscale.SelectedColor = c;

                    cLandColorscaleExponentiality.SelectedIndex = cs.LandGradient.GetExponentIndex();

                }

                // Water gradient
                if (!cs.WaterGradient.IsEmpty)
                {
                    gdef = cs.WaterGradient.GetGradientStops();
                    lMapModeMinColorW.Text = min.ToString("N2");
                    lMapModeMaxColorW.Text = max.ToString("N2");
                    lMapModeMinColorW.Visibility = Double.IsNaN(min) ? Visibility.Collapsed : Visibility.Visible;
                    lMapModeMaxColorW.Visibility = Double.IsNaN(max) ? Visibility.Collapsed : Visibility.Visible;
                    rMapModeColorscaleW.Fill = new LinearGradientBrush(gdef, 0);

                    Color c = cs.WaterGradient.GetLowColor();
                    cpLowWaterColorscale.SelectedColor = c;
                    c = cs.WaterGradient.GetMidColor();
                    if (c.A > 0)
                    {
                        cpMidWaterColorscale.SelectedColor = c;
                        cIntermediateColorWaterColorscale.IsChecked = true;
                        cpMidWaterColorscale.IsEnabled = true;
                    }
                    else
                    {
                        cpMidWaterColorscale.SelectedColor = Color.FromRgb(255, 255, 255);
                        cIntermediateColorWaterColorscale.IsChecked = false;
                        cpMidWaterColorscale.IsEnabled = true;
                    }
                    c = cs.WaterGradient.GetHighColor();
                    cpHighWaterColorscale.SelectedColor = c;

                    cWaterColorscaleExponentiality.SelectedIndex = cs.WaterGradient.GetExponentIndex();
                }

                bResetColors.Visibility = (cs.LandGradient.IsEmpty && cs.WaterGradient.IsEmpty) ? Visibility.Collapsed : Visibility.Visible;
                gLandProvinces.Visibility = cs.LandGradient.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
                gWaterProvinces.Visibility = cs.WaterGradient.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
                gDiscreteColors.Visibility = Visibility.Collapsed;
                refresh = false;
                return;
            }
            if (!cs.DatakeysColors.IsEmpty || !cs.MultiGradient.IsEmpty)
            {
                refresh = true;                             
                           
                if (!cs.DatakeysColors.IsEmpty)
                {
                    // Check for possible reuse
                    if (cs.DatakeysColors.CountNonEmpty == pDiscreteColors.Children.Count)
                    {
                        cs.DatakeysColors.UpdateColorLegend(pDiscreteColors.Children);
                    }
                    else
                    {
                        var colors = cs.DatakeysColors.GenerateColorLegend();
                        pDiscreteColors.Children.Clear();
                        foreach (var color in colors)
                        {
                            pDiscreteColors.Children.Add(color);
                        }
                    }
                }
                else if (!cs.MultiGradient.IsEmpty)
                {
                    // Check for possible reuse
                    if (cs.DatakeysColors.CountNonEmpty == pDiscreteColors.Children.Count)
                    {
                        cs.MultiGradient.DatakeysColors.UpdateColorLegend(pDiscreteColors.Children);
                    }
                    else
                    {
                        var colors = cs.MultiGradient.DatakeysColors.GenerateColorLegend();
                        pDiscreteColors.Children.Clear();
                        foreach (var color in colors)
                        {
                            pDiscreteColors.Children.Add(color);
                        }
                    }
                }

                bResetColors.Visibility = Visibility.Visible;
                gLandProvinces.Visibility = Visibility.Collapsed;
                gWaterProvinces.Visibility = Visibility.Collapsed;
                gDiscreteColors.Visibility = Visibility.Visible;
                refresh = false;
                return;
            }

            refresh = true;
            bResetColors.Visibility = Visibility.Collapsed;
            gLandProvinces.Visibility = Visibility.Collapsed;
            gWaterProvinces.Visibility = Visibility.Collapsed;
            gDiscreteColors.Visibility = Visibility.Collapsed;
            refresh = false;
        }

        public void AlterColorscale()
        {
            float exponent = GradientDefinition.GetExponentFromIndex(cLandColorscaleExponentiality.SelectedIndex);
            Colorscale cs = Core.Data.Tables.GetColorscale(Core.UI_Mapview.CurrentTable.Name);

            if (!cs.LandGradient.IsEmpty || !cs.WaterGradient.IsEmpty)
            {
                // Land gradient
                if (!cs.LandGradient.IsEmpty)
                {
                    if (cIntermediateColorLandColorscale.IsChecked == true)
                    {
                        cs.LandGradient.SetColors(cpLowLandColorscale.SelectedColor.Value, cpMidLandColorscale.SelectedColor.Value, cpHighLandColorscale.SelectedColor.Value);
                    }
                    else
                    {
                        cs.LandGradient.SetColors(cpLowLandColorscale.SelectedColor.Value, cpHighLandColorscale.SelectedColor.Value);
                    }
                    cs.LandGradient.SetExponent(GradientDefinition.GetExponentFromIndex(cLandColorscaleExponentiality.SelectedIndex));
                    cs.LandGradient.Refresh();

                    var gdef = cs.LandGradient.GetGradientStops();
                    rMapModeColorscale.Fill = new LinearGradientBrush(gdef, 0);
                }
                // Water gradient
                if (!cs.WaterGradient.IsEmpty)
                {
                    if (cIntermediateColorWaterColorscale.IsChecked == true)
                    {
                        cs.WaterGradient.SetColors(cpLowWaterColorscale.SelectedColor.Value, cpMidWaterColorscale.SelectedColor.Value, cpHighWaterColorscale.SelectedColor.Value);
                    }
                    else
                    {
                        cs.WaterGradient.SetColors(cpLowWaterColorscale.SelectedColor.Value, cpHighWaterColorscale.SelectedColor.Value);
                    }
                    cs.WaterGradient.SetExponent(GradientDefinition.GetExponentFromIndex(cWaterColorscaleExponentiality.SelectedIndex));
                    cs.WaterGradient.Refresh();

                    var gdef = cs.WaterGradient.GetGradientStops();
                    rMapModeColorscaleW.Fill = new LinearGradientBrush(gdef, 0);
                }
            }
            else if (!cs.DatakeysColors.IsEmpty || !cs.MultiGradient.IsEmpty)
            {
            }

            Core.UI_Mapview.Refresh();
        }

        private void cpColorscale_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (refresh) return;
            AlterColorscale();
        }

        private void cExponentiality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (refresh) return;
            AlterColorscale();
        }

        private void cColorscale_Checked(object sender, RoutedEventArgs e)
        {
            if (refresh) return;
            AlterColorscale();
        }

        private void bResetColors_Click(object sender, RoutedEventArgs e)
        {
            var cs = Core.Data.Tables.ResetColorscale(Core.UI_Mapview.CurrentTable.Name);
            Core.UI_Mapview.Refresh();
        }
    }
}
