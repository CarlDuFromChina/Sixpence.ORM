using Sixpence.ORM.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Utils
{
    /// <summary>
    /// 断言
    /// </summary>
    public static class AssertUtil
    {
        public static void IsTrue(bool condition, string message)
        {
            if (condition)
             Throw(message);
        }

        public static void IsFalse(bool condition, string message)
        {
            IsTrue(!condition, message);
        }

        public static void IsNull(object anObject, string message)
        {
            IsTrue(anObject == null, message);
        }

        public static void IsNullOrEmpty(string value, string message)
        {
            IsTrue(string.IsNullOrEmpty(value), message);
        }

        public static void IsEmpty<T>(IEnumerable<T> list, string message)
        {
            IsTrue(list.IsEmpty(), message);
        }

        private static void Throw(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "未知错误";
            }
            LogUtil.Error($"系统错误：{message}");
            throw new SpException(message);
        }
    }
}
