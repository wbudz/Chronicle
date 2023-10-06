using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle
{
    [Serializable]
    public struct GameDate : IComparable<GameDate>
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;

        public GameDate(int year, int month, int day, int hour)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
        }

        public GameDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = 0;
        }

        public GameDate(string text)
        {
            string t = text;
            try
            {
                t = t.Replace("date", "");
                t = t.Replace("\"", "");
                t = t.Replace("=", "").Trim();

                t = t.Replace(".", "-");

                string[] ta = t.Split('-');

                int year = Int32.Parse(ta[0]);
                int month = Int32.Parse(ta[1]);
                int day = Int32.Parse(ta[2]);
                Hour = 0;

                Year = year;
                Month = month;
                Day = day;
            }
            catch
            {
                Year = 1;
                Month = 1;
                Day = 1;
                Hour = 0;
            }

        }

        public long TotalDays
        {
            get
            {
                int[] monthLength = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                return Year * 365 + monthLength.Take(Month).Sum() + Day;
            }
        }

        public static GameDate FromBytes(byte[] raw, int index)
        {
            if (index + 16 > raw.Length) return GameDate.Empty;
            GameDate timepoint = new GameDate();
            timepoint.Year = BitConverter.ToInt32(raw, index); index += 4;
            timepoint.Month = BitConverter.ToInt32(raw, index); index += 4;
            timepoint.Day = BitConverter.ToInt32(raw, index); index += 4;
            timepoint.Hour = BitConverter.ToInt32(raw, index); index += 4;
            return timepoint;
        }

        public static byte[] ToBytes(GameDate timepoint)
        {
            byte[] output = new byte[16];
            Array.Copy(BitConverter.GetBytes(timepoint.Year), 0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(timepoint.Month), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(timepoint.Day), 0, output, 8, 4);
            Array.Copy(BitConverter.GetBytes(timepoint.Hour), 0, output, 12, 4);
            return output;
        }

        public override string ToString()
        {
            return String.Format("{0}-{1}-{2}", Year.ToString().PadLeft(4, '0'), Month.ToString().PadLeft(2, '0'), Day.ToString().PadLeft(2, '0'));
        }

        public string GetString(string s)
        {
            s = s.Replace("yyyy", Year.ToString().PadLeft(4, '0'));
            s = s.Replace("yyy", Year.ToString());
            s = s.Replace("MMMM", GetMonthLongString(Month));
            s = s.Replace("MMM", GetMonthShortString(Month));
            s = s.Replace("MM", Month.ToString().PadLeft(2, '0'));
            s = s.Replace("dd", Day.ToString().PadLeft(2, '0'));
            s = s.Replace("d", Day.ToString());
            s = s.Replace("hh", Hour.ToString().PadLeft(2, '0'));

            return s;
        }

        string GetMonthLongString(int m)
        {
            return sMonthsLong[m];
        }

        string GetMonthShortString(int m)
        {
            return sMonthsShort[m];
        }

        public int CompareTo(GameDate other)
        {
            //if (!(other is GameDate))
            //    return -1;

            //GameDate o = (GameDate)other;

            int c = Year.CompareTo(other.Year);
            if (c != 0) return c;
            c = Month.CompareTo(other.Month);
            if (c != 0) return c;
            c = Day.CompareTo(other.Day);
            if (c != 0) return c;
            c = Hour.CompareTo(other.Year);
            return c;
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (!(other is GameDate))
                return false;

            GameDate o = (GameDate)other;

            return Year.Equals(o.Year) && Month.Equals(o.Month) && Day.Equals(o.Day) && Hour.Equals(o.Hour);
        }

        public static bool operator ==(GameDate a, GameDate b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(GameDate a, GameDate b)
        {
            return !(a == b);
        }

        public static TimeSpan operator -(GameDate a, GameDate b)
        {
            return new TimeSpan((int)(a.TotalDays - b.TotalDays), a.Hour - b.Hour, 0, 0);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static GameDate Empty
        {
            get
            {
                return new GameDate();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Year.Equals(0) && Month.Equals(0) && Day.Equals(0) && Hour.Equals(0);
            }
        }

        static string[] sMonthsLong = { "?", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static string[] sMonthsShort = { "?", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    }
}
