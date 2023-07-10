using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数
    /// </summary>
    public static class EntityCommon
    {
        /// <summary>
        /// 生成唯一 ID
        /// </summary>
        /// <param name="primaryType"></param>
        /// <returns></returns>
        public static object GenerateID(PrimaryType primaryType = PrimaryType.GUID)
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
            switch (SormServiceCollectionExtensions.Options.EntityClassNameCase)
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

        /// <summary>
        /// 获取实体所有字段设置
        /// </summary>
        /// <returns></returns>
        public static List<ColumnOptions> GetColumns(IEntity entity, IDbDriver driver)
        {
            return entity
                .GetType()
                .GetProperties()
                .Where(item => item.IsDefined(typeof(ColumnAttribute), false))
                .Select(item =>
                {
                    var column = (item.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute).Options;
                    if (string.IsNullOrEmpty(column.Name))
                    {
                        column.Name = item.Name;
                    }
                    if (string.IsNullOrEmpty(column.Type))
                    {
                        if (driver.FieldMapping.GetFieldMappings().TryGetValue(item.PropertyType, out var type))
                        {
                            column.Type = type;
                        }
                        else
                        {
                            throw new Exception($"未找到类型 {item.PropertyType} 的映射");
                        }
                    }
                    if (item.IsDefined(typeof(DescriptionAttribute), false))
                    {
                        column.Remark = (item.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute)?.Description;
                    }
                    return column;
                })
                .ToList();
        }
    }
}
