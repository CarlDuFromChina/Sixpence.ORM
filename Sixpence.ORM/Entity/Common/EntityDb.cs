using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sixpence.ORM.Utils;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 类属性读写（数据库字段名）
    /// </summary>
    public static partial class EntityCommon
    {
        private static readonly ConcurrentDictionary<string, string> _entityNameCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 获取实体表名
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetEntityTableName(IEntity entity)
        {
            return GetEntityTableName(entity.GetType());
        }

        /// <summary>
        /// 获取实体表名
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetEntityTableName(Type entity)
        {
            return _entityNameCache.GetOrAdd(entity.FullName, (Func<string, string>)((key) =>
            {
                var attr = Attribute.GetCustomAttribute(entity, typeof(TableAttribute)) as TableAttribute;
                if (attr == null)
                {
                    throw new Exception("获取实体名失败，请检查是否定义实体名");
                }

                // 若未设置自定义表名，则根据类名去格式化
                if (string.IsNullOrEmpty(attr.TableName))
                {
                    var name = entity.Name;
                    return PascalToUnderline(name);
                }
                return attr.TableName;
            }));
        }

        public static string GetEntityTableDescription(IEntity entity)
        {
            return GetEntityTableDescription(entity.GetType());
        }

        public static string GetEntityTableDescription(Type entity)
        {
            var attr = Attribute.GetCustomAttribute(entity, typeof(TableAttribute)) as TableAttribute;
            return attr?.Description ?? string.Empty;
        }

        /// <summary>
        /// 根据设置命名规则转换成数据库名称，数据库字段和表名都需要转换
        /// 例如下划线命名 UserName，返回值 user_name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ConvertToDbName(string name)
        {
            return PascalToUnderline(name);
        }

        /// <summary>
        /// 获取实体模式
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetEntitySchema(IEntity entity) => GetEntitySchema(entity.GetType());

        /// <summary>
        /// 获取实体模式
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetEntitySchema(Type entity)
        {
            var attribute = Attribute.GetCustomAttribute(entity, typeof(TableAttribute)) as TableAttribute;
            if (attribute == null)
            {
                throw new Exception("获取实体名失败，请检查是否定义实体名");
            }

            if (!string.IsNullOrEmpty(attribute.Schema))
            {
                return attribute.Schema;
            }

            return SormServiceCollectionExtensions.Options?.DbSetting?.Driver?.Dialect?.Schema;
        }

        #region 根据数据库字段名读写
        /// <summary>
        /// 获取实体所有的数据库字段名和值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Dictionary<string, object?> GetDbColumns(object entity)
        {
            var attributes = new Dictionary<string, object?>();
            var entityType = entity.GetType();

            entityType
                .GetProperties()
                .Where(item => Attribute.IsDefined(item, typeof(ColumnAttribute)))
                .ToList().ForEach(item =>
                {
                    if (item.CanRead)
                    {
                        var attribute = AttributeUtil.GetAttribute<ColumnAttribute>(item);
                        var keyName = !string.IsNullOrEmpty(attribute?.Options?.Name) ? attribute.Options.Name : PascalToUnderline(item.Name);
                        var keyValue = item.GetValue(entity);
                        attributes.Add(keyName, keyValue);
                    }
                });

            return attributes;
        }

        /// <summary>
        /// 根据数据库字段名设置实体属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetDbColumnValue(object entity, string name, object? value)
        {
            var entityType = entity.GetType();
            var property = entityType.GetProperty(UnderlineToPascal(name));
            if (property?.GetSetMethod() != null)
            {
                property.SetValue(entity, value);
            }
        }

        /// <summary>
        /// 根据数据库字段名获取属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object? GetDbColumnValue(object entity, string name)
        {
            var entityType = entity.GetType();
            return entityType.GetProperty(UnderlineToPascal(name))?.GetValue(entity);
        }
        #endregion
    }
}
