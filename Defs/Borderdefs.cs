using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    /// <summary>
    /// Contains definitions of all the pixels that make up borders between provinces.
    /// </summary>
    public class Borderdefs
    {
        // Each border pixel is defined as two uint's (32 bit numbers), with position defined in one uint (the same numbering as for the map, counting from bottom-left) and IDs of two bordering provinces in the second uint (one ushort ID shifted by 16 bits).

        List<int>[] LBpos;
        List<int>[] LBid;
        List<int>[] SBpos;
        List<int>[] SSpos;

        public Borderdefs(CEBitmap.Editable32bppBitmap map, Provincedefs def)
        {
            LBpos = new List<int>[map.Height];
            LBid = new List<int>[map.Height];
            SBpos = new List<int>[map.Height];
            SSpos = new List<int>[map.Height];

            LBpos[0] = new List<int>();
            LBid[0] = new List<int>();
            SBpos[0] = new List<int>();
            SSpos[0] = new List<int>();
            LBpos[map.Height - 1] = new List<int>();
            LBid[map.Height - 1] = new List<int>();
            SBpos[map.Height - 1] = new List<int>();
            SSpos[map.Height - 1] = new List<int>();

            bool[] isWaterProv = def.WaterProvinces;

            // Now calculate borders, adding bordering pixels to a corresponding border pixel list

            Parallel.For(1, map.Height - 1, row =>
              {
                  LBpos[row] = new List<int>();
                  LBid[row] = new List<int>();
                  SBpos[row] = new List<int>();
                  SSpos[row] = new List<int>();

                  int thisRowStartIndex = map.RowsOrigins[row];
                  int lowerRowStartIndex = map.RowsOrigins[row - 1];

                  int thisIndex = thisRowStartIndex;
                  int bottomIndex = lowerRowStartIndex;

                  ushort thisID = map.IDMap[thisIndex];
                  ushort bottomID = map.IDMap[bottomIndex];
                  ushort leftID = thisID;

                  int pos = row * map.Width;

                  while (thisIndex < map.RowsOrigins[row + 1] - 1)
                  {
                      if (map.PosMap[thisIndex + 1] < map.PosMap[bottomIndex + 1] + map.Width) // this row changes color earlier than the one below
                      {
                          if (thisID != bottomID) // save horizontal border
                          {
                              if (thisID == 0 || bottomID == 0) { }
                              else if (!isWaterProv[bottomID] && !isWaterProv[thisID]) // if land border
                              {
                                  LBpos[row].Add(-pos);
                                  LBpos[row].Add(map.PosMap[thisIndex + 1]);
                                  LBid[row].Add((int)(bottomID << 16) | (int)(thisID));
                                  LBid[row].Add(0);
                              }
                              else if (isWaterProv[bottomID] && isWaterProv[thisID]) // if sea border
                              {
                                  SBpos[row].Add(-pos);
                                  SBpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                              else // if shore
                              {
                                  SSpos[row].Add(-pos);
                                  SSpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                          }
                          else // save vertical border
                          {
                              if (thisID == 0 || bottomID == 0) { }
                              else if (!isWaterProv[map.IDMap[thisIndex + 1]] && !isWaterProv[thisID]) // if land border
                              {
                                  LBpos[row].Add(map.PosMap[thisIndex + 1]);
                                  LBid[row].Add((int)(map.IDMap[thisIndex + 1] << 16) | (int)(thisID));
                              }
                              else if (isWaterProv[map.IDMap[thisIndex + 1]] && isWaterProv[thisID]) // if sea border
                              {
                                  SBpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                              else // if shore
                              {
                                  SSpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                          }
                          pos = map.PosMap[thisIndex + 1];

                          leftID = thisID;
                          thisID = map.IDMap[++thisIndex];
                      }
                      else if (map.PosMap[thisIndex + 1] > map.PosMap[bottomIndex + 1] + map.Width) // this row changes color later than the one below
                      {
                          // save horizontal border
                          if (thisID != bottomID)
                          {
                              if (thisID == 0 || bottomID == 0) { }
                              else if (!isWaterProv[bottomID] && !isWaterProv[thisID]) // if land border
                              {
                                  LBpos[row].Add(-pos);
                                  LBpos[row].Add(map.PosMap[bottomIndex + 1] + map.Width);
                                  LBid[row].Add((int)(bottomID << 16) | (int)(thisID));
                                  LBid[row].Add(0);
                              }
                              else if (isWaterProv[bottomID] && isWaterProv[thisID]) // if sea border
                              {
                                  SBpos[row].Add(-pos);
                                  SBpos[row].Add(map.PosMap[bottomIndex + 1] + map.Width);
                              }
                              else // if shore
                              {
                                  SSpos[row].Add(-pos);
                                  SSpos[row].Add(map.PosMap[bottomIndex + 1] + map.Width);
                              }
                          }
                          pos = (map.PosMap[bottomIndex + 1] + map.Width);

                          bottomID = map.IDMap[++bottomIndex];
                      }
                      else // this row changes color at the same point as the one below
                      {
                          if (thisID != bottomID) // save horizontal border
                          {
                              if (thisID == 0 || bottomID == 0) { }
                              else if (!isWaterProv[bottomID] && !isWaterProv[thisID]) // if land border
                              {
                                  LBpos[row].Add(-pos);
                                  LBpos[row].Add(map.PosMap[thisIndex + 1]);
                                  LBid[row].Add((int)(bottomID << 16) | (int)(thisID));
                                  LBid[row].Add(0);
                              }
                              else if (isWaterProv[bottomID] && isWaterProv[thisID]) // if sea border
                              {
                                  SBpos[row].Add(-pos);
                                  SBpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                              else // if shore
                              {
                                  SSpos[row].Add(-pos);
                                  SSpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                          }
                          else // save vertical border
                          {
                              if (thisID == 0 || bottomID == 0) { }
                              else if (!isWaterProv[map.IDMap[thisIndex + 1]] && !isWaterProv[thisID]) // if land border
                              {
                                  LBpos[row].Add(map.PosMap[thisIndex + 1]);
                                  LBid[row].Add((int)(map.IDMap[thisIndex + 1] << 16) | (int)(thisID));
                              }
                              else if (isWaterProv[map.IDMap[thisIndex + 1]] && isWaterProv[thisID]) // if sea border
                              {
                                  SBpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                              else // if shore
                              {
                                  SSpos[row].Add(map.PosMap[thisIndex + 1]);
                              }
                          }
                          pos = map.PosMap[thisIndex + 1];

                          bottomID = map.IDMap[++bottomIndex];
                          leftID = thisID;
                          thisID = map.IDMap[++thisIndex];
                      }
                  }
              });
        }

        public void Overlay(int[] array, Color cLB, Color cSB, Color cSS, Color cCB, ushort[] provOwnership, int row)
        {
            if (row < LBpos.Length - 1) row++; // necessary for alignment

            int origin = row * Core.Data.Defs.Map.Width;

            int lb = CEBitmap.Bitmap.ColorToInt32(cLB);
            int sb = CEBitmap.Bitmap.ColorToInt32(cSB);
            int ss = CEBitmap.Bitmap.ColorToInt32(cSS);
            int cb = CEBitmap.Bitmap.ColorToInt32(cCB);
            int cb2 = CEBitmap.Bitmap.ColorToInt32(Color.FromArgb((byte)(cCB.A / 2f), cCB.R, cCB.G, cCB.B));

            if (lb != 0 || cb != 0)
            {
                for (int i = 0; i < LBpos[row].Count; i++)
                {
                    if (cb != 0)
                    {
                        ushort id1 = (ushort)(LBid[row][i] >> 16);
                        ushort id2 = (ushort)(LBid[row][i]);
                        ushort owner1 = provOwnership[id1];
                        ushort owner2 = provOwnership[id2];
                        if (owner1 != owner2 && id1 != 0 && id2 != 0)
                        {
                            if (LBpos[row][i] < 0)
                            {
                                for (int j = -LBpos[row][i] - origin; j <= LBpos[row][i + 1] - origin; j++) array[j] = Blend.BlendColors(cb, array[j]);
                                array[LBpos[row][i + 1] - origin + 1] = Blend.BlendColors(cb2, array[LBpos[row][i + 1] - origin + 1]); // doppel
                                i++;
                            }
                            else
                            {
                                array[LBpos[row][i] - origin] = Blend.BlendColors(cb, array[LBpos[row][i] - origin]);
                                array[LBpos[row][i] - origin + 1] = Blend.BlendColors(cb2, array[LBpos[row][i] - origin + 1]); // doppel
                            }
                            continue;
                        }
                    }
                    if (lb != 0)
                    {
                        if (LBpos[row][i] < 0)
                        {
                            for (int j = -LBpos[row][i] - origin; j < LBpos[row][i + 1] - origin; j++) array[j] = Blend.BlendColors(lb, array[j]);
                            i++;
                        }
                        else
                        {
                            array[LBpos[row][i] - origin] = Blend.BlendColors(lb, array[LBpos[row][i] - origin]);
                        }
                    }
                }
            }

            if (ss != 0)
            {
                for (int i = 0; i < SSpos[row].Count; i++)
                {
                    if (SSpos[row][i] < 0)
                    {
                        for (int j = -SSpos[row][i] - origin; j < SSpos[row][i + 1] - origin; j++) array[j] = Blend.BlendColors(ss, array[j]);
                        i++;
                    }
                    else
                    {
                        array[SSpos[row][i] - origin] = Blend.BlendColors(ss, array[SSpos[row][i] - origin]);
                    }
                }
            }

            if (sb != 0)
            {
                for (int i = 0; i < SBpos[row].Count; i++)
                {
                    if (SBpos[row][i] < 0)
                    {
                        for (int j = -SBpos[row][i] - origin; j < SBpos[row][i + 1] - origin; j++) array[j] = Blend.BlendColors(sb, array[j]);
                        i++;
                    }
                    else
                    {
                        array[SBpos[row][i] - origin] = Blend.BlendColors(sb, array[SBpos[row][i] - origin]);
                    }
                }
            }
        }

        public unsafe void Draw(BitmapContext bc, BorderDisplayOptions options, ushort[] provOwnership)
        {
            byte* b = (byte*)bc.Pixels;
            Parallel.For(0, Core.Data.Defs.Map.Height, (row) =>
            {
                int idx;
                int end;
                //if (row < LBpos.Length - 1) row++; // necessary for alignment

                for (int i = 0; i < LBpos[row].Count; i++)
                {
                    ushort owner1 = provOwnership[(ushort)(LBid[row][i] >> 16)];
                    ushort owner2 = provOwnership[(ushort)(LBid[row][i])];
                    if (options.HasFlag(BorderDisplayOptions.Country) && owner1 != owner2)
                    {
                        if (LBpos[row][i] < 0)
                        {
                            idx = -LBpos[row][i] * 4;
                            end = LBpos[row][i + 1] * 4;
                            while (idx < end)
                            {
                                *(b + idx) = (byte)(*(b + idx) * 0.25f);
                                *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.25f);
                                *(b + idx + 2) = 255;

                                *(b + idx + 4) = (byte)(*(b + idx + 4) * 0.5f);
                                *(b + idx + 5) = (byte)(*(b + idx + 5) * 0.5f);
                                *(b + idx + 6) = 200;

                                idx += 4;
                            }
                            ++i;
                        }
                        else
                        {
                            idx = LBpos[row][i] * 4;
                            *(b + idx) = (byte)(*(b + idx) * 0.25f);
                            *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.25f);
                            *(b + idx + 2) = 255;

                            *(b + idx + 4) = (byte)(*(b + idx + 4) * 0.5f);
                            *(b + idx + 5) = (byte)(*(b + idx + 5) * 0.5f);
                            *(b + idx + 6) = 200;
                        }
                        continue;
                    }
                    else if (options.HasFlag(BorderDisplayOptions.Land))
                    {
                        if (LBpos[row][i] < 0)
                        {
                            idx = -LBpos[row][i] * 4;
                            end = LBpos[row][i + 1] * 4;
                            while (idx < end)
                            {
                                *(b + idx) = (byte)(*(b + idx) * 0.3f);
                                *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.3f);
                                *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.3f);
                                idx += 4;
                            }
                            ++i;
                        }
                        else
                        {
                            idx = LBpos[row][i] * 4;
                            *(b + idx) = (byte)(*(b + idx) * 0.3f);
                            *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.3f);
                            *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.3f);
                        }
                    }
                }

                if (options.HasFlag(BorderDisplayOptions.Country))
                {
                    for (int i = 0; i < SSpos[row].Count; i++)
                    {
                        if (SSpos[row][i] < 0)
                        {
                            idx = -SSpos[row][i] * 4;
                            end = SSpos[row][i + 1] * 4;
                            while (idx < end)
                            {
                                *(b + idx) = (byte)(*(b + idx) * 0.6f);
                                *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.6f);
                                *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.6f);
                                idx += 4;
                            }
                            ++i;
                        }
                        else
                        {
                            idx = SSpos[row][i] * 4;
                            *(b + idx) = (byte)(*(b + idx) * 0.6f);
                            *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.6f);
                            *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.6f);
                        }
                    }
                }

                if (options.HasFlag(BorderDisplayOptions.Sea))
                {
                    for (int i = 0; i < SBpos[row].Count; i++)
                    {
                        if (SBpos[row][i] < 0)
                        {
                            idx = -SBpos[row][i] * 4;
                            end = SBpos[row][i + 1] * 4;
                            while (idx < end)
                            {
                                *(b + idx) = (byte)(*(b + idx) * 0.5f);
                                *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.5f);
                                *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.5f);
                                idx += 4;
                            }
                            ++i;
                        }
                        else
                        {
                            idx = SBpos[row][i] * 4;
                            *(b + idx) = (byte)(*(b + idx) * 0.5f);
                            *(b + idx + 1) = (byte)(*(b + idx + 1) * 0.5f);
                            *(b + idx + 2) = (byte)(*(b + idx + 2) * 0.5f);
                        }
                    }
                }
            });
        }
    }

    [Flags]
    public enum BorderDisplayOptions
    {
        Country = 1,
        Land = 2,
        Shore = 4,
        Sea = 8
    }
}
