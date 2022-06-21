using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute
    {
        public EntityAttribute(string tableName = "", string logicalName = "", bool isSystemEntity = false)
        {
            this.TableName = tableName;
            this.LogicalName = logicalName;
            this.IsSystemEntity = isSystemEntity;
        }

        public string TableName { get; set; }
        public string LogicalName { get; set; }
        public bool IsSystemEntity { get; set; }
    }
}
