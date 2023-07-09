using Microsoft.AspNetCore.Builder;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sixpence.Common.Utils;

namespace Sixpence.ORM
{
    public static class SixpenceORMBuilderExtension
    {
        public static BuilderOptions? Options;

        /// <summary>
        /// 使用 Sixpence.ORM，必须设置 ConnectionString 和 Driver
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action"></param>
        public static IApplicationBuilder UseORM(this IApplicationBuilder app, Action<BuilderOptions>? action = null)
        {
            Options = new BuilderOptions()
            {
                EntityClassNameCase = NameCase.Pascal
            };

            action?.Invoke(Options);

            return app;
        }

        /// <summary>
        /// 自动迁移实体类
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMigrateDB(this IApplicationBuilder app)
        {
            var logDebug = Options?.LogOptions?.LogDebug;
            var logError = Options?.LogOptions?.LogError;

            using (var manager = EntityManagerFactory.GetManager())
            {
                var driver = manager.DbClient.Driver;
                var entityList = ServiceContainer.ResolveAll<IEntity>();

                manager.ExecuteTransaction(() =>
                {
                    ServiceContainer.Resolve<IPreCreateEntities>()?.Execute(manager, entityList);

                    entityList.Each(item =>
                    {
                        var tableName = item.GetEntityName();
                        var tableExsit = ConvertUtil.ConToBoolean(manager.ExecuteScalar(driver.Dialect.GetTableExsitSql(item.GetEntityName())));

                        // 表未创建则创建，否则自动添加字段
                        if (!tableExsit)
                        {
                            ServiceContainer.Resolve<IPreCreateEntity>()?.Execute(manager, item); // 创建前

                            var attrSql = EntityCommon
                                .GetColumns(item, driver)
                                .Select(e =>
                                {
                                    var lengthSQL = e.Length != null ? $"({e.Length})" : "";
                                    var requireSQL = e.IsRequire == true ? " NOT NULL" : "";
                                    var defaultValueSQL = e.DefaultValue == null ? "" : e.DefaultValue is string ? $"DEFAULT '{e.DefaultValue}'" : $"DEFAULT {e.DefaultValue}";
                                    var primaryKeySQL = e.Name == item.GetPrimaryColumn().Name ? "PRIMARY KEY" : "";

                                    return $"{e.Name} {e.Type}{lengthSQL} {requireSQL} {primaryKeySQL} {defaultValueSQL}";
                                })
                                .Aggregate((a, b) => a + ",\r\n" + b);
                            manager.Execute($@"CREATE TABLE public.{tableName} ({attrSql})");

                            ServiceContainer.Resolve<IPostCreateEntity>()?.Execute(manager, item); // 创建后

                            if (logDebug != null)
                                logDebug($"实体 {tableName} 创建成功");
                        }
                        else
                        {
                            var tableAttrs = driver.Dialect.GetTableColumns(manager.DbClient.DbConnection, tableName); // 查询表现有字段
                            var entityAttrs = EntityCommon.GetColumns(item, driver); // 查询实体现有字段
                            var addColumns = new List<ColumnOptions>(); // 表需要添加的字段
                            var removeColumns = new List<ColumnOptions>(); // 表需要删除的字段

                            // 循环实体字段
                            entityAttrs.Each(attr =>
                            {
                                var _attr = tableAttrs.Find(e => e.Name.Equals(attr.Name, StringComparison.CurrentCultureIgnoreCase));
                                if (_attr == null)
                                {
                                    addColumns.Add(attr);
                                }
                            });

                            // 循环表字段
                            tableAttrs.Each(attr =>
                            {
                                var _attr = entityAttrs.Find(e => e.Name.Equals(attr.Name, StringComparison.CurrentCultureIgnoreCase));
                                if (_attr == null)
                                {
                                    removeColumns.Add(new ColumnOptions() { Name = attr.Name });
                                }
                            });

                            // 删除字段
                            if (removeColumns.IsNotEmpty())
                                manager.Execute(driver.Dialect.GetDropColumnSql(tableName, removeColumns));

                            // 新增字段
                            if (addColumns.IsNotEmpty())
                                manager.Execute(driver.Dialect.GetAddColumnSql(tableName, addColumns));
                        }
                    });

                    ServiceContainer.Resolve<IPostCreateEntities>()?.Execute(manager, entityList);
                });
            }
            return app;
        }
    }
}
