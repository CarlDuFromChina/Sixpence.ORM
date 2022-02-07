using Sixpence.Common;
using Sixpence.Common.Utils;
using Sixpence.ORM.Extensions;
using Sixpence.ORM.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    [DataContract]
    [Serializable]
    public abstract class BaseEntity : IEntity
    {
        private static readonly ConcurrentDictionary<string, string> _entityNameCache = new ConcurrentDictionary<string, string>();
        public BaseEntity() { }
        public BaseEntity(string entityName) { this._entityName = entityName; }

        private string _entityName;

        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityName
        {
            get
            {
                if (string.IsNullOrEmpty(_entityName))
                {
                    var type = GetType();
                    _entityName = _entityNameCache.GetOrAdd(type.FullName, (key) =>
                    {
                        var attr = Attribute.GetCustomAttribute(type, typeof(EntityAttribute)) as EntityAttribute;
                        if (attr == null)
                        {
                            throw new SpException("获取实体名失败，请检查是否定义实体名", "");
                        }
                        return attr.Name;
                    });
                }
                return _entityName;
            }
            set
            {
                _entityName = value;
            }
        }

        /// <summary>
        /// 是否是系统实体
        /// </summary>
        public bool IsSystemEntity()
        {
            var attribute = Attribute.GetCustomAttribute(GetType(), typeof(EntityAttribute)) as EntityAttribute;
            return attribute != null && attribute.IsSystemEntity;
        }

        /// <summary>
        /// 主键
        /// </summary>
        public (string Name, string Value) PrimaryKey
        {
            get
            {
                var keyName = GetPrimaryColumn()?.Name;
                return (Name: keyName, Value: GetAttributeValue<string>(keyName));
            }
        }

        public string GetPrimaryKey() => this.PrimaryKey.Name;

        #region 实体基础字段
        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember, Column("created_by", "创建人id", DataType.Varchar, 100, true)]
        public string created_by { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember, Column("created_by_name", "创建人名称", DataType.Varchar, 100, true)]
        public string created_by_name { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember, Column("created_at", "创建时间", DataType.Timestamp, 6, true)]
        public DateTime? created_at { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [DataMember, Column("updated_by", "修改人id", DataType.Varchar, 100, true)]
        public string updated_by { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [DataMember, Column("updated_by_name", "修改人名称", DataType.Varchar, 100, true)]
        public string updated_by_name { get; set; }


        /// <summary>
        /// 修改日期
        /// </summary>
        [DataMember, Column("updated_at", "修改时间", DataType.Timestamp, 6, true)]
        public DateTime? updated_at { get; set; }

        #endregion

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get => GetAttributeValue(key);
            set => SetAttributeValue(key, value);
        }

        #region Methods
        public PrimaryColumnAttribute GetPrimaryColumn()
        {
            return this.GetType()
                    .GetProperties()
                    .FirstOrDefault(item => item.IsDefined(typeof(PrimaryColumnAttribute), false))
                    .GetCustomAttributes(typeof(PrimaryColumnAttribute), false)
                    .FirstOrDefault() as PrimaryColumnAttribute;
        }

        public virtual IEnumerable<string> GetKeys()
        {
            return this.GetType().GetProperties().Select(item => item.Name);
        }

        public virtual bool ContainKey(string name)
        {
            return GetKeys().Contains(name);
        }

        public virtual IEnumerable<object> GetValues()
        {
            return this.GetType().GetProperties().Select(item => item.GetValue(this));
        }

        public virtual IDictionary<string, object> GetAttributes()
        {
            var attributes = new Dictionary<string, object>();
            this.GetType()
                .GetProperties()
                .Where(item => item.IsDefined(typeof(ColumnAttribute), true))
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
                return this.GetType().GetProperty(name).GetValue(this) as T;
            }
            return null;
        }

        public virtual void SetAttributeValue(string name, object value)
        {
            if (ContainKey(name))
            {
                this.GetType().GetProperty(name).SetValue(this, value);
            }
        }

        /// <summary>
        /// 获取实体名
        /// </summary>
        /// <returns></returns>
        public string GetEntityName()
        {
            return this.EntityName;
        }

        /// <summary>
        /// 获取实体所有字段
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Column> GetColumns()
        {
            return this.GetType()
                .GetProperties()
                .Where(item => item.IsDefined(typeof(ColumnAttribute), false))
                .Select(item => (item.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute).Column)
                .ToList();
        }

        /// <summary>
        /// 获取逻辑名
        /// </summary>
        /// <returns></returns>
        public string GetLogicalName()
        {
            var attr = Attribute.GetCustomAttribute(GetType(), typeof(EntityAttribute)) as EntityAttribute;
            if (attr == null)
            {
                return string.Empty;
            }
            return attr.LogicalName;
        }
        #endregion
    }
}
