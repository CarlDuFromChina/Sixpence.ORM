using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute
    {
        public EntityAttribute(string tableName = "", string remark = "", string schema = "")
        {
            this.TableName = tableName;
            this.Remark = remark;
            this.Schema = schema;

            if (string.IsNullOrEmpty(schema))
            {
                Schema = SormServiceCollectionExtensions.Options.DbSetting.Driver.Dialect.Schema;
            }
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 模式
        /// </summary>
        public string Schema { get; set; }
    }
}
