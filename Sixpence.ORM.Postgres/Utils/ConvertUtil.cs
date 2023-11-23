using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Postgres.Utils
{
    internal class ConvertUtil
    {
        internal static int ConToInt(object value)
        {
            if (value == null || value is DBNull || value.ToString().Trim() == "") return 0;

            if (value is int)
                return (int)value;

            return Convert.ToInt32(value);
        }

        internal static string ConToString(object value)
        {
            return (value == null || value is DBNull) ? string.Empty : value.ToString();
        }

        internal static byte[] ConToBytes(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new byte[0];
            }

            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            return bytes;
        }

        internal static bool ConToBoolean(object value)
        {
            if (value == null || value is DBNull || value.ToString().Trim() == "")
            {
                return false;
            }

            if (value is string)
            {
                var s = value.ToString();
                if (s.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                if (s.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                return s == "1";
            }

            return Convert.ToBoolean(value);
        }

        internal static double ConToDouble(object value)
        {
            if (value == null || value is DBNull || value.ToString().Trim() == "")
                return 0d;

            return Convert.ToDouble(value);
        }

        internal static decimal ConToDecimal(object value)
        {
            if (value == null || value is DBNull || value.ToString().Trim() == "")
                return 0M;

            return Convert.ToDecimal(value);
        }

        internal static DateTime ConToDateTime(object date)
        {
            if (date == null || date is DBNull)
                return default(DateTime);

            if (date is DateTime)
                return (DateTime)date;

            try
            {
                return Convert.ToDateTime(date.ToString());
            }
            catch
            {
                return default(DateTime);
            }
        }
    }
}
