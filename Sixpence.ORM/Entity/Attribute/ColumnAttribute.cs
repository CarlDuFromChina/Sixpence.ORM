using System;
using Sixpence.ORM.Interface;
using Sixpence.ORM.Mappers;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 字段特性（映射数据库字段类型）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        {
            Options = new DbPropertyMap();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="type">字段类型</param>
        /// <param name="length">字段长度</param>
        /// <param name="isRequire">是否必填</param>
        public ColumnAttribute(string name = "", string type = "", int length = 0, bool isRequire = false, object? defaultValue = null)
        {
            Options = new DbPropertyMap()
            {
                Name = name,
                Type = type,
                IsRequired = isRequire,
                DefaultValue = defaultValue
            };
            if (length == 0)
            {
                Options.Length = null;
            }
            else
            {
                Options.Length = length;
            }
        }

        public IDbPropertyMap Options { get; set; }
    }
}
