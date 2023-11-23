using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    internal static class LongExtension
    {
        internal static string ToDateTimeString(this long value, string format = "yyyy-MM-dd HH:mm")
        {
            return value.ToDateTime().ToString(format);
        }

        internal static DateTime ToDateTime(this long value)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(value + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }
    }
}
