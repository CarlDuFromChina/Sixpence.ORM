using Sixpence.ORM.Interface;
using Sixpence.ORM.Mappers;
using Sixpence.ORM.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数
    /// </summary>
    public static class EntityCommon
    {
        private static readonly ConcurrentDictionary<string, string> _entityNameCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <param name="primaryType"></param>
        /// <returns></returns>
        public static object GenerateID(PrimaryType? primaryType = PrimaryType.GUID)
        {
            switch (primaryType)
            {
                case PrimaryType.GUIDNumber:
                    return GenerateGuidNumber();
                case PrimaryType.GUID:
                default:
                    return GenerateGuid();
            }
        }

        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static object GenerateID<TEntity>() where TEntity : IEntity, new()
        {
            var entity = new TEntity();
            return GenerateID(entity.PrimaryColumn.PrimaryType);
        }

        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <returns></returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 生成实体随机 ID（数字版）
        /// </summary>
        /// <returns></returns>
        public static long GenerateGuidNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
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
        /// 帕斯卡命名转下划线命名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string PascalToUnderline(string name)
        {
            var formatName = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var temp = name[i].ToString();
                if (Regex.IsMatch(temp, "[A-Z]"))
                {
                    if (i == 0)
                        temp = temp.ToLower();
                    else
                        temp = $"_{temp.ToLower()}";
                }
                formatName.Append(temp);
            }
            return formatName.ToString();
        }

        /// <summary>
        /// 下划线命名转帕斯卡命名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string UnderlineToPascal(string name)
        {
            var formatName = new StringBuilder();
            var nameArray = name.Split('_');
            foreach (var item in nameArray)
            {
                formatName.Append(item.Substring(0, 1).ToUpper() + item.Substring(1));
            }
            return formatName.ToString();
        }

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
                var attr = Attribute.GetCustomAttribute(entity, typeof(EntityAttribute)) as EntityAttribute;
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
            var attribute = Attribute.GetCustomAttribute(entity, typeof(EntityAttribute)) as EntityAttribute;
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

        #region 类属性读写
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
        #endregion

        #region 类属性读写（数据库字段名）
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

        #region 转 DataTable
        public static DataTable ParseToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(PascalToUnderline(prop.Name), Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[PascalToUnderline(prop.Name)] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static DataTable ParseToDataTable<T>(IList<T> data, DataColumnCollection columns)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (DataColumn item in columns)
            {
                var prop = properties.Find(item.ColumnName, true);
                table.Columns.Add(new DataColumn(PascalToUnderline(item.ColumnName), item.DataType));
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (DataColumn c in columns)
                {
                    row[c.ColumnName] = DBNull.Value;

                    var prop = properties.Find(UnderlineToPascal(c.ColumnName), true);
                    var propValue = prop?.GetValue(item);
                    if (propValue != null)
                    {
                        row[c.ColumnName] = Convert.ChangeType(propValue, c.DataType);
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
        #endregion
    }
}
