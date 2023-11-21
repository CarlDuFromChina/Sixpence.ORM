using System;
using Sixpence.ORM.Interface;
using Sixpence.ORM.Mappers;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 字段特性（映射数据库字段类型）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
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
        /// <param name="IsUnique">是否唯一</param>
        /// <param name="isRequire">是否必填</param>
        public ColumnAttribute(string Name = "", string DbType = "", int Length = 0, bool CanBeNull = false, bool IsUnique = false, object? DefaultValue = null)
        {
            Options = new DbPropertyMap()
            {
                Name = Name,
                DbType = DbType,
                CanBeNull = !CanBeNull,
                DefaultValue = DefaultValue,
                IsUnique = IsUnique
            };
            if (Length == 0)
            {
                Options.Length = null;
            }
            else
            {
                Options.Length = Length;
            }
        }

        public IDbPropertyMap Options { get; set; }
    }
}
