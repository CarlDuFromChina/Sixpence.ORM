using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Mappers
{
    /// <summary>
    /// 字段
    /// </summary>
    internal class DbPropertyMap : IDbPropertyMap
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public int? Length { get; set; }
        public bool? IsRequired { get; set; }
        public object DefaultValue { get; set; }
        public bool IsKey { get; set; }
        public PrimaryType? PrimaryType { get; set; }
    }
}
