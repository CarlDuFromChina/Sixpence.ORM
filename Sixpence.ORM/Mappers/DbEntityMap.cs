using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Mappers
{
    internal class DbEntityMap : IDbEntityMap
    {
        public DbEntityMap(IEntity entity, IDbDriver driver)
        {
            this.Table = EntityCommon.GetEntityTableName(entity);
            this.Schema = EntityCommon.GetEntitySchema(entity);
            this.Description = EntityCommon.GetEntityTableDescription(entity);
            this.Properties = GetMaps(entity, driver);
        }

        public string Table { get; set; }
        public string Schema { get; set; }
        public string Description { get; set; }
        public IList<IDbPropertyMap> Properties { get; set; }

        private List<IDbPropertyMap> GetMaps(IEntity entity, IDbDriver driver)
        {
            var maps = new List<IDbPropertyMap>();
            var properties = entity.GetType().GetProperties().Where(item => Attribute.IsDefined(item, typeof(ColumnAttribute)));

            foreach (var item in properties)
            {
                var column = (item.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute)?.Options;
                if (column != null)
                {
                    // 以特性定义字段名优先级最高
                    if (!string.IsNullOrEmpty(column.Name))
                    {
                        column.Name = column.Name;
                    }
                    else
                    {
                        column.Name = EntityCommon.ConvertToDbName(item.Name); // 转数据库字段名（下划线命名转换）
                    }

                    if (string.IsNullOrEmpty(column.DbType))
                    {
                        if (driver.FieldMapping.GetFieldMappings().TryGetValue(item.PropertyType, out var type))
                        {
                            column.DbType = type;
                        }
                        else
                        {
                            throw new Exception($"未找到类型 {item.PropertyType} 的映射");
                        }
                    }

                    if (item.IsDefined(typeof(DescriptionAttribute), false))
                    {
                        column.Remark = (item.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute)?.Description ?? "";
                    }

                    if (item.IsDefined(typeof(PrimaryColumnAttribute), false))
                    {
                        column.IsKey = true;
                    }
                    maps.Add(column);
                }
            }

            return maps;
        }
    }
}
