using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace Chronicle
{
    public class Cache
    {
        ZipFile file;

        string path;

        object accessLock = new object();

        public Cache()
        {
            this.path = Path.Combine(Core.Paths.Cache, Core.GUID + ".cache");

            try
            {
                lock (accessLock)
                {
                    Core.TryCreateDirectory(Path.GetDirectoryName(path));
                    file = new ZipFile(path);
                    file.Save();
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Critical error initializing cache.", ex);
            }
        }

        public Cache(string path)
        {
            this.path = Path.Combine(Core.Paths.Cache, Core.GUID + ".cache");

            lock (accessLock)
            {
                Core.TryDelete(this.path);
                Core.TryCreateDirectory(Path.GetDirectoryName(this.path));
                new FileInfo(path).CopyTo(this.path, true);
                file = ZipFile.Read(path);
            }
        }

        public void Clear()
        {
            lock (accessLock)
            {
                Core.TryCreateDirectory(Path.GetDirectoryName(path));
                Core.TryDelete(this.path);
                file = new ZipFile(path);
                file.Save();
            }
        }

        public void ClearTables()
        {
            lock (accessLock)
            {
                file.RemoveSelectedEntries("*.data");
                file.RemoveSelectedEntries("*.table");
            }
        }

        public void SaveAs(string destination)
        {
            try
            {
                lock (accessLock)
                {
                    File.Copy(path, destination, true);
                }
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error copying cache file to destination: <" + destination + ">.", ex);
            }
        }

        #region Readers

        public byte[] ReadGameDefinitions(string name)
        {
            lock (accessLock)
            {
                var t = file.SelectEntries(name + ".bin").ToList();
                if (t.Count < 1) throw new Exception("Cache does not contain <" + name + "> entry.");
                using (var ms = new MemoryStream())
                {
                    t[0].Extract(ms);
                    return ms.ToArray();
                }
            }
        }

        public IEnumerable<byte[]> ReadTableDefinitions()
        {
            lock (accessLock)
            {
                var t = file.SelectEntries("*.table").ToList();
                for (int i = 0; i < t.Count; i++)
                {
                    using (var ms = new MemoryStream())
                    {
                        t[i].Extract(ms);
                        yield return ms.ToArray();
                    }
                }
            }
        }

        public byte[] ReadColorscales()
        {
            lock (accessLock)
            {
                var t = file.SelectEntries("colorscales.bin").ToList();
                if (t.Count < 1) throw new Exception("Colorscales information missing in the cache.");
                using (var ms = new MemoryStream())
                {
                    t[0].Extract(ms);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ReadTableData(string cacheFilename)
        {
            lock (accessLock)
            {
                var t = file.SelectEntries(cacheFilename + ".data").ToList();
                if (t.Count < 1) throw new Exception("Table data (<" + cacheFilename + ">) missing in the cache.");
                using (var ms = new MemoryStream())
                {
                    t[0].Extract(ms);
                    return ms.ToArray();
                }
            }
        }

        #endregion

        #region Writers

        public void WriteGameDefinitions(string name, byte[] data)
        {
            lock (accessLock)
            {
                if (!file.ContainsEntry(name + ".bin"))
                {
                    file.AddEntry(name + ".bin", data);
                    file.Save();
                }
            }
        }

        public void WriteTableDefinitions(IEnumerable<Table> tables)
        {
            lock (accessLock)
            {
                foreach (var t in tables)
                {
                    if (!file.ContainsEntry(t.CacheFilename + ".table"))
                    {
                        file.AddEntry(t.CacheFilename + ".table", t.WriteDefinition());
                    }
                }
                file.Save();
            }
        }

        public void WriteColorscales(byte[] data)
        {
            lock (accessLock)
            {
                file.UpdateEntry("colorscales.bin", data);
                file.Save();
            }
        }

        public void WriteTableData(IEnumerable<Table> tables)
        {
            lock (accessLock)
            {
                foreach (var t in tables)
                {
                    if (!file.ContainsEntry(t.CacheFilename + ".data"))
                    {
                        file.AddEntry(t.CacheFilename + ".data", t.WriteData());
                    }
                }
                file.Save();
            }
        }

        #endregion
    }
}
