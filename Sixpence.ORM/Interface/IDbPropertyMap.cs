using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IDbPropertyMap
    {
        /// <summary>
        /// 字段名
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 字段类型（数据库字段类型）
        /// </summary>
        string DbType { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        string Remark { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        int? Length { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        bool? CanBeNull { get; set; }

        /// <summary>
        /// 是否唯一
        /// </summary>
        bool IsUnique { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        object DefaultValue { get; set; }

        /// <summary>
        /// 是否是主键
        /// </summary>
        bool IsKey { get; set; }

        /// <summary>
        /// 主键类型
        /// </summary>
        PrimaryType? PrimaryType { get; set; }
    }
}
