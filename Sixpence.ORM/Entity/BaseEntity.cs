using Sixpence.ORM.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体类基类，实体类可以继承此类
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class BaseEntity : IEntity
    {
        public IDbEntityMap EntityMap => SormServiceCollectionExtensions.Options.EntityMaps[this.GetType().FullName];

        public ISormPrimaryColumn PrimaryColumn
        {
            get
            {
                var propertyMap = EntityMap.Properties.FirstOrDefault(item => item.IsKey);
                var property = EntityCommon.GetPrimaryPropertyInfo(GetType());

                if (propertyMap == null)
                    throw new Exception("实体未定义主键");

                return new SormPrimaryColumn()
                {
                    Name = propertyMap.Name,
                    Value = property.GetValue(this) ?? "",
                    DbPropertyMap = propertyMap,
                };
            }
        }

        public IList<ISormColumn> Columns
        {
            get
            {
                var columns = new List<ISormColumn>();
                var attributes = EntityCommon.GetProperties(this);
                foreach(var item in attributes)
                {
                    if (item.Key != PrimaryColumn.Name)
                    {
                        var column = new SormColumn()
                        {
                            Name = item.Key,
                            Value = item.Value,
                            DbPropertyMap = EntityMap.Properties.FirstOrDefault(p => p.Name == EntityCommon.PascalToUnderline(item.Key)),
                        };
                        columns.Add(column);
                    }
                }
                return columns;
            }
        }

        public string NewId() => EntityCommon.GenerateID(this.PrimaryColumn.PrimaryType)?.ToString();
    }
}
