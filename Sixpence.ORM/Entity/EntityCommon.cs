using Sixpence.ORM.Interface;
using Sixpence.ORM.Mappers;
using Sixpence.ORM.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static long GenerateGuidNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// 比较类名和实体名
        /// </summary>
        /// <param name="className"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool CompareEntityName(string className, string tableName)
        {
            switch (SormServiceCollectionExtensions.Options.NameCase)
            {
                case NameCase.Pascal:
                    return tableName.Replace("_", "").Equals(className, StringComparison.OrdinalIgnoreCase);
                case NameCase.UnderScore:
                default:
                    return tableName.Equals(className, StringComparison.OrdinalIgnoreCase);
            }
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
        /// 大写字符转小写下划线
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string UpperChartToLowerUnderLine(string name)
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

        public static string GetEntityTableName(IEntity entity)
        {
            return GetEntityTableName(entity.GetType());
        }

        public static string GetEntityTableName(Type entity)
        {
            return _entityNameCache.GetOrAdd(entity.FullName, (key) =>
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
                    switch (SormServiceCollectionExtensions.Options.NameCase)
                    {
                        case NameCase.UnderScore:
                            return name.ToLower();
                        case NameCase.Pascal:
                        default:
                            return UpperChartToLowerUnderLine(name);
                    }
                }
                return attr.TableName;
            });
        }

        /// <summary>
        /// 根据设置命名规则转换成数据库名称，数据库字段和表名都需要转换
        /// 例如下划线命名 UserName，返回值 user_name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ConvertToDbName(string name)
        {
            switch (SormServiceCollectionExtensions.Options.NameCase)
            {
                case NameCase.UnderScore:
                    return name.ToLower();
                case NameCase.Pascal:
                default:
                    return UpperChartToLowerUnderLine(name);
            }
        }

        public static string GetEntitySchema(IEntity entity) => GetEntitySchema(entity.GetType());

        public static string GetEntitySchema(Type entity)
        {
            var attr = Attribute.GetCustomAttribute(entity, typeof(EntityAttribute)) as EntityAttribute;
            if (attr == null)
            {
                throw new Exception("获取实体名失败，请检查是否定义实体名");
            }
            return attr.Schema;
        }

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
