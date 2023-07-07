﻿using Microsoft.AspNetCore.Builder;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.Common.Logging;
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
        public static ORMOptions Options;

        public static int EntityClassNameCase { get; internal set; }

        /// <summary>
        /// 使用 Sixpence.ORM，必须设置 ConnectionString 和 Driver
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action"></param>
        public static IApplicationBuilder UseORM(this IApplicationBuilder app, Action<ORMOptions> action = null)
        {
            Options = new ORMOptions()
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
            OpenEntityAutoGenerate();
            return app;
        }

        /// <summary>
        /// ORM参数
        /// </summary>
        public class ORMOptions
        {
            /// <summary>
            /// 实体类命名规范，默认帕斯卡命名（表名使用小写+下划线命名）
            /// </summary>
            public NameCase EntityClassNameCase { get; set; }

            /// <summary>
            /// 数据库驱动
            /// </summary>
            public IDbDriver Driver { get; set; }

            /// <summary>
            /// 数据库连接字符串
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// 超时时间
            /// </summary>
            public int CommandTimeout { get; set; }
        }

        private static void OpenEntityAutoGenerate()
        {
            var logger = LoggerFactory.GetLogger("migrations");
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

                            logger.Info($"实体 {tableName} 创建成功");
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
        }
    }

    /// <summary>
    /// 类命名规范
    /// </summary>
    public enum NameCase
    {
        /// <summary>
        /// 帕斯卡命名（UserInfo）
        /// </summary>
        Pascal,
        /// <summary>
        /// 下划线命名（user_info）
        /// </summary>
        UnderScore
    }
}
