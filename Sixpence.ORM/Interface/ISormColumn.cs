using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    /// <summary>
    /// 普通字段
    /// </summary>
    public interface ISormColumn
    {
        /// <summary>
        /// 字段名，例如：public string PhoneNumber { get;set; }，则Name为PhoneNumber
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 数据库字段映射
        /// </summary>
        public IDbPropertyMap DbPropertyMap { get; set; }
    }
}
