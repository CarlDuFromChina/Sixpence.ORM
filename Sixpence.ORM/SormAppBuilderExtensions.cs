using Microsoft.AspNetCore.Builder;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sixpence.ORM.Utils;
using Microsoft.Extensions.Options;

namespace Sixpence.ORM
{
    public static class SormAppBuilderExtensions
    {
        internal static AppBuilderOptions BuilderOptions = new AppBuilderOptions();

        /// <summary>
        /// 自动迁移实体类
        /// </summary>
        /// <param name="app"></param>
        /// <param name="migrate"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSorm(this IApplicationBuilder app, Action<AppBuilderOptions>? action)
        {
            ServiceContainer.Configure(app);
            
            action?.Invoke(BuilderOptions);

            if (BuilderOptions.MigrateDb)
                MigrateDB(app);

            return app;
        }

        /// <summary>
        /// 自动迁移实体类
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static void MigrateDB(this IApplicationBuilder app)
        {
            var interceptor = app.ApplicationServices.GetService<IEntityMigrationInterceptor>();
            var entityList = app.ApplicationServices.GetServices<IEntity>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(SormAppBuilderExtensions));

            var context = new EntityMigrationInterceptorContext() { EntityList = entityList };

            using (var manager = EntityManagerFactory.GetManager())
            {
                var driver = manager.DbClient.Driver;
                context.Manager = manager;

                manager.ExecuteTransaction(() =>
                {
                    // 迁移所有实体前
                    context.Action = EntityMigrationAction.PreUpdateEntities;
                    interceptor?.Execute(context);

                    entityList.Each(item =>
                    {
                        var tableName = item.GetEntityName();
                        var tableExsit = ConvertUtil.ConToBoolean(manager.ExecuteScalar(driver.Dialect.GetTableExsitSql(item.GetEntityName())));

                        // 表未创建则创建，否则自动添加字段
                        if (!tableExsit)
                        {
                            // 创建实体前
                            context.Action = EntityMigrationAction.PreUpdateEntity;
                            context.CurrentEntity = item;
                            interceptor?.Execute(context);

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

                            context.Action = EntityMigrationAction.PostUpdateEntity;
                            interceptor?.Execute(context);

                            if (BuilderOptions.EnableLogging)
                                logger?.LogDebug($"实体 {tableName} 创建成功");
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

                    context.Action = EntityMigrationAction.PostUpdateEntities;
                    interceptor?.Execute(context);
                });
            }
        }
    }
}
