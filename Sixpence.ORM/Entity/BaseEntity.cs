using Newtonsoft.Json.Linq;
using Sixpence.Common;
using Sixpence.ORM.Extensions;
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
    [DataContract]
    [Serializable]
    public abstract class BaseEntity : IEntity
    {
        private static readonly ConcurrentDictionary<string, string> _entityNameCache = new ConcurrentDictionary<string, string>();

        #region 实体基础字段
        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember, Column(isRequire: true), Description("创建人id")]
        public string created_by { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember, Column(isRequire: true), Description("创建人名称")]
        public string created_by_name { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember, Column(isRequire: true), Description("创建时间")]
        public DateTime? created_at { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [DataMember, Column(isRequire: true), Description("修改人")]
        public string updated_by { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [DataMember, Column(isRequire: true), Description("修改人姓名")]
        public string updated_by_name { get; set; }


        /// <summary>
        /// 修改日期
        /// </summary>
        [DataMember, Column(isRequire: true), Description("修改时间")]
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
        public PrimaryColumnAttribute GetPrimaryColumnAttribute()
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
                    if (property.PropertyType == typeof(JToken) && !string.IsNullOrEmpty(value?.ToString()))
                    {
                        property.SetValue(this, JToken.Parse(value?.ToString()));
                    }
                    else
                    {
                        property.SetValue(this, value);
                    }
                }
            }
        }

        /// <summary>
        /// 获取实体名
        /// </summary>
        /// <returns></returns>
        public string GetEntityName()
        {
            var type = GetType();
            return _entityNameCache.GetOrAdd(type.FullName, (key) =>
            {
                var attr = Attribute.GetCustomAttribute(type, typeof(EntityAttribute)) as EntityAttribute;
                if (attr == null)
                {
                    throw new SpException("获取实体名失败，请检查是否定义实体名");
                }

                // 若未设置自定义表名，则根据类名去格式化
                if (string.IsNullOrEmpty(attr.TableName))
                {
                    var name = this.GetType().Name;
                    switch (SixpenceORMBuilderExtension.Options.EntityClassNameCase)
                    {
                        case NameCase.UnderScore:
                            return name.ToLower();
                        case NameCase.Pascal:
                        default:
                            return EntityCommon.UpperChartToLowerUnderLine(name);
                    }
                }
                return attr.TableName;
            });
        }

        /// <summary>
        /// 获取逻辑名
        /// </summary>
        /// <returns></returns>
        public string GetRemark()
        {
            var attr = Attribute.GetCustomAttribute(GetType(), typeof(EntityAttribute)) as EntityAttribute;
            if (attr == null)
            {
                return string.Empty;
            }
            return attr.Remark;
        }

        /// <summary>
        /// 生成一个新 ID
        /// </summary>
        /// <returns></returns>
        public string NewId()
        {
            return EntityCommon.GenerateID(GetPrimaryColumn().Type).ToString();
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <returns></returns>
        public (string Name, string Value, PrimaryType Type) GetPrimaryColumn()
        {
            var primaryColumn = GetPrimaryColumnAttribute();
            var keyName = primaryColumn.Name;
            var type = primaryColumn.Type;
            var value = GetAttributeValue<string>(keyName);
            return (Name: keyName, Value: value, Type: type);
        }
        #endregion
    }
}
