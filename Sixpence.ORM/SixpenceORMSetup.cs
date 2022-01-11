using Microsoft.AspNetCore.Builder;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.Common.Logging;
using Sixpence.ORM.Broker;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM
{
    public static class SixpenceORMSetup
    {
        public static IApplicationBuilder UseEntityGenerate(this IApplicationBuilder app)
        {
            var logger = LogFactory.GetLogger("entity");
            var broker = PersistBrokerFactory.GetPersistBroker();
            var dialect = broker.DbClient.Driver;
            var entityList = ServiceContainer.ResolveAll<IEntity>();

            broker.ExecuteTransaction(() =>
            {
                ServiceContainer.Resolve<IPreCreateEntities>()?.Execute(broker, entityList);

                entityList.Each(item =>
                {
                    if (broker.QueryCount(dialect.TableExsit(item.GetEntityName())) == 0)
                    {
                        ServiceContainer.Resolve<IPreCreateEntity>()?.Execute(broker, item); // 创建前

                        var attrSql = item
                            .GetColumns()
                            .Select(e =>
                            {
                                return $"{e.Name} {e.Type.GetDescription()}{(e.Length != null ? $"({e.Length.Value})" : "")} {(e.IsRequire.HasValue && e.IsRequire.Value ? "NOT NULL" : "")}{(e.Name == $"{item.GetPrimaryKey()}" ? " PRIMARY KEY" : "")}";
                            })
                            .Aggregate((a, b) => a + ",\r\n" + b);
                        broker.Execute($@"CREATE TABLE public.{item.GetEntityName()} ({attrSql})");

                        ServiceContainer.Resolve<IPostCreateEntity>()?.Execute(broker, item); // 创建后

                        logger.Info($"实体{item.GetLogicalName()}（{item.GetEntityName()}）创建成功");
                    }
                });

                ServiceContainer.Resolve<IPostCreateEntities>()?.Execute(broker, entityList);
            });

            return app;
        }
    }

    /// <summary>
    /// 创建实体前
    /// </summary>
    [ServiceRegister]
    public interface IPreCreateEntity
    {
        void Execute(IPersistBroker broker, IEntity entity);
    }

    /// <summary>
    /// 创建实体后
    /// </summary>
    [ServiceRegister]
    public interface IPostCreateEntity
    {
        void Execute(IPersistBroker broker, IEntity entity);
    }

    /// <summary>
    /// 创建所有实体前
    /// </summary>
    [ServiceRegister]
    public interface IPreCreateEntities
    {
        void Execute(IPersistBroker broker, IEnumerable<IEntity> entities);
    }

    /// <summary>
    /// 创建所有实体后
    /// </summary>
    [ServiceRegister]
    public interface IPostCreateEntities
    {
        void Execute(IPersistBroker broker, IEnumerable<IEntity> entities);
    }
}
