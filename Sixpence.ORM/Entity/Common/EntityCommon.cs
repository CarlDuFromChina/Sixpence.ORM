using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数
    /// </summary>
    public static partial class EntityCommon
    {
        /// <summary>
        /// 获取实体所有的字段名和值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Dictionary<string, object?> GetProperties(object entity)
        {
            var entityType = entity.GetType();
            var attributes = new Dictionary<string, object?>();
            entityType
                .GetProperties()
                .Where(item => Attribute.IsDefined(item, typeof(ColumnAttribute)))
                .ToList().ForEach(item =>
                {
                    var keyName = item.Name;
                    var keyValue = item.GetValue(entity);
                    attributes.Add(keyName, keyValue);
                });
            return attributes;
        }

        /// <summary>
        /// 设置实体属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetAttributeValue(object entity, string name, object value)
        {
            var entityType = entity.GetType();
            var property = entityType.GetProperty(name);
            if (property?.GetSetMethod() != null)
            {
                property.SetValue(entity, value);
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object? GetAttributeValue(object entity, string name)
        {
            var entityType = entity.GetType();
            return entityType.GetProperty(name)?.GetValue(entity);
        }

        /// <summary>
        /// 匹配 EntityManager Plugin
        /// </summary>
        /// <param name="className"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool MatchEntityManagerPlugin(string className, string tableName)
        {
            return className.StartsWith(tableName.Replace("_", ""), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取主键名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static PropertyInfo GetPrimaryPropertyInfo(Type type)
        {
            var properties = type.GetProperties().Where(item => Attribute.IsDefined(item, typeof(PrimaryColumnAttribute)));
            if (properties == null)
            {
                throw new Exception("实体未定义主键");
            }
            if (properties.Count() > 1)
            {
                throw new Exception("实体只能有一个主键");
            }
            return properties.FirstOrDefault();
        }
    }
}
