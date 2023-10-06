using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Media;

namespace CEBitmap
{
    static class Reader
    {
        public static List<int> Read1bppBitmap(string path, out int width, out int height, byte[] ignoredColors, bool flip, out List<int> rowsOrigins)
        {
            if (!(File.Exists(path))) throw new Exception("The file at the location: <" + path + "> does not exist.");

            rowsOrigins = new List<int>();

            try
            {
                int offset = 0;
                // Read raw contents and create an id map.
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[54];
                    fs.Read(header, 0, header.Length);
                    offset = BitConverter.ToInt32(header, 10);
                    width = BitConverter.ToInt32(header, 18);
                    height = BitConverter.ToInt32(header, 22);
                    if (BitConverter.ToInt16(header, 28) != 8) throw new Exception("The source file at the location: <" + path + "> has incorrect color depth.");

                    List<int> pixelList = new List<int>();

                    byte[] buffer = new byte[width];
                    List<int> list = new List<int>();
                    int index = 0;
                    bool acceptCurrentColor = false;
                    rowsOrigins.Add(0);

                    if (flip)
                    {
                        fs.Seek(-(fs.Length % (offset + width * height)) - width, SeekOrigin.End);
                        while (fs.Position >= offset)
                        {
                            fs.Read(buffer, 0, buffer.Length);

                            acceptCurrentColor = !ignoredColors.Contains(buffer[0]);
                            if (acceptCurrentColor) list.Add(index);
                            index++;
                            for (int i = 1; i < buffer.Length; i++)
                            {
                                if (buffer[i] != buffer[i - 1])
                                    acceptCurrentColor = !ignoredColors.Contains(buffer[i]);
                                if (acceptCurrentColor) list.Add(index);
                                index++;
                            }
                            rowsOrigins.Add(list.Count - 1);
                            if (fs.Position < width * 2)
                                break;
                            else
                            {
                                fs.Seek(-width * 2, SeekOrigin.Current);
                            }
                        }
                    }
                    else
                    {
                        fs.Seek(offset - 54, SeekOrigin.Current);
                        while (fs.Position < 54 + width * height)
                        {
                            fs.Read(buffer, 0, buffer.Length);

                            acceptCurrentColor = !ignoredColors.Contains(buffer[0]);
                            if (acceptCurrentColor) list.Add(index);
                            index++;
                            for (int i = 1; i < buffer.Length; i++)
                            {
                                if (buffer[i] != buffer[i - 1])
                                    acceptCurrentColor = !ignoredColors.Contains(buffer[i]);
                                if (acceptCurrentColor) list.Add(index);
                                index++;
                            }
                            rowsOrigins.Add(list.Count - 1);
                        }
                    }

                    rowsOrigins.Add(list.Count - 1);

                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading 1 bits-per-pixel bitmap.", ex);
            }
        }

