using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体主键
    /// </summary>
    public class PrimaryColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public PrimaryType Type { get; set; }

        /// <summary>
        /// 主键名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        /// <param name="name">主键名</param>
        /// <param name="primaryType">主键类型</param>
        public PrimaryColumnAttribute(string name = "id", PrimaryType primaryType = PrimaryType.GUID)
        {
            this.Name = name;
            this.Type = primaryType;
            this.Options = new ColumnOptions()
            {
                IsRequire = true,
                Name = name,
            };
        }
    }

    public enum PrimaryType
    {
        /// <summary>
        /// GUID
        /// </summary>
        GUID,
        /// <summary>
        /// GUID 转换为数字类型
        /// </summary>
        GUIDNumber,
    }
}
