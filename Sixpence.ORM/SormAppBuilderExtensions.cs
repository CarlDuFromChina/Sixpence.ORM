using Microsoft.AspNetCore.Builder;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sixpence.ORM.Utils;
using Microsoft.Extensions.Options;
using Sixpence.ORM.Mappers;

namespace Sixpence.ORM
{
    public static class SormAppBuilderExtensions
    {
        internal static AppBuilderOptions BuilderOptions = new AppBuilderOptions();

        /// <summary>
        /// 添加 Sorm 中间件，并进行配置
        /// </summary>
        /// <param name="app"></param>
        /// <param name="migrate"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSorm(this IApplicationBuilder app, Action<AppBuilderOptions>? action)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
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
            var manager = app.ApplicationServices.GetRequiredService<IEntityManager>();
            var logger = loggerFactory?.CreateLogger(typeof(SormAppBuilderExtensions));

            var context = new EntityMigrationInterceptorContext()
            {
                EntityList = entityList,
                Manager = manager,
                Logger = logger
            };

            var driver = manager.DbClient.Driver;

            manager.ExecuteTransaction(() =>
            {
                // 迁移所有实体前
                context.Action = EntityMigrationAction.PreUpdateEntities;
                interceptor?.Execute(context);

                entityList.Each(item =>
                {
                    var entityMap = item.EntityMap;
                    var tableName = entityMap.Table;
                    var schema = entityMap.Schema;
                    var propertyMapList = entityMap.Properties.ToList();

                    var tableExsit = ConvertUtil.ConToBoolean(manager.ExecuteScalar(driver.Dialect.GetTableExsitSql(tableName)));

                    // 表未创建则创建，否则自动添加字段
                    if (!tableExsit)
                    {
                        // 创建实体前
                        context.Action = EntityMigrationAction.PreUpdateEntity;
                        context.CurrentEntity = item;
                        interceptor?.Execute(context);

                        var attrSql = propertyMapList
                            .Select(e =>
                            {
                                var lengthSQL = e.Length != null ? $"({e.Length})" : "";
                                var requireSQL = e.CanBeNull != true ? " NOT NULL" : "";
                                var uniqueSQL = e.IsUnique == true ? " UNIQUE" : "";
                                var defaultValueSQL = e.DefaultValue == null ? "" : e.DefaultValue is string ? $"DEFAULT '{e.DefaultValue}'" : $"DEFAULT {e.DefaultValue}";
                                var primaryKeySQL = e.Name == item.PrimaryColumn.DbPropertyMap.Name ? "PRIMARY KEY" : "";

                                return $"{e.Name} {e.DbType}{lengthSQL} {requireSQL} {uniqueSQL} {primaryKeySQL} {defaultValueSQL}";
                            })
                            .Aggregate((a, b) => a + ",\r\n" + b);
                        manager.Execute($@"CREATE TABLE {schema}.{tableName} ({attrSql})");

                        context.Action = EntityMigrationAction.PostUpdateEntity;
                        interceptor?.Execute(context);

                        if (BuilderOptions.EnableLogging)
                            logger?.LogDebug($"实体 {tableName} 创建成功");
                    }
                    else
                    {
                        var getTableColumnsSql = driver.Dialect.GetTableColumnsSql(tableName);
                        var columns = manager.Query<DbPropertyMap>(getTableColumnsSql).ToList();
                        var addColumns = new List<IDbPropertyMap>(); // 表需要添加的字段
                        var removeColumns = new List<IDbPropertyMap>(); // 表需要删除的字段

                        // 循环实体字段
                        propertyMapList.Each(attr =>
                        {
                            var _attr = columns.Find(e => e.Name.Equals(attr.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (_attr == null)
                            {
                                addColumns.Add(attr);
                            }
                        });

                        // 循环表字段
                        columns.Each(attr =>
                        {
                            var _attr = propertyMapList.Find(e => e.Name.Equals(attr.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (_attr == null)
                            {
                                removeColumns.Add(new DbPropertyMap() { Name = attr.Name });
                            }
                        });

                        // 删除字段
                        if (removeColumns.IsNotEmpty())
                            manager.Execute(driver.Dialect.GetDropColumnSql(tableName, removeColumns.Select(item => item.Name).ToList()));

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