        public static void Read8bppBitmap(string path, out int width, out int height, out Color[] palette, bool flip, int enforcedPaletteSize, out List<byte> idMap, out List<int> posMap, byte[] aggregator)
        {
            if (!(File.Exists(path))) throw new Exception("The file at the location: <" + path + "> does not exist.");

            idMap = new List<byte>();
            posMap = new List<int>();

            try
            {

                // Read raw contents and create an id map.
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[54];
                    fs.Read(header, 0, header.Length);
                    int bo = BitConverter.ToInt32(header, 10);
                    width = BitConverter.ToInt32(header, 18);
                    height = BitConverter.ToInt32(header, 22);
                    if (BitConverter.ToInt16(header, 28) != 8) throw new Exception("The source file at the location: <" + path + "> has incorrect color depth.");

                    // Read palette    
                    int paletteSize = (int)fs.Length - (54 + width * height); // should be 1024
                    if (enforcedPaletteSize > 0) paletteSize = enforcedPaletteSize;

                    byte[] paletteDef = new byte[paletteSize];
                    palette = new Color[256];
                    fs.Read(paletteDef, 0, paletteDef.Length);
                    int paletteIndex = 0;
                    for (int i = 0; i < paletteDef.Length - 2; i += 4)
                    {
                        palette[paletteIndex++] = Color.FromRgb(paletteDef[i + 2], paletteDef[i + 1], paletteDef[i]);
                        if (paletteIndex >= 255) break;
                    }

                    byte[] buffer = new byte[width];
                    int row = 0;
                    int offset = 0;

                    byte id = 0;

                    if (flip)
                    {
                        fs.Seek(-(fs.Length % (bo + width * height)) - width, SeekOrigin.End);
                        while (fs.Position >= bo)
                        {
                            offset = row * width;
                            fs.Read(buffer, 0, buffer.Length); //fs.Read(buffer, row++ * width, width);
                            posMap.Add(offset);
                            id = aggregator[buffer[0]];
                            idMap.Add(id);
                            for (int i = 1; i < buffer.Length; i++)
                            {
                                if (buffer[i - 1] != buffer[i] && id != aggregator[buffer[i]])
                                {
                                    id = aggregator[buffer[i]];
                                    posMap.Add(offset + i);
                                    idMap.Add(id);
                                }
                            }
                            row++;
                            if (fs.Position < width * 2)
                                break;
                            else
                                fs.Seek(-width * 2, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        fs.Seek(bo, SeekOrigin.Begin);
                        while (fs.Position < bo + width * height)
                        {
                            offset = row * width;
                            fs.Read(buffer, 0, buffer.Length); // fs.Read(buffer, row++ * width, width);
                            posMap.Add(offset);
                            idMap.Add(aggregator[buffer[0]]);
                            for (int i = 1; i < buffer.Length; i++)
                            {
                                if (buffer[i - 1] != buffer[i] && aggregator[buffer[i - 1]] != aggregator[buffer[i]])
                                {
                                    posMap.Add(offset + i);
                                    idMap.Add(aggregator[buffer[i]]);
                                }
                            }
                            row++;
                        }
                    }

                    idMap.Add(0);
                    posMap.Add(width * height);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading 32 bits-per-pixel bitmap.", ex);
            }
        }

        public static void Read32bppBitmap(string path, out int width, out int height, Dictionary<int, ushort> idConverter, bool flip, out ushort[] idMap, out int[] posMap, out int[] rowsOrigins)
        {
            if (!(File.Exists(path))) throw new Exception("The source file at the location: <" + path + "> does not exist.");

            List<ushort> _idMap = new List<ushort>();
            List<int> _posMap = new List<int>();
            List<int> _rowsOrigins = new List<int>();

            try
            {

                // Read raw contents and create an id map.
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[54];
                    fs.Read(header, 0, header.Length);
                    int bo = BitConverter.ToInt32(header, 10);
                    width = BitConverter.ToInt32(header, 18);
                    height = BitConverter.ToInt32(header, 22);
                    if (BitConverter.ToInt16(header, 28) != 24) throw new Exception("The source file at the location: <" + path + "> has incorrect color depth.");

                    byte[] buffer = new byte[width * 3];
                    int row = 0;
                    int offset = 0;
                    ushort id = 0;

                    if (flip)
                    {
                        fs.Seek(-(fs.Length % (bo + width * height * 3)) - width * 3, SeekOrigin.End);
                        while (fs.Position >= bo)
                        {
                            offset = row * width;
                            fs.Read(buffer, 0, buffer.Length);
                            idConverter.TryGetValue(255 << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0] << 0, out id);
                            _posMap.Add(offset);
                            _idMap.Add(id);
                            _rowsOrigins.Add(_posMap.Count - 1);
                            for (int i = 3; i < buffer.Length - 2; i += 3)
                            {
                                if ((buffer[i - 3] != buffer[i]) || (buffer[i - 2] != buffer[i + 1]) || (buffer[i - 1] != buffer[i + 2]))
                                {
                                    idConverter.TryGetValue(255 << 24 | buffer[i + 2] << 16 | buffer[i + 1] << 8 | buffer[i] << 0, out id);
                                    _posMap.Add(offset + i / 3);
                                    _idMap.Add(id);
                                }
                            }
                            row++;
                            if (fs.Position < width * 3 * 2)
                                break;
                            else
                                fs.Seek(-width * 3 * 2, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        fs.Seek(bo, SeekOrigin.Begin);
                        while (fs.Position < bo + width * height * 3)
                        {
                            offset = row * width;
                            fs.Read(buffer, 0, buffer.Length);
                            idConverter.TryGetValue(255 << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0] << 0, out id);
                            _posMap.Add(offset);
                            _idMap.Add(id);
                            _rowsOrigins.Add(_posMap.Count - 1);
                            for (int i = 3; i < buffer.Length - 2; i += 3)
                            {
                                if ((buffer[i - 3] != buffer[i]) || (buffer[i - 2] != buffer[i + 1]) || (buffer[i - 1] != buffer[i + 2]))
                                {
                                    idConverter.TryGetValue(255 << 24 | buffer[i + 2] << 16 | buffer[i + 1] << 8 | buffer[i] << 0, out id);
                                    _posMap.Add(offset + i / 3);
                                    _idMap.Add(id);
                                }
                            }
                            row++;
                        }
                    }

                    _idMap.Add(0);
                    _posMap.Add(width * height);
                    _rowsOrigins.Add(_posMap.Count - 1);
                }

                idMap = _idMap.ToArray();
                posMap = _posMap.ToArray();
                rowsOrigins = _rowsOrigins.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading 32 bits-per-pixel bitmap.", ex);
            }
        }
    }
}
