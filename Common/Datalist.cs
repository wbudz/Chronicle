using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    /// <summary>
    /// Represents the raw data that is present in the table.
    /// </summary>
    [Serializable]
    public class Datalist
    {
        ConflictResolution cr;
        List<Tuple<ushort, double>>[] data;
        List<string> strings;

        public bool IsCached
        {
            get
            {
                return data != null && strings != null;
            }
        }

        public long Size
        {
            get
            {
                long output = 0;
                try
                {
                    if (data != null)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            output += data[i].Count * 10;
                        }
                    }
                    if (strings != null)
                    {
                        for (int i = 0; i < strings.Count; i++)
                        {
                            output += Encoding.UTF8.GetByteCount(strings[i]);
                        }
                    }
                    return output;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public Datalist(ConflictResolution cr, int count)
        {
            this.cr = cr;
            data = new List<Tuple<ushort, double>>[count];
            for (int i = 0; i < data.Length; i++) { data[i] = new List<Tuple<ushort, double>>(); }
            strings = new List<string>();
            strings.Add("");
        }

        public void SetConflictResolution(ConflictResolution cr)
        {
            this.cr = cr;
        }

        public void Clear()
        {
            data = null;
            strings = null;
        }

        public void Read(byte[] raw)
        {
            int pos = 0;

            int count = BitConverter.ToInt32(raw, pos); pos += 4;

            data = new List<Tuple<ushort, double>>[count];

            for (int i = 0; i < count; i++)
            {
                data[i] = new List<Tuple<ushort, double>>();
                while (raw[pos++] == 0)
                {
                    data[i].Add(new Tuple<ushort, double>(BitConverter.ToUInt16(raw, pos), BitConverter.ToDouble(raw, pos + 2)));
                    pos += 10;
                }
            }

            count = BitConverter.ToInt32(raw, pos); pos += 4;
            int length;

            strings = new List<string>(count);
            strings.Add("");

            for (int i = pos; i < raw.Length;)
            {
                length = BitConverter.ToUInt16(raw, i); i += 2;
                strings.Add(Encoding.UTF8.GetString(raw, i, length)); i += length;
            }
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(data.Length), 0, 4);

                for (int i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        ms.WriteByte(0);
                        ms.Write(BitConverter.GetBytes(data[i][j].Item1), 0, 2);
                        ms.Write(BitConverter.GetBytes(data[i][j].Item2), 0, 8);
                    }
                    ms.WriteByte(1);
                }

                ms.Write(BitConverter.GetBytes(strings.Count), 0, 4);

                for (int i = 1; i < strings.Count; i++) // ignore empty string
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(strings[i]);
                    ms.Write(BitConverter.GetBytes((ushort)bytes.Length), 0, 2);
                    ms.Write(bytes, 0, bytes.Length);
                }

                return ms.ToArray();
            }
        }

        public void Set(ushort basekey, ushort datakey, double value)
        {
            if (basekey >= data.Length) return;
            try
            {
                // Check if already exists
                int index = data[basekey].FindIndex(x => x.Item1 == datakey);
                if (index == -1)
                {
                    data[basekey].Add(new Tuple<ushort, double>(datakey, value));
                }
                else
                {
                    switch (cr)
                    {
                        case ConflictResolution.ReplaceValue: data[basekey][index] = new Tuple<ushort, double>(datakey, value); break;
                        case ConflictResolution.AddValue: data[basekey][index] = new Tuple<ushort, double>(datakey, data[basekey][index].Item2 + value); break;
                        case ConflictResolution.AddEntry: data[basekey].Add(new Tuple<ushort, double>(datakey, value)); break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Table fill exception.", ex);
            }
        }

        public void Set(ushort basekey, string datakey, double value)
        {
            if (basekey >= data.Length) return;
            try
            {
                int sindex = strings.FindIndex(x => x == datakey);

                if (sindex == -1)
                {
                    strings.Add(datakey);
                    sindex = strings.Count - 1;
                }
                ushort usindex = (ushort)sindex;

                // Check if already exists
                int index = data[basekey].FindIndex(x => x.Item1 == sindex);
                if (index == -1)
                {
                    data[basekey].Add(new Tuple<ushort, double>(usindex, value));
                }
                else
                {
                    switch (cr)
                    {
                        case ConflictResolution.ReplaceValue: data[basekey][index] = new Tuple<ushort, double>(usindex, value); break;
                        case ConflictResolution.AddValue: data[basekey][index] = new Tuple<ushort, double>(usindex, data[basekey][index].Item2 + value); break;
                        case ConflictResolution.AddEntry: data[basekey].Add(new Tuple<ushort, double>(usindex, value)); break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Table fill exception.", ex);
            }
        }

        public double Get(ushort basekey, ushort datakey)
        {
            
            return (data[basekey].Find(x => x.Item1 == datakey)?.Item2) ?? 0;
        }

        public double Get(ushort basekey, string datakey)
        {
            if (basekey >= data.Length) return 0;
            int sindex = strings.FindIndex(x => x == datakey);
            if (sindex == -1) return 0;
            return (data[basekey].Find(x => x.Item1 == sindex)?.Item2) ?? 0;
        }

        public double[] GetVector()
        {
            double[] output = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    output[i] += data[i][j].Item2;
                }
            }

            return output;
        }

        public double[,] Get(ushort basekey)
        {
            if (basekey >= data.Length) return new double[0,0];

            double[,] output = new double[2, data[basekey].Count];

            for (int i = 0; i < data[basekey].Count; i++)
            {
                output[0, i] = data[basekey][i].Item1;
                output[1, i] = data[basekey][i].Item2;
            }

            return output;
        }

        public double[,] Get()
        {
            int length = 0;

            for (int i = 0; i < data.Length; i++)
            {
                length += data[i].Count;
            }

            double[,] output = new double[3, length];
            int index = 0;

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    output[0, index] = i;
                    output[1, index] = data[i][j].Item1;
                    output[2, index] = data[i][j].Item2;
                    index++;
                }
            }

            return output;
        }

        public string[] GetAllDatakeys(bool includeEmpty)
        {
            if (!includeEmpty)
                return strings.Skip(1).ToArray();
            else
                return strings.ToArray();
        }

        public string GetDatakey(ushort identifier)
        {
            return strings[identifier];
        }

        public double GetMax(ushort? basekey, ushort? datakey)
        {
            if (basekey.HasValue)
            {
                if (datakey.HasValue)
                {
                    return Math.Max((data[basekey.Value].FindAll(x => x.Item1 == datakey.Value)?.Max(x => x.Item2)) ?? 0, 0);
                }
                else
                {
                    return Math.Max(data[basekey.Value].Max(x => x.Item2), 0);
                }
            }
            else
            {
                double[] values = new double[data.Length];
                if (datakey.HasValue)
                {
                    for (int i = 0; i < values.Length; i++) { values[i] = (data[i].FindAll(x => x.Item1 == datakey.Value)?.Max(x => x.Item2)) ?? 0; }
                }
                else
                {
                    for (int i = 0; i < values.Length; i++) { values[i] = data[i].Max(x => x.Item2); }
                }
                return Math.Max(values.Max(), 0);
            }
        }

        public double GetMin(ushort? basekey, ushort? datakey)
        {
            if (basekey.HasValue)
            {
                if (datakey.HasValue)
                {
                    return Math.Min((data[basekey.Value].FindAll(x => x.Item1 == datakey.Value)?.Min(x => x.Item2)) ?? 0, 0);
                }
                else
                {
                    return Math.Min(data[basekey.Value].Min(x => x.Item2), 0);
                }
            }
            else
            {
                double[] values = new double[data.Length];
                if (datakey.HasValue)
                {
                    for (int i = 0; i < values.Length; i++) { values[i] = (data[i].FindAll(x => x.Item1 == datakey.Value)?.Min(x => x.Item2)) ?? 0; }
                }
                else
                {
                    for (int i = 0; i < values.Length; i++) { values[i] = data[i].Min(x => x.Item2); }
                }
                return Math.Min(values.Min(), 0);
            }
        }
    }

    public enum ConflictResolution { ReplaceValue, AddValue, AddEntry }
}
