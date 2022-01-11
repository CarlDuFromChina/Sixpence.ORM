using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    public class PrimaryColumnAttribute : ColumnAttribute
    {
        public PrimaryType Type { get; set; }

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
            this.Column = new Column()
            {
                IsRequire = true,
                Length = 100,
                Name = name,
                LogicalName = name,
                Type = DataType.Varchar
            };
        }
    }

    public enum PrimaryType
    {
        /// <summary>
        /// GUID
        /// </summary>
        GUID
    }
}
