using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Utils
{
    /// <summary>
    /// 断言
    /// </summary>
    internal static class AssertUtil
    {
        internal static void IsTrue(bool condition, string message)
        {
            if (condition)
              throw new Exception(message);
        }

        internal static void IsFalse(bool condition, string message)
        {
            IsTrue(!condition, message);
        }

        internal static void IsNull(object? anObject, string message)
        {
            IsTrue(anObject == null, message);
        }

        internal static void IsNullOrEmpty(string value, string message)
        {
            IsTrue(string.IsNullOrEmpty(value), message);
        }

        internal static void IsEmpty<T>(IEnumerable<T> list, string message)
        {
            IsTrue(list.IsEmpty(), message);
        }
    }
}
