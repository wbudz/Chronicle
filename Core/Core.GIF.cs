using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public static partial class Core
    {
        public static class Animated
        {
            public static void SaveImage(string path, ModeInterfaceElement mode, bool prioritize)
            {
                // Generate frames
                GIF gif = new GIF(path, Data.Defs.Map.IDMap, Data.Defs.Map.PosMap, 100, Data.Defs.Map.Width, Data.Defs.Map.Height, mode.Caption, prioritize);
                var timepoints = Data.Tables.GetTimepoints();
                for (int i = 0; i < timepoints.Count; i++)
                {
                    Table t = Data.Tables.Select(timepoints[i], mode.Name);
                    double min; double max; double[] values;
                    int[][] colors = null;
                    Core.Dispatch.Run(() =>
                    {
                        colors = t.GetColors(
                            (MultivalueSetting)(Core.UI_Mapview.cMultiValueMode.SelectedIndex + 1),
                            ((Core.UI_Mapview.cMultiValueKeys.SelectedItem) ?? "").ToString(),
                            Data.IndexToTag(Core.UI_Mapview.CurrentCountry),
                            (MultivalueColorSetting)(Core.UI_Mapview.cMultiValueColor.SelectedIndex + 1),
                            new int[] { Core.Settings.EmptyColor, Core.Settings.LandColor, Core.Settings.WaterColor, Core.Settings.SelectedCountryColor,
                            Core.UI_Mapview.cAbsoluteColor.IsChecked==false?Core.Settings.DefaultColor:0},
                            out min, out max, out values);
                    });
                    gif.AddFrame(colors, timepoints[i]);
                }
                gif.Write((int)Math.Pow(2, Core.Settings.GIFZoom));
            }
        }

        class GIF
        {
            string path;
            Guid guid;
            ushort[] idMap;
            int[] posMap;

            List<int> palette = new List<int>();
            List<int> fixedColors = new List<int>();
            List<int> priorityColors = new List<int>();

            Dictionary<int, int> paletteTranslations = new Dictionary<int, int>();
            List<int[][]> colormaps = new List<int[][]>();
            List<int[]> overlays = new List<int[]>();
            List<GameDate> timepoints = new List<GameDate>();
            string mapmode;

            static int white = CEBitmap.Bitmap.ColorToInt32(255, 255, 255);
            static int black = CEBitmap.Bitmap.ColorToInt32(0, 0, 0);
            static int land = Core.Settings.LandColor;
            static int water = Core.Settings.WaterColor;

            int width;
            int height;
            int originalWidth;
            int originalHeight;
            int captionHeight = Core.Settings.GIFDisplayCaptionBar ? 40 : 0;
            int delay = 100;

            bool prioritize;

            public GIF(string path, ushort[] idMap, int[] posMap, int delay, int width, int height, string mapmode, bool prioritize)
            {
                this.path = path;
                this.guid = Guid.NewGuid();
                this.idMap = idMap;
                this.posMap = posMap;
                this.originalWidth = width;
                this.originalHeight = height;
                this.mapmode = mapmode;
                this.prioritize = prioritize;

                fixedColors.Add(white); // 0
                fixedColors.Add(black); // 1
                fixedColors.Add(land); // 2
                fixedColors.Add(water); // 3
                palette.AddRange(fixedColors);
            }

            public void AddFrame(int[][] colors, GameDate timepoint)
            {
                timepoints.Add(timepoint);
                colormaps.Add(colors);
                for (int i = 0; i < colors.GetLength(0); i++)
                {
                    palette.AddRange(colors[i]);
                }
            }

            public void Prioritize(List<int> distinctColors, List<int[][]> colorHeap)
            {
                List<KeyValuePair<int, int>> weighs = new List<KeyValuePair<int, int>>();
                for (int i = 0; i < distinctColors.Count; i++)
                {
                    weighs.Add(new KeyValuePair<int, int>(distinctColors[i], colorHeap.AsParallel().Sum(x => x.Count(y => y[0] == distinctColors[i]))));
                }

                priorityColors.AddRange(weighs.OrderByDescending(x => x.Value).Take(128).Select(y => y.Key));
                palette.AddRange(priorityColors);
            }

            public void Write(int sizeReduction)
            {
                Core.Dispatch.DisplayProgress("Preparing to export animated map data...");

                palette = palette.Distinct().ToList();

                if (palette.Count > 256)
                {
                    if (prioritize) Prioritize(palette, colormaps);
                    ConstructDictionary();
                }

                originalWidth = Data.Defs.Map.Width;
                originalHeight = Data.Defs.Map.Height;
                width = originalWidth / sizeReduction;
                height = originalHeight / sizeReduction;

                if (Core.Settings.GIFDisplayCaptionBar) SetupOverlays(sizeReduction);

                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    WriteString(fs, "GIF89a");

                    for (int i = 0; i < colormaps.Count; i++)
                    {
                        Core.Dispatch.DisplayProgress("Exporting animated map data...", i / (float)colormaps.Count);
                        if (i == 0)
                        {
                            WriteLSD(fs); // logical screen descriptior
                            WritePalette(fs); // global color table
                                              // use NS app extension to indicate reps
                            WriteNetscapeExt(fs);
                        }
                        WriteGraphicCtrlExt(fs); // write graphic control extension
                        WriteImageDesc(fs); // image descriptor
                        WritePixels(fs, PreparePixels(sizeReduction, i)); // encode and write pixel data
                    }

                    fs.WriteByte(0x3b); // gif trailer
                    fs.Flush();
                    fs.Close();
                }
            }

            private void SetupOverlays(int sizeReduction)
            {
                FormattedText textMapmode = new FormattedText(mapmode, new System.Globalization.CultureInfo("en-us"), FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, new FontStretch()), 16, Brushes.Black);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Chronicle;component/Icons/chronicle_tag.png", UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageSource imgTagline = bitmap;

                for (int i = 0; i < timepoints.Count; i++)
                {
                    FormattedText textDate = new FormattedText(timepoints[i].GetString("yyy.MM.dd"),
                        new System.Globalization.CultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Heavy, new FontStretch()),
                        20,
                        Brushes.Black);

                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    if (Core.Settings.GIFDisplayDate) drawingContext.DrawText(textDate, new Point(0, -2));
                    if (Core.Settings.GIFDisplayMapmode) drawingContext.DrawText(textMapmode, new Point(0, 20));
                    if (Core.Settings.GIFDisplayLogo) drawingContext.DrawImage(imgTagline, new Rect(width - 160, 0, 160, captionHeight));
                    drawingContext.Close();

                    // Rect sourceRect = new Rect(0, 0, (int)(width), (int)textDate.Height);
                    Rect sourceRect = new Rect(0, 0, (int)(width), captionHeight);

                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)(sourceRect.Width), (int)(sourceRect.Height), 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(drawingVisual);

                    int[] overlay = new int[(int)rtb.Width * (int)rtb.Height];
                    rtb.CopyPixels(overlay, (int)rtb.Width * 4, 0);
                    overlays.Add(overlay);
                }
            }

            private byte[] PreparePixels(int sizeReduction, int currentFrame)
            {
                byte[] output = new byte[posMap[posMap.Length - 1] / sizeReduction / sizeReduction + captionHeight * width];

                int outputIndex = 0;
                int inputIndex = 0;
                int posMapIndex = 0;

                int previousID = 0;
                int currentID = 0;

                int currentColor = GetTranslation(colormaps[currentFrame][currentID][0]);

                byte currentColorIndex = (byte)palette.FindIndex(x => x == currentColor);

                int mapEnd = (int)(posMap[posMap.Length - 1] / sizeReduction / sizeReduction);

                while (outputIndex < mapEnd)
                {
                    int row = inputIndex / originalWidth;
                    int column = inputIndex % originalWidth;

                    if (row % sizeReduction == 0 && column % sizeReduction == 0)
                    {
                        while (inputIndex >= posMap[posMapIndex + 1]) posMapIndex++;
                        previousID = currentID;
                        currentID = idMap[posMapIndex];

                        int colorsPerProvince = colormaps[currentFrame][currentID].Length;
                        if (colorsPerProvince == 1)
                            currentColor = GetTranslation(colormaps[currentFrame][currentID][0]);
                        else
                            currentColor = GetTranslation(colormaps[currentFrame][currentID][((outputIndex + outputIndex / originalWidth) / 2) % colorsPerProvince]);

                        if (currentID != previousID || colorsPerProvince != 1)
                        {
                            currentColorIndex = (byte)palette.FindIndex(x => x == currentColor);
                        }

                        output[outputIndex] = currentColorIndex;
                        outputIndex++;
                    }
                    inputIndex++;
                }

                // Impose overlay
                if (Core.Settings.GIFDisplayCaptionBar)
                {
                    int whiteint = CEBitmap.Bitmap.ColorToInt32(255, 255, 255);
                    for (int i = 0; i < overlays[currentFrame].Length; i++)
                    {
                        if (overlays[currentFrame][i] != 0 && overlays[currentFrame][i] != whiteint)
                            output[mapEnd + i] = 1;
                    }
                }

                return output;
            }

            private int GetTranslation(int color)
            {
                int colorTranslation;
                if (paletteTranslations.TryGetValue(color, out colorTranslation))
                    return colorTranslation;
                else
                    return color;
            }

            private void ConstructDictionary()
            {
                List<int> orderedPalette = new List<int>(palette);

                // Remove fixed colors
                orderedPalette.RemoveRange(0, fixedColors.Count);
                orderedPalette.RemoveRange(0, priorityColors.Count);
                orderedPalette.Sort(); // ascending order

                while (orderedPalette.Count > 256 - fixedColors.Count - priorityColors.Count)
                {
                    List<int> diff = new List<int>();
                    for (int i = 1; i < orderedPalette.Count; i++)
                    {
                        diff.Add(orderedPalette[i] - orderedPalette[i - 1]);
                    }

                    int minDiff = diff.Min();
                    int minDiffIndex = diff.FindIndex(x => x == minDiff) + 1;

                    paletteTranslations.Add(orderedPalette[minDiffIndex], orderedPalette[minDiffIndex - 1]);

                    palette.Remove(orderedPalette[minDiffIndex]);
                    orderedPalette.RemoveAt(minDiffIndex);
                }
            }

            private void WriteLSD(FileStream fs)
            {
                // logical screen size
                WriteShort(fs, width);
                WriteShort(fs, height + captionHeight);
                // packed fields
                fs.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
                    0x70 | // 2-4 : color resolution = 7
                    0x00 | // 5   : gct sort flag = 0
                    7)); // 6-8 : gct size

                fs.WriteByte(0); // background color index
                fs.WriteByte(0); // pixel aspect ratio - assume 1:1
            }

            private void WritePalette(FileStream fs)
            {
                for (int i = 0; i < palette.Count; i++)
                {
                    System.Windows.Media.Color color = CEBitmap.Bitmap.Int32ToColor(palette[i]);
                    fs.Write(new byte[] { color.R, color.G, color.B }, 0, 3);
                }
                int n = (3 * 256) - (3 * palette.Count);
                for (int i = 0; i < n; i++)
                {
                    fs.WriteByte(0);
                }
            }

            private void WriteNetscapeExt(FileStream fs)
            {
                fs.WriteByte(0x21); // extension introducer
                fs.WriteByte(0xff); // app extension label
                fs.WriteByte(11); // block size
                WriteString(fs, "NETSCAPE" + "2.0"); // app id + auth code
                fs.WriteByte(3); // sub-block size
                fs.WriteByte(1); // loop sub-block id
                WriteShort(fs, 0); // loop count (extra iterations, 0=repeat forever)
                fs.WriteByte(0); // block terminator
            }

            private void WriteGraphicCtrlExt(FileStream fs)
            {
                fs.WriteByte(0x21); // extension introducer
                fs.WriteByte(0xf9); // GCE label
                fs.WriteByte(4); // data block size
                int transp, disp;
                transp = 0;
                disp = 0; // dispose = no action
                disp <<= 2;

                // packed fields
                fs.WriteByte(Convert.ToByte(0 | // 1:3 reserved
                    disp | // 4:6 disposal
                    0 | // 7   user input - 0 = none
                    transp)); // 8   transparency flag

                WriteShort(fs, delay); // delay x 1/100 sec
                fs.WriteByte(Convert.ToByte(0)); // transparent color index
                fs.WriteByte(0); // block terminator
            }

            /**
             * Writes Image Descriptor
             */
            private void WriteImageDesc(FileStream fs)
            {
                fs.WriteByte(0x2c); // image separator
                WriteShort(fs, 0); // image position x,y = 0,0
                WriteShort(fs, 0);
                WriteShort(fs, width); // image size
                WriteShort(fs, height + captionHeight);
                // packed fields

                // no LCT  - GCT is used for first (or only) frame
                fs.WriteByte(0);
            }

            private void WritePixels(FileStream fs, byte[] pixels)
            {
                LZWEncoder encoder = new LZWEncoder(width, height + captionHeight, pixels, 8);
                encoder.Encode(fs);
            }

            private void WriteShort(FileStream fs, int value)
            {
                fs.WriteByte(Convert.ToByte(value & 0xff));
                fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
            }

            private void WriteString(FileStream fs, String s)
            {
                char[] chars = s.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    fs.WriteByte((byte)chars[i]);
                }
            }
        }
    }
}
