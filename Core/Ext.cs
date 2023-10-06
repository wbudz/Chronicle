using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Chronicle
{
    public static class Ext
    {
        #region Data parsers

        public static byte ParseByte(string input)
        {
            byte value;
            Byte.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            return value;
        }

        public static double ParseDouble(string input)
        {
            double value;
            Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return value;
        }

        public static ushort ParseUShort(string input)
        {
            ushort value;
            UInt16.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            return value;
        }

        public static double ParseDate(string input, string format)
        {
            if (format.ToLowerInvariant() == "yyy.M.d")
            {
                try
                {
                    string[] dateSplit = input.Split('.');
                    int year;
                    int month;
                    int day;
                    Int32.TryParse(dateSplit[0], out year);
                    Int32.TryParse(dateSplit[1], out month);
                    Int32.TryParse(dateSplit[2], out day);
                    if (year < 1) year = 1;
                    if (month < 1) month = 1;
                    if (day < 1) day = 1;

                    return new TimeSpan((new DateTime(year, month, day)).Ticks).TotalDays * 24;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error when converting date \"" + input + "\" into double.", ex);
                }
            }
            else
            {
                throw new Exception("Incorrect date format for conversion into double.");
            }
        }

        #endregion

        #region File operations

        public static string GetDirectoryLastWrite(ref DateTime LastWrite, string directory)
        {
            if (!Directory.Exists(directory)) return "";

            string[] files = Directory.GetFiles(directory);
            DateTime newestWrite = DateTime.MinValue;
            string newestFile = "";
            for (int i = 0; i < files.Length; i++)
            {
                if (File.GetLastWriteTime(files[i]) > newestWrite)
                {
                    newestWrite = File.GetLastWriteTime(files[i]);
                    newestFile = files[i];
                }
            }

            if (newestWrite <= LastWrite) return "";

            LastWrite = newestWrite;
            return newestFile;
        }
        
        public static async Task WaitForFileUnlock(string path)
        {
            FileStream stream = null;
            int failsafe = 10;
            // Loop until the file is completely written to; there are 10 attempts to read the file, each 0.5 s after the previous.
            await Task.Run(() =>
            {
                do
                {
                    try
                    {
                        // Full access without sharing so that we can be sure that the file is fully written to before starting to read it
                        stream = new FileInfo(path).Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        break;
                    }
                    catch (IOException)
                    {
                        //the file is unavailable because it is still being written to or being processed by another thread or does not exist (has already been processed)
                        Thread.Sleep(200);
                    }
                    finally
                    {
                        if (stream != null) stream.Close();
                    }
                } while (failsafe-- > 0);
            });
        }

        public static byte[] GetFileHash(string fileName)
        {
            try
            {
                HashAlgorithm sha1 = HashAlgorithm.Create();
                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return sha1.ComputeHash(stream);
            }
            catch
            {
                return null;
            }
        }

        public static bool AreHashesEqual(byte[] b1, byte[] b2)
        {
            try
            {
                return b1.SequenceEqual(b2);
            }
            catch
            {
                return false;
            }
        }

        public static async Task WaitForWritingFinish(string path)
        {
            byte[] hash1 = new byte[] { 0 };
            byte[] hash2 = new byte[] { 0 };
            int failsafe = 10;
            await Task.Run(() =>
            {
                do
                {
                    hash1 = GetFileHash(path);
                    Thread.Sleep(500);
                    hash2 = GetFileHash(path);
                } while (!AreHashesEqual(hash1, hash2) && failsafe-- > 0);
            });
        }

        #endregion

        #region Graphics

        public static BitmapSource LoadBitmap(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            BitmapSource src = bitmap;

            return src;
        }

        #endregion
               
    }
}
