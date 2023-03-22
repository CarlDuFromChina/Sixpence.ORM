using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,  AllowMultiple = true)]
    public sealed class KeyAttributesAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public KeyAttributesAttribute(string repeatMessage, params string[] attributes)
        {
            RepeatMessage = repeatMessage;
            AttributeList = new List<string>(attributes);
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string RepeatMessage { get; }

        /// <summary>
        /// 主键
        /// </summary>
        public IList<string> AttributeList { get; }
    }
}
