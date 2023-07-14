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
    /// 主键
    /// </summary>
    public interface ISormPrimaryColumn : ISormColumn
    {
        /// <summary>
        /// 主键类型
        /// </summary>
        public PrimaryType PrimaryType { get; set; }
    }
}
