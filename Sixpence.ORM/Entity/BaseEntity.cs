using Sixpence.ORM.Interface;
using Sixpence.ORM.Models;
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
                var columns = new List<ISormColumn>() { PrimaryColumn };
                var attributes = GetAttributes();
                foreach(var item in attributes)
                {
                    if (item.Key != PrimaryColumn.Name)
                    {
                        var column = new SormColumn()
                        {
                            Name = item.Key,
                            Value = item.Value,
                            DbPropertyMap = EntityMap.Properties.FirstOrDefault(p => p.Name == EntityCommon.ConvertToDbName(item.Key)),
                        };
                        columns.Add(column);
                    }
                }
                return columns;
            }
        }

        #region 索引器
        public object this[string key]
        {
            get => GetAttributeValue(key);
            set => SetAttributeValue(key, value);
        }

        public virtual IDictionary<string, object> GetAttributes()
        {
            var attributes = new Dictionary<string, object>();
            this.GetType()
                .GetProperties()
                .Where(item => Attribute.IsDefined(item, typeof(ColumnAttribute)))
                .ToList().ForEach(item =>
                {
                    attributes.Add(item.Name, item.GetValue(this));
                });
            return attributes;
        }

        public virtual object GetAttributeValue(string name)
        {
            if (ContainKey(name))
            {
                return this.GetType().GetProperty(name).GetValue(this);
            }
            return null;
        }

        public virtual T GetAttributeValue<T>(string name) where T : class
        {
            if (ContainKey(name))
            {
                var property = this.GetType().GetProperty(name);
                if (property?.GetGetMethod() != null)
                {
                    return property.GetValue(this) as T;
                }
            }
            return null;
        }

        public virtual void SetAttributeValue(string name, object value)
        {
            if (ContainKey(name))
            {
                var property = this.GetType().GetProperty(name);
                if (property?.GetSetMethod() != null)
                {
                    property.SetValue(this, value);
                }
            }
        }
        #endregion

        #region Methods
        public string NewId() => EntityCommon.GenerateID(this.PrimaryColumn.PrimaryType)?.ToString();
        public virtual IEnumerable<string> GetKeys() => EntityMap.Properties.Select(item => item.Name);
        public virtual bool ContainKey(string name) => GetKeys().Contains(name);
        #endregion
    }
}
