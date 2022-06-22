using Microsoft.AspNetCore.Builder;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.Common.Logging;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Extensions
{
    public static class SixpenceORMBuilderExtension
    {
        internal static ORMOptions Options = new ORMOptions()
        {
            AutoGenerate = true,
            EntityClassNameCase = NameCase.Pascal, // 类名用帕斯卡命名
        };

        public static int EntityClassNameCase { get; internal set; }

        public static IApplicationBuilder UseORM(this IApplicationBuilder app, Action<ORMOptions> action = null)
        {
            action?.Invoke(Options);

            if (Options.AutoGenerate)
            {
                OpenEntityAutoGenerate();
            }

            return app;
        }

        /// <summary>
        /// ORM参数
        /// </summary>
        public class ORMOptions
        {
            /// <summary>
            /// 实体自动生成，默认为 True
            /// </summary>
            public bool AutoGenerate { get; set; }

            /// <summary>
            /// 实体类命名规范，默认帕斯卡命名（表名使用小写+下划线命名）
            /// </summary>
            public NameCase EntityClassNameCase { get; set; }
        }

        private static void OpenEntityAutoGenerate()
        {
            var logger = LoggerFactory.GetLogger("entity");
            var manager = EntityManagerFactory.GetManager();
            var dialect = manager.DbClient.Driver;
            var entityList = ServiceContainer.ResolveAll<IEntity>();

            manager.ExecuteTransaction(() =>
            {
                ServiceContainer.Resolve<IPreCreateEntities>()?.Execute(manager, entityList);

                entityList.Each(item =>
                {
                    if (manager.QueryCount(dialect.TableExsit(item.GetEntityName())) == 0)
                    {
                        ServiceContainer.Resolve<IPreCreateEntity>()?.Execute(manager, item); // 创建前

                        var attrSql = EntityCommon
                            .GetColumns(item, dialect)
                            .Select(e =>
                            {
                                var lengthSQL = e.Length != null ? $"({e.Length})" : "";
                                var requireSQL = e.IsRequire == true ? " NOT NULL" : "";
                                var defaultValueSQL = e.DefaultValue == null ? "" : e.DefaultValue is string ? $"DEFAULT '{e.DefaultValue}'" : $"DEFAULT {e.DefaultValue}";
                                var primaryKeySQL = e.Name == item.GetPrimaryColumn().Name ? "PRIMARY KEY" : "";

                                return $"{e.Name} {e.Type}{lengthSQL} {requireSQL} {primaryKeySQL} {defaultValueSQL}";
                            })
                            .Aggregate((a, b) => a + ",\r\n" + b);
                        manager.Execute($@"CREATE TABLE public.{item.GetEntityName()} ({attrSql})");

                        ServiceContainer.Resolve<IPostCreateEntity>()?.Execute(manager, item); // 创建后

                        logger.Info($"实体{item.GetLogicalName()}（{item.GetEntityName()}）创建成功");
                    }
                });

                ServiceContainer.Resolve<IPostCreateEntities>()?.Execute(manager, entityList);
            });
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
