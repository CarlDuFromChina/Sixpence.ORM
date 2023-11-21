using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// 与数据库表映射
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Description"></param>
        /// <param name="Schema"></param>
        public TableAttribute(string TableName = "", string Description = "", string Schema = "")
        {
            this.TableName = TableName;
            this.Description = Description;
            this.Schema = Schema;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 模式
        /// </summary>
        public string Schema { get; set; }
    }
}
